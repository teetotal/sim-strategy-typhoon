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
    public override bool AddAction(QNode node, int insertIndex = -1)
    {
        Meta.Actor meta =  MetaManager.Instance.actorInfo[this.id];
        Action action = new Action();

        switch(node.type)
        {
            case ActionType.ACTOR_CREATE:
                action = new Action(ActionType.ACTOR_CREATE, node.immediately ? 0 : MetaManager.Instance.actorInfo[id].createTime, null);
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
                        GetMovingAction(node.id, meta.ability, node.type) : GetFlyingAction(node.id, meta.ability, node.type);
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
                TAG tag = (TAG)node.values[0];
                action = new Action(ActionType.ACTOR_ATTACK);
                
                switch(tag)
                {
                    case TAG.ACTOR:
                        this.followObject = ActorManager.Instance.actors[node.id];
                        break;
                    case TAG.MOB:
                        this.followObject = MobManager.Instance.mobs[node.id];
                        break;
                }
                //actions.Add(action);
                break;
            }
        }
        if(insertIndex != -1)
            actions.Insert(insertIndex, action);
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
        
        Meta.Actor actor = MetaManager.Instance.actorInfo[id];
        //prefab 생성
        this.Instantiate(mapId, id, actor.prefab, TAG.ACTOR, actor.flying);

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
            
            action.currentTime += Time.deltaTime;
            actions[0] = action;

            switch(action.type)
            {
                case ActionType.ACTOR_CREATE:
                    SetProgress(action.currentTime, action.totalTime, true);
                    break;
                case ActionType.ACTOR_MOVING:
                    Moving(action);
                    break;
                case ActionType.ACTOR_FLYING:
                    Flying(action, 1);
                    break;
                case ActionType.ACTOR_ATTACK:
                {
                    Meta.Actor meta = MetaManager.Instance.actorInfo[this.id];
                    //거리 측정해서 공격 거리보다 멀면 
                    if(CheckAttacking(meta.ability))
                    {
                        //attack
                        //죽었는지 확인
                        //안죽었으면 계속 공격
                        Attacking();
                    } 
                    else 
                    {
                        //따라가기 설정
                        AddAction(new QNode(meta.flying ? ActionType.ACTOR_FLYING : ActionType.ACTOR_MOVING, 
                            this.mapId, 
                            MapManager.Instance.GetRandomNearEmptyMapId(followObject.GetCurrentMapId(), (int)meta.ability.attackDistance), 
                            null, false), 0); 
                    }
                    return;
                }
                    
            }

            //finish
            if(action.currentTime >= action.totalTime)
            {
                switch(action.type)
                {
                    case ActionType.ACTOR_CREATE:
                        GameObject.Destroy(progress);
                        progress = null;
                        break;
                }
                actions.RemoveAt(0);
            }
        }
    }
}