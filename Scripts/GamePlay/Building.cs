using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//건물 정보
public class BuildingObject : Object
{  
    //디펜스 타워의 경우 상대가 움직이면 미사일이 따라가기 때문에 인식한 시점 위치로 날려야 한다. 
    struct DefenseTargetInfo
    {
        public Object target;
        public int targetMapId;
        public DefenseTargetInfo(Object target, int targetMapId)
        {
            this.target = target;
            this.targetMapId = targetMapId;
        }
    }
    float elapsedDefense;
    List<DefenseTargetInfo> defenseTargets = new List<DefenseTargetInfo>();
    bool defenseLock = false;

    public List<Actor> actors = new List<Actor>();
    /*
    from object
    amount
    */
    public override bool AddAction(QNode node)
    {
        Meta.Building meta =  MetaManager.Instance.buildingInfo[this.id];
        Action action = new Action(ActionType.MAX);

        switch(node.type)
        {
            case ActionType.BUILDING_CREATE:
                action = new Action(node.type, meta.level[this.level].buildTime, null, node.immediately);
                break;
            case ActionType.BUILDING_DEFENSE:
            {
                /*
                defenseTargets을 처리하면 된다.
                */
                action = new Action(node.type, meta.level[this.level].defense.speed);
                List<Transform> qList = new List<Transform>();
                for(int n=0; n< defenseTargets.Count; n++)
                {
                    qList.Add(MapManager.Instance.defaultGameObjects[defenseTargets[n].targetMapId].transform);
                }
                this.gameObject.GetComponent<IBuildingDefensing>().Rotation(qList);
                break;
            }
            case ActionType.BUILDING_UNDER_ATTACK:
            /*
            node.id: from
            node.values[0]: from TAG
            node.values[1]: amount
            */
                //Object obj = Util.GetObject(node.id, (TAG)node.values[0]);
                //일부러 null 체크 안함
                //underAttackQ.Enqueue(new UnderAttack(obj, node.values[1]));

                underAttackQ.Enqueue(new UnderAttack(node.requestInfo.fromObject, (int)node.requestInfo.amount));
                return true;
            case ActionType.BUILDING_DESTROY:
                //action = new Action(node.type, 2, new List<int>() { node.id, node.values == null ? (int)ActionType.MAX : node.values[0] }, node.immediately);
                action = new Action(node.type, node.requestInfo, 2);
                break;
        }
        actions.Add(action);
        return true;
    }
    
    public override bool Create(int tribeId, int mapId, int id, bool isInstantiate, float rotation)
    {
        this.currentMapId = mapId;
        this.elapsedDefense = 0;

        this.Init(tribeId, id, mapId, TAG.BUILDING, MetaManager.Instance.buildingInfo[id].level[0].HP, 0, rotation);

        if(isInstantiate)
        {
            Instantiate();
        }
        
        return true;
    }

    public override void Instantiate()
    {
        Meta.Building meta = MetaManager.Instance.buildingInfo[id];
        Instantiate(meta.level[level].prefab, false);
    }
    public void RemoveActor(int seq)
    {
        for(int n = 0; n < actors.Count; n++)
        {
            if(actors[n].seq == seq)
            {
                actors.RemoveAt(n);
                return;
            }
        }
    }

    public override void Update()
    {
        Meta.Building meta =  MetaManager.Instance.buildingInfo[this.id];

        List<int> removeActionIds = new List<int>();
        if(actions.Count > 0)
        {
            Action action = actions[0];
            //progress
            if(progress != null)
                progress.transform.position = GetProgressPosition(); //position

            if(action.immediately)
                action.currentTime = action.totalTime;
            else
                action.currentTime += Time.deltaTime;

            actions[0] = action;

            switch(action.type)
            {
                case ActionType.BUILDING_CREATE:
                    ShowProgress(action.currentTime, action.totalTime, true);
                    break;
                case ActionType.BUILDING_DEFENSE:
                    List<Vector3> posList = new List<Vector3>();
                    for(int n = 0; n < defenseTargets.Count; n++)
                    {
                        posList.Add(
                            MapManager.Instance.GetVector3FromMapId(defenseTargets[n].targetMapId)
                            );
                    }
                    IBuildingDefensing p = this.gameObject.GetComponent<IBuildingDefensing>();
                    p.Attack(posList, action.currentTime / action.totalTime);
                    
                    break;
            }

            //finish
            if(action.currentTime >= action.totalTime)
            {
                //removeActionIds.Add(n);
                switch(action.type)
                {
                    case ActionType.BUILDING_DEFENSE:
                        this.gameObject.GetComponent<IBuildingDefensing>().AttackEnd();
                        
                        //현재 범위안에 있으면 under attack
                        for(int n = 0; n < defenseTargets.Count; n++)
                        {
                            Object targetObj = defenseTargets[n].target;
                            float d = MapManager.Instance.GetDistance(this.mapId, targetObj.GetCurrentMapId());
                            if(d <= meta.level[this.level].defense.range)
                                Updater.Instance.AddQ(Util.GetUnderAttackActionType((TAG)MetaManager.Instance.GetTag(targetObj.gameObject.tag)), 
                                    targetObj.tribeId,
                                    targetObj.mapId, 
                                    this.mapId, 
                                    new List<int>() { (int)TAG.BUILDING, meta.level[this.level].defense.attack },
                                    false
                                    );
                        }
                        defenseLock = false;
                        break;
                    case ActionType.BUILDING_CREATE:
                        progress.SetActive(false);
                        Context.Instance.onCreationFinish(action.type, this);
                        break;
                    case ActionType.BUILDING_DESTROY:
                        Context.Instance.onCreationFinish(action.type, this);
                        //딸린 actor들 삭제
                        for(int n = 0; n < this.actors.Count; n++)
                        {
                            Actor actor = actors[n];
                            /*
                            Updater.Instance.AddQ(ActionType.ACTOR_DIE_FROM_DESTROYED_BUILDING
                                                , actor.tribeId
                                                , actor.mapId
                                                , action.values[0]
                                                , new List<int>() { action.values[1] }
                                                , action.immediately
                                                , 0);
                            */
                            QNode q = new QNode(ActionType.ACTOR_DIE_FROM_DESTROYED_BUILDING, actor.seq);
                            q.requestInfo.fromObject = action.requestInfo.fromObject;
                            Updater.Instance.AddQ(q);
                        }
                        actions.Clear();
                        
                        //BuildingManager.Instance.objects.Remove(mapId);
                        this.Release();
                        return;
                }
                actions.RemoveAt(0);
            }
        }
    }
    public override void UpdateUnderAttack()
    {
        Meta.Building meta = MetaManager.Instance.buildingInfo[this.id];
        while(underAttackQ.Count > 0)
        {
            UnderAttack p = underAttackQ.Dequeue();
            
            this.currentHP -= p.amount;

            //callback
            Context.Instance.onAttack(p.from, this, p.amount);

            ShowHP(meta.level[this.level].HP);
            if(this.currentHP <= 0)
            {
                HideProgress();
                underAttackQ.Clear();
                this.actions.Clear();
                /*
                Updater.Instance.AddQ(ActionType.BUILDING_DESTROY
                                    , this.tribeId
                                    , this.mapId
                                    , p.from.mapId
                                    , new List<int>() { (int)MetaManager.Instance.GetTag(p.from.gameObject.tag) }
                                    , false);
                */
                QNode q = new QNode(ActionType.BUILDING_DESTROY, this.seq);
                q.requestInfo.fromObject = p.from;
                Updater.Instance.AddQ(q);
                return;
            }
        }
    }
    public override void UpdateDefence()
    {
        if(defenseLock)
            return;
        
        //생성중에는 공격 못함
        if(IsCreating())
            return;

        if(defenseTargets.Count > 0)
            defenseTargets.Clear();

        Meta.Building meta = MetaManager.Instance.buildingInfo[this.id];
        if(meta.level[this.level].defense.range <= 0)
            return;

        elapsedDefense += Time.deltaTime;
        if(elapsedDefense < meta.level[this.level].defense.patrolTime)
            return;
        
        elapsedDefense = 0;

        if(this.HasActionType(ActionType.BUILDING_DEFENSE))
            return;

        List<GameObject> list = MapManager.Instance.GetFilledMapId(this.mapId, (int)meta.level[this.level].defense.range, new List<TAG>() { TAG.ENVIRONMENT, TAG.NEUTRAL });
        for(int n = 0; n < list.Count; n++)
        {
            int seq = Util.GetIntFromGameObjectName(list[n].name);
            Object obj = ObjectManager.Instance.Get(seq);
            //이미 죽었으면 패스
            if(obj.currentHP <= 0)
                continue;
            if(MapManager.Instance.GetDistance(this.mapId, obj.GetCurrentMapId()) > meta.level[this.level].defense.range)
                continue;

            if(Context.Instance.checkDefenseAttack(obj, this))
            {
                defenseTargets.Add(new DefenseTargetInfo(obj, obj.GetCurrentMapId()));
            }
            //Debug.Log(string.Format("{0} - {1}", list[n].name, list[n].tag));
        }

        if(defenseTargets.Count > 0)
        {
            Updater.Instance.AddQ(ActionType.BUILDING_DEFENSE, this.tribeId, this.mapId, this.id, null, false);
            defenseLock = true;
        }       
    }
    public override void UpdateEarning()
    {
        Meta.Building meta = GetMeta();
        if(meta.level[this.level].output == null || meta.level[this.level].output.Count == 0)
            return;

        earningElapse += Time.deltaTime;
        if(meta.level[this.level].earningTime <= earningElapse)
        {
            earningElapse = 0;
            bool success = GameStatusManager.Instance.Earn(this.tribeId, meta.level[this.level].output);
            Context.Instance.onEarning(this, success);
            this.gameObject.GetComponent<IBuilding>().Earning(success);
        }   
    }

    private Meta.Building GetMeta()
    {
        return MetaManager.Instance.buildingInfo[this.id];
    }
}
