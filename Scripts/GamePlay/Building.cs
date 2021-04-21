using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//건물 정보
public class BuildingObject : Object
{   
    float elapsedDefense;

    public List<Actor> actors = new List<Actor>();
    public override bool AddAction(QNode node)
    {
        Meta.Building meta =  MetaManager.Instance.buildingInfo[this.id];
        Action action = new Action(ActionType.MAX);

        switch(node.type)
        {
            case ActionType.BUILDING_DEFENSE:
            {
                /*
                node.id: target
                node.values[0]: TAG
                */
                action = new Action(node.type, meta.level[this.level].defense.speed, new List<int>(){ node.id, node.values[0] });

                Vector3 target = MapManager.Instance.GetVector3FromMapId(node.id);
                this.gameObject.GetComponent<IBuildingAttack>().Rotation(Quaternion.LookRotation(target));
                break;
            }
            case ActionType.BUILDING_UNDER_ATTACK:
            /*
            node.id: from
            node.values[0]: from TAG
            */
                Object obj = Util.GetObject(node.id, (TAG)node.values[0]);
                //일부러 null 체크 안함
                underAttackQ.Enqueue(new UnderAttack(obj, node.values[1]));
                return true;
            case ActionType.BUILDING_DESTROY:
                action = new Action(node.type, 2);
                break;
        }
        actions.Add(action);
        return true;
    }
    
    public override bool Create(int mapId, int id)
    {
        this.mapId = mapId;
        this.id = id;
        this.currentMapId = mapId;
        this.currentHP = MetaManager.Instance.buildingInfo[id].level[this.level].HP;
        this.level = 0;

        elapsedDefense = 0;

        //progress
        Vector3 pos = GetProgressPosition();
        progress = GameObject.Instantiate(Context.Instance.progressPrefab, GetProgressPosition(), Quaternion.identity);
        progress.name = string.Format("progress-{0}-{1}", mapId, this.id);
        progress.transform.SetParent(Context.Instance.canvas);

        return true;
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

            action.currentTime += Time.deltaTime;
            actions[0] = action;

            switch(action.type)
            {
                case ActionType.BUILDING_CREATE:
                    ShowProgress(action.currentTime, action.totalTime, true);
                    break;
                case ActionType.BUILDING_DEFENSE:
                    Vector3 pos = MapManager.Instance.GetVector3FromMapId(action.values[0]);

                    IBuildingAttack p = this.gameObject.GetComponent<IBuildingAttack>();
                    p.Attack(pos, action.currentTime / action.totalTime);
                    break;
                case ActionType.BUILDING_DESTROY:
                    if(action.currentTime >= action.totalTime)
                    {
                        //딸린 actor들 삭제
                        for(int n = 0; n < this.actors.Count; n++)
                        {
                            Actor actor = actors[n];
                            Updater.Instance.AddQ(ActionType.ACTOR_DIE, actor.mapId, actor.id, null, false, 0);
                        }
                        actions.Clear();
                        DestroyProgress();
                        BuildingManager.Instance.objects.Remove(mapId);
                        MapManager.Instance.DestroyBuilding(mapId);
                        return;
                    }
                    break;
            }

            //finish
            if(action.currentTime >= action.totalTime)
            {
                //removeActionIds.Add(n);
                switch(action.type)
                {
                    case ActionType.BUILDING_DEFENSE:
                        this.gameObject.GetComponent<IBuildingAttack>().AttackEnd();
                        //under attack
                        Updater.Instance.AddQ(Util.GetUnderAttackActionType((TAG)action.values[1]), 
                            action.values[0], 
                            this.mapId, 
                            new List<int>() { (int)TAG.BUILDING, meta.level[this.level].defense.attack },
                            false
                            );
                        break;
                    case ActionType.BUILDING_CREATE:
                        progress.SetActive(false);
                        break;
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
                Updater.Instance.AddQ(ActionType.BUILDING_DESTROY, this.mapId, this.id, null, false);
                return;
            }
        }
    }
    public override void UpdateDefence()
    {
        elapsedDefense += Time.deltaTime;
        if(elapsedDefense < 1)
            return;
        
        elapsedDefense = 0;

        if(this.HasActionType(ActionType.BUILDING_DEFENSE))
            return;

        Meta.Building meta = MetaManager.Instance.buildingInfo[this.id];
        List<GameObject> list = MapManager.Instance.GetFilledMapId(this.mapId, meta.level[this.level].defense.rage, new List<TAG>() { TAG.ENVIRONMENT, TAG.NEUTRAL });
        for(int n = 0; n < list.Count; n++)
        {
            int target = Util.GetIntFromGameObjectName(list[n].name);
            TAG tag = MetaManager.Instance.GetTag(list[n].tag);
            Object obj = Util.GetObject(target, tag);
            //이미 죽었으면 패스
            if(obj.currentHP <= 0)
                continue;
            //이동 중이면 패스
            switch(tag)
            {
                case TAG.MOB:
                    if(obj.HasActionType(ActionType.MOB_MOVING))
                        continue;
                    break;
                case TAG.ACTOR: 
                    if(obj.HasActionType(ActionType.ACTOR_MOVING))
                        continue;
                    break;
            }

            Updater.Instance.AddQ(ActionType.BUILDING_DEFENSE, this.mapId, target, new List<int>() { (int)tag }, false);
            //Debug.Log(string.Format("{0} - {1}", list[n].name, list[n].tag));
            break;
        }
    }
}
