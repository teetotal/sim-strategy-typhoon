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
    public override bool AddAction(QNode node)
    {
        Meta.Actor meta =  MetaManager.Instance.actorInfo[this.id];
        Action action = new Action(ActionType.MAX);

        switch(node.type)
        {
            case ActionType.ACTOR_CREATE:
                action = new Action(ActionType.ACTOR_CREATE, node.immediately ? 0 : meta.level[this.level].createTime, null);
                if(node.values != null && node.values.Count == 1)
                    this.currentHP = node.values[0];
                break;
            case ActionType.ACTOR_MOVING_1_STEP:
            {
                if(node.id == -1 || MapManager.Instance.GetCost(node.id) != MapManager.Instance.mapMeta.defaultVal.cost)
                    return false;
                
                action = GetMovingAction(node.id, meta.level[this.level].ability, ActionType.ACTOR_MOVING, 3);
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
                if(node.id == -1)  
                    return false;

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

                break;
            }
            case ActionType.ACTOR_ATTACK:
            {
                action = new Action(node.type, meta.level[this.level].ability.attackSpeed); //공격 속도
                break;
            }
            case ActionType.ACTOR_UNDER_ATTACK:
            /*
            id: from mapId
            actions[0]: from TAG
            actions[1]: attack amount
            */
                Object obj = Util.GetObject(node.id, (TAG)node.values[0]);
                //일부러 null 체크 안함
                underAttackQ.Enqueue(new UnderAttack(obj, node.values[1]));
                break;
            case ActionType.ACTOR_DIE:
                action = new Action(node.type, 2);
                break;
            case ActionType.ACTOR_LOAD_RESOURCE:
                action = new Action(node.type, 1, new List<int>() { node.id });
                break;
            case ActionType.ACTOR_DELIVERY:
                action = new Action(node.type, 1, new List<int>() { node.id, node.values[0] });
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
        //actormanager변경
        ActorManager.Instance.actors[target] = this;
        ActorManager.Instance.actors.Remove(mapId);

        //actor map id변경
        this.mapId = target;
        GameObject parent = MapManager.Instance.defaultGameObjects[target];
        gameObject.name = this.mapId.ToString();
        gameObject.transform.SetParent(parent.transform);
    }
    
    public override bool Create(int tribeId, int mapId, int id)
    {
        //생성 위치 찾기.
        mapId = MapManager.Instance.AssignNearEmptyMapId(mapId);
        if(mapId == -1)
            return false;
        
        Meta.Actor meta = MetaManager.Instance.actorInfo[id];
        //prefab 생성
        this.Instantiate(tribeId, mapId, id, meta.level[this.level].prefab, TAG.ACTOR, meta.flying);

        //HP
        this.currentHP = meta.level[0].ability.HP;
        //level
        this.level = 0;

        //progress
        /*
        progress = GameObject.Instantiate(Context.Instance.progressPrefab, GetProgressPosition(), Quaternion.identity);
        progress.name = string.Format("progress-{0}-{1}", mapId, this.id);
        progress.transform.SetParent(Context.Instance.canvas);
        */

        return MapManager.Instance.SetCurrentMap(this, TAG.ACTOR); //currentMap에 등록
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
                    //거리 측정해서 공격 거리보다 멀면 
                    if(CheckAttacking(meta.level[this.level].ability))
                    {
                        ShowHP(meta.level[this.level].ability.HP);
                        //attack
                        //죽었는지 확인
                        if(followObject.currentHP > 0 && Attacking())
                        {
                            if(action.currentTime >= action.totalTime)
                            {
                                //상대방 공격 당함
                                TAG t = MetaManager.Instance.GetTag(followObject.gameObject.tag);
                                ActionType at = ActionType.MAX;
                                if(t == TAG.ACTOR)
                                    at = ActionType.ACTOR_UNDER_ATTACK;
                                else if(t == TAG.BUILDING)
                                    at = ActionType.BUILDING_UNDER_ATTACK;
                                
                                Updater.Instance.AddQ(
                                    at,
                                    followObject.tribeId,
                                    followObject.mapId, 
                                    this.mapId,
                                    new List<int>() { (int)TAG.ACTOR, meta.level[this.level].ability.attack },
                                    true
                                );
                                action.currentTime = 0;
                                actions[0] = action;
                            }
                            
                        }
                        else
                        {
                            Clear(true, true, false);
                            SetAnimation(ActionType.ACTOR_MAX);
                            this.HideProgress();
                        }
                        return;
                    } 
                    else 
                    {
                        //SetAnimation(ActionType.ACTOR_MAX);
                        this.Clear(true, false, true);
                        List<QNode> list = new List<QNode>()
                        {
                            //공격하기
                            new QNode(ActionType.ACTOR_ATTACK,
                                this.tribeId,
                                this.mapId, 
                                -1,
                                null,
                                false,
                                -1
                                ),
                            //따라가기. 이동을 먼저 넣으면 mapid정보가 바뀌니까 뒤에 넣고 actions 순서를 바꾼다.
                            new QNode(meta.flying ? ActionType.ACTOR_FLYING : ActionType.ACTOR_MOVING_1_STEP,
                                this.tribeId,
                                this.mapId, 
                                MapManager.Instance.GetRandomNearEmptyMapId(followObject.GetCurrentMapId(), (int)meta.level[this.level].ability.attackDistance),
                                null,
                                false,
                                0
                                ),
                        };
                        
                        Updater.Instance.AddQs(list);
                    }
                    return;
                }
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
                    case ActionType.ACTOR_DIE:
                        Context.Instance.onCreationFinish(action.type, this);
                        //object삭제
                        GameObject.DestroyImmediate(this.gameObject);
                        ActorManager.Instance.actors.Remove(this.mapId);
                        MapManager.Instance.Remove(this.mapId, TAG.ACTOR);
                        DestroyProgress();
                        Clear();
                        return;
                    case ActionType.ACTOR_LOAD_RESOURCE:
                        /*
                        values[0]:     적재할 리소스를 가지고 있는 빌딩 mapId
                        */
                        Context.Instance.onLoadResource(this, action.values[0]);
                        break;
                    case ActionType.ACTOR_DELIVERY:
                        /*
                        values[0]:   배송할 건물의 mapId
                        values[1]:   배송할 건물의 TAG
                        */
                        Context.Instance.onDelivery(this, action.values[0], (TAG)action.values[1]);
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
                Updater.Instance.AddQ(ActionType.ACTOR_DIE, this.tribeId, this.mapId, this.id, null, false);
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
            this.gameObject.GetComponent<IActor>().Earning(success);
        }
    }

    private Meta.Actor GetMeta()
    {
        return MetaManager.Instance.actorInfo[this.id];
    }
}