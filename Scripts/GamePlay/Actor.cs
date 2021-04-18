using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Actor : ActingObject
{
    /*
    public BuildingObject attachedBuilding; //소속된 건물 정보
    //전투의 경우, 수량 정보도 필요할 수 있음.
    public int currentHeadcount; //현재 인원. 전체 인원은 type과 level로 파악
    */
    public override bool AddAction(QNode node)
    {
        Meta.Actor meta =  MetaManager.Instance.actorInfo[this.id];
        Action action = new Action();

        switch(node.type)
        {
            case ActionType.ACTOR_CREATE:
                action = new Action(ActionType.ACTOR_CREATE, node.immediately ? 0 : meta.level[this.level].createTime, null);
                if(node.values != null && node.values.Count == 1)
                    this.currentHP = node.values[0];
                break;
            case ActionType.ACTOR_MOVING:
            case ActionType.ACTOR_FLYING:
            {
                if(node.id == -1 || MapManager.Instance.GetCost(node.id) != MapManager.Instance.mapMeta.defaultVal.cost)
                    return false;

                //mapmanager 변경. 
                MapManager.Instance.Move(mapId, node.id);
                //actormanager변경
                ActorManager.Instance.actors[node.id] = this;
                ActorManager.Instance.actors.Remove(mapId);

                //Debug.Log(string.Format("current id: {0}, map id {1}", this.currentMapId, this.mapId));

                action = (node.type == ActionType.ACTOR_MOVING) ? 
                        GetMovingAction(node.id, meta.level[this.level].ability, node.type) : GetFlyingAction(node.id, meta.level[this.level].ability, node.type);
                if(action.type == ActionType.MAX)
                    return false;

                RemoveActionType(node.type); //이전 이동 액션을 제거
                //actions.Add(action);

                //actor map id변경
                this.mapId = node.id;
                GameObject parent = MapManager.Instance.defaultGameObjects[node.id];
                gameObject.name = this.mapId.ToString();
                gameObject.transform.SetParent(parent.transform);

                break;
            }
            case ActionType.ACTOR_ATTACK:
            {
                action = new Action(node.type, 1); //공격 속도
                break;
            }
            case ActionType.ACTOR_UNDER_ATTACK:
                action = new Action(node.type, 1, node.values);
                break;
            case ActionType.ACTOR_DIE:
                action = new Action(node.type, 2);
                break;
        }
        if(node.insertIndex != -1 && actions.Count > 0)
            actions.Insert(node.insertIndex, action);
        else
            actions.Add(action);
        return true;
    }
    
    public override bool Create(int mapId, int id)
    {
        //생성 위치 찾기.
        mapId = MapManager.Instance.AssignNearEmptyMapId(mapId);
        if(mapId == -1)
            return false;
        
        Meta.Actor meta = MetaManager.Instance.actorInfo[id];
        //prefab 생성
        this.Instantiate(mapId, id, meta.level[this.level].prefab, TAG.ACTOR, meta.flying);

        //HP
        this.currentHP = meta.level[0].ability.HP;

        //progress
        progress = GameObject.Instantiate(Context.Instance.progressPrefab, GetProgressPosition(), Quaternion.identity);
        progress.name = string.Format("progress-{0}-{1}", mapId, this.id);
        progress.transform.SetParent(Context.Instance.canvas);

        return true;
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
                        Attacking();
                        //attack
                        //죽었는지 확인
                        if(followObject.currentHP > 0)
                        {
                            //안죽었으면 계속 공격
                            //상대방 공격 당함
                            if(action.currentTime >= action.totalTime)
                            {
                                Updater.Instance.AddQ(
                                    ActionType.ACTOR_UNDER_ATTACK,
                                    followObject.mapId, 
                                    this.mapId,
                                    new List<int>() { meta.level[this.level].ability.attack },
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
                                this.mapId, 
                                -1,
                                null,
                                false,
                                -1
                                ),
                            //따라가기. 이동을 먼저 넣으면 mapid정보가 바뀌니까 뒤에 넣고 actions 순서를 바꾼다.
                            new QNode(meta.flying ? ActionType.ACTOR_FLYING : ActionType.ACTOR_MOVING,
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
                case ActionType.ACTOR_UNDER_ATTACK:
                    
                    this.currentHP -= action.values[0];

                    Debug.Log(string.Format("ACTOR_UNDER_ATTACK {0}/{1} - {2}", 
                        this.currentHP, meta.level[this.level].ability.HP, action.values[0]));
                    
                    ShowHP(meta.level[this.level].ability.HP);
                    if(this.currentHP <= 0)
                    {
                        Debug.Log("ACTOR_UNDER_ATTACK Die");
                        actions.Clear();
                        Updater.Instance.AddQ(ActionType.ACTOR_DIE, this.mapId, this.id, null, false);
                        return;
                    }
                    actions.RemoveAt(0);
                    return;
                case ActionType.ACTOR_DIE:
                    SetAnimation(ActionType.ACTOR_DIE);
                    if(action.currentTime >= action.totalTime)
                    {
                        //object삭제
                        GameObject.DestroyImmediate(this.gameObject);
                        ActorManager.Instance.actors.Remove(this.mapId);
                        MapManager.Instance.Remove(this.mapId);
                        DestroyProgress();
                        Clear();
                        return;
                    }
                    break;
            }

            //finish
            if(action.currentTime >= action.totalTime)
            {
                switch(action.type)
                {
                    case ActionType.ACTOR_CREATE:
                        progress.SetActive(false);
                        break;
                }
                actions.RemoveAt(0);
            }
        }
    }
}