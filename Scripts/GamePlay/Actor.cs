using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Actor : ActingObject
{
    
    public BuildingObject attachedBuilding; //소속된 건물 정보
    /*
    //전투의 경우, 수량 정보도 필요할 수 있음.
    public int currentHeadcount; //현재 인원. 전체 인원은 type과 level로 파악
    */
    /*
    target map id
    from object
    amount
    target object // load/delivery
    */
    public override bool AddAction(QNode node)
    {
        //생성중에는 아무것도 못함
        if(IsCreating())
            return false;

        Meta.Actor meta =  MetaManager.Instance.actorInfo[this.id];
        Action action = new Action(ActionType.MAX);

        switch(node.type)
        {
            case ActionType.ACTOR_CREATE:
                action = new Action(ActionType.ACTOR_CREATE, node.immediately ? 0 : meta.level[this.level].createTime, null, node.immediately);
                //if(node.values != null && node.values.Count == 1) this.currentHP = node.values[0];
                break;
            case ActionType.ACTOR_MOVING_1_STEP:
            {
                if(MapManager.Instance.GetCost(node.requestInfo.targetMapId) != MapManager.Instance.mapMeta.defaultVal.cost)
                {
                    //정확한 위치를 찍어서 들어 오기 때문에 이동할 수 없으면 튕긴다. 
                    return false;
                }
                
                action = GetMovingAction(node.requestInfo.targetMapId, meta.level[this.level].ability, ActionType.ACTOR_MOVING, 3);
                if(action.type == ActionType.MAX)
                    return false;
                
                RemoveActionType(node.type); //이전 이동 액션을 제거
                MoveMapId(action.values[action.values.Count - 1]);
                break;
            }
            case ActionType.ACTOR_MOVING:
            case ActionType.ACTOR_FLYING:
            {
                /*
                mapId: 사용 안함
                id:     target 위치
                */
                /*
                if(node.id == -1)  
                    return false;
                
                //Clear(true, false, false); //진행중인 행위 취소

                // 타겟 위치에 갈 수 없으면 근처로 이동.
                //정확히 그 위치에 가고 싶은건 위치 지정레벨에서 컨트롤
                if(MapManager.Instance.GetCost(node.id) != MapManager.Instance.mapMeta.defaultVal.cost)
                {
                    node.id = MapManager.Instance.GetRandomNearEmptyMapId(node.id, 1);
                    if(node.id == -1)
                        return false;
                }

                action = (node.type == ActionType.ACTOR_MOVING) ? 
                        GetMovingAction(node.id, meta.level[this.level].ability, node.type) : GetFlyingAction(node.id, meta.level[this.level].ability, node.type);
                if(action.type == ActionType.MAX)
                    return false;
                
                RemoveActionType(node.type); //이전 이동 액션을 제거

                MoveMapId(node.id);
                */
                if(MapManager.Instance.GetCost(node.requestInfo.targetMapId) != MapManager.Instance.mapMeta.defaultVal.cost)
                {
                    node.requestInfo.targetMapId = MapManager.Instance.GetRandomNearEmptyMapId(node.requestInfo.targetMapId, 1);
                    if(node.requestInfo.targetMapId== -1)
                        return false;
                }

                action = (node.type == ActionType.ACTOR_MOVING) ? 
                        GetMovingAction(node.requestInfo.targetMapId, meta.level[this.level].ability, node.type) : GetFlyingAction(node.requestInfo.targetMapId, meta.level[this.level].ability, node.type);
                if(action.type == ActionType.MAX)
                    return false;
                
                RemoveActionType(node.type); //이전 이동 액션을 제거
                MoveMapId(node.requestInfo.targetMapId);
                break;
            }
            case ActionType.ACTOR_ATTACK:
                action = new Action(node.type, meta.level[this.level].ability.attackSpeed); //공격 속도
                break;
            case ActionType.ACTOR_UNDER_ATTACK:
            /*
            id: from mapId
            actions[0]: from TAG
            actions[1]: attack amount
            */
                //Object obj = Util.GetObject(node.id, (TAG)node.values[0]);
                //일부러 null 체크 안함
                //underAttackQ.Enqueue(new UnderAttack(obj, node.values[1]));
                underAttackQ.Enqueue(new UnderAttack(node.requestInfo.fromObject, (int)node.requestInfo.amount));
                break;
            case ActionType.ACTOR_DIE_FROM_DESTROYED_BUILDING:
            case ActionType.ACTOR_DIE:
            /*
            id: from mapId
            actions[0]: from TAG
            */
                //action = new Action(node.type, 2, new List<int>() { node.id, node.values[0] }, node.immediately);
                action = new Action(node.type, node.requestInfo, 2);
                break;
            case ActionType.ACTOR_LOAD_RESOURCE:
                action = new Action(node.type, node.requestInfo, 1);//, new List<int>() { node.id });
                break;
            case ActionType.ACTOR_DELIVERY:
                action = new Action(node.type, node.requestInfo, 1); //, new List<int>() { node.id, node.values[0] });
                break;
        }
        if(node.insertIndex != -1 && actions.Count > 0)
            actions.Insert(node.insertIndex, action);
        else
            actions.Add(action);
        return true;
    }

    protected void MoveMapId(int target)
    {
        //mapmanager 변경. 
        MapManager.Instance.Move(mapId, target);
        //actor map id변경
        this.mapId = target;
        GameObject parent = MapManager.Instance.defaultGameObjects[target];
        
        //gameObject.transform.SetParent(parent.transform);
    }
    
    public override bool Create(int tribeId, int mapId, int id, bool isInstantiate, float rotation)
    {
        if(MapManager.Instance.GetCost(mapId) != MapManager.Instance.mapMeta.defaultVal.cost)
        {
            //생성 위치 찾기.
            mapId = MapManager.Instance.AssignNearEmptyMapId(mapId);
            if(mapId == -1)
                return false;
        }
        
        Meta.Actor meta = MetaManager.Instance.actorInfo[id];
        this.Init(tribeId, id, mapId, TAG.ACTOR, meta.level[0].ability.HP, 0, rotation);

        //prefab 생성
        if(isInstantiate)
        {
            Instantiate();
        }

        return MapManager.Instance.SetCurrentMap(this, TAG.ACTOR); //currentMap에 등록
    }

    public override void Instantiate()
    {
        Meta.Actor meta = MetaManager.Instance.actorInfo[id];
        Instantiate(meta.level[this.level].prefab, meta.flying);
    }
    public void Destroy()
    {
        this.attachedBuilding.RemoveActor(this.seq); //부모 빌딩에서 삭제   
        //ActorManager.Instance.actors.Remove(this.mapId);
        Clear();
        this.Release();
    }

    //동시에 진행 못하고 순차적으로함. 이래야 아래 같은 시퀀스가 가능해짐
    //창고에 간다 -> 물건을 실는다 -> 시장에 가서 판다.
    public override void Update()
    {
        ApplyRoutine();
        SetCurrentMapId();

        List<int> removeActionIds = new List<int>();
        if(actions.Count > 0)
        {
            Action action = actions[0];
            Meta.Actor meta = MetaManager.Instance.actorInfo[this.id];

            if(action.immediately)
                action.currentTime = action.totalTime;
            else
                action.currentTime += Time.deltaTime;
                
            actions[0] = action;

            switch(action.type)
            {
                case ActionType.ACTOR_CREATE:
                    ShowProgress(action.currentTime, action.totalTime, true);
                    break;
                case ActionType.ACTOR_MOVING:
                    if(!Moving(action))
                    {
                        actions.RemoveAt(0);
                        return;
                    }
                    break;
                case ActionType.ACTOR_FLYING:
                    if(!Flying(action, 1))
                    {
                        actions.RemoveAt(0);
                        return;
                    }
                    break;
                case ActionType.ACTOR_ATTACK:
                {
                    Attack(meta.level[this.level].ability, meta.flying);
                    return;
                }
                case ActionType.ACTOR_DIE_FROM_DESTROYED_BUILDING:
                case ActionType.ACTOR_DIE:
                    SetAnimation(ActionType.ACTOR_DIE);
                    break;
            }

            //finish
            if(action.currentTime >= action.totalTime)
            {
                switch(action.type)
                {
                    case ActionType.ACTOR_CREATE:
                        progress.SetActive(false);
                        Context.Instance.onCreationFinish(action.type, this);
                        break;
                    case ActionType.ACTOR_DIE_FROM_DESTROYED_BUILDING:
                    case ActionType.ACTOR_DIE:
                        //Context.Instance.onCreationFinish(action.type, this);
                        Context.Instance.onDie(action.type, this, action.requestInfo.fromObject);
                        Destroy();
                        return;
                    case ActionType.ACTOR_LOAD_RESOURCE:
                        /*
                        values[0]:     적재할 리소스를 가지고 있는 빌딩 mapId
                        */
                        Context.Instance.onLoadResource(this);
                        break;
                    case ActionType.ACTOR_DELIVERY:
                        /*
                        values[0]:   배송할 건물의 mapId
                        values[1]:   배송할 건물의 TAG
                        */
                        Context.Instance.onDelivery(this, action.requestInfo.targetObject);
                        break;
                }
                actions.RemoveAt(0);
            }
        }
    }
    public override void UpdateUnderAttack()
    {
        Meta.Actor meta = MetaManager.Instance.actorInfo[this.id];
        while(underAttackQ.Count > 0)
        {
            UnderAttack p = underAttackQ.Dequeue();
            
            this.currentHP -= p.amount;

            //callback
            Context.Instance.onAttack(p.from, this, p.amount);

            ShowHP(meta.level[this.level].ability.HP);
            if(this.currentHP <= 0)
            {
                HideProgress();
                underAttackQ.Clear();
                this.actions.Clear();
                /*
                Updater.Instance.AddQ(ActionType.ACTOR_DIE, this.tribeId, this.mapId, p.from.mapId
                    , new List<int>() { (int)MetaManager.Instance.GetTag(p.from.gameObject.tag) }
                    , false);
                */
                QNode q = new QNode(ActionType.ACTOR_DIE, this.seq);
                q.requestInfo.fromObject = p.from;
                Updater.Instance.AddQ(q);
                
                return;
            }

            //도망가기
            /*
            if(!this.HasActionType(ActionType.ACTOR_ATTACK))
                Updater.Instance.AddQ(
                    meta.flying ? ActionType.ACTOR_FLYING : ActionType.ACTOR_MOVING, 
                    this.mapId,
                    MapManager.Instance.GetRandomNearEmptyMapId(this.mapId, 2),
                    null,
                    false
                    );
            
            */
            return;

        }
    }
    public override void UpdateDefence()
    {

    }

    public override void UpdateEarning()
    {
        Meta.Actor meta = GetMeta();
        if(meta.level[this.level].wage == null || meta.level[this.level].wage.Count == 0)
            return;

        earningElapse += Time.deltaTime;
        if(meta.earningTime <= earningElapse)
        {
            earningElapse = 0;
            bool success = GameStatusManager.Instance.Spend(this.tribeId, meta.level[this.level].wage);
            Context.Instance.onEarning(this, success);
            IActor ia = this.gameObject.GetComponent<IActor>();
            if(ia != null)
                ia.Earning(success);
        }
    }

    private Meta.Actor GetMeta()
    {
        return MetaManager.Instance.actorInfo[this.id];
    }
}