using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mob : ActingObject
{
    public int attachedId;
    public override bool AddAction(QNode node)
    {
        switch(node.type)
        {
            case ActionType.MOB_CREATE:
                break;
            case ActionType.MOB_MOVING:
            case ActionType.MOB_FLYING:
            {
                Action action = GetMovingFlyingAction(node.type, node.id);
                if(action.type != ActionType.MAX)
                {
                    actions.Add(action);
                    return true;
                }
                
                return false;
            }
            case ActionType.MOB_ATTACK:
                break;
        }

        return true;
    }
    public Action GetMovingFlyingAction(ActionType type, int targetMapId)
    {
        Meta.Mob meta =  MetaManager.Instance.mobInfo[this.id];

        int to = targetMapId;
        if(to == -1)
        {
            to = MapManager.Instance.GetRandomNearEmptyMapId(this.mapId, meta.movingRange);
            if(to == -1)
                return new Action(ActionType.MAX, 0);
        }

        //MapManager 변경. 
        MapManager.Instance.Move(mapId, to);
        //MobManager변경
        MobManager.Instance.mobs[to] = this;
        MobManager.Instance.mobs.Remove(mapId);

        Action  action = (type == ActionType.MOB_MOVING) ? GetMovingAction(to, meta.ability, type) : GetFlyingAction(to, meta.ability, type);
        if(action.type == ActionType.MAX)
            return action;

        RemoveActionType(type); //이전 이동 액션을 제거
        isMovingStarted = false;

        //Mob map id변경
        this.mapId = to;
        GameObject parent = MapManager.Instance.defaultGameObjects[to];
        gameObject.name = this.mapId.ToString();
        gameObject.transform.SetParent(parent.transform);

        return action;
    }

    public override bool Create(int mapId, int id)
    {
        Meta.Mob mob = MetaManager.Instance.meta.mobs[id];
        this.Instantiate(mapId, id, mob.prefab, TAG.MOB, mob.flyingHeight > 0 ? true: false);
        MapManager.Instance.SetMapId(mapId, mob.mapCost);
        
        return true;
    }

    public override void Update()
    {
        if(actions.Count == 0)
        {
            if(Util.Random(MetaManager.Instance.mobInfo[this.id].movingProbability))
                ApplyRoutine();
        }

        List<int> removeActionIds = new List<int>();
        if(actions.Count > 0)
        {
            Action action = actions[0];
            
            action.currentTime += Time.deltaTime;
            actions[0] = action;

            Meta.Mob mob = MetaManager.Instance.meta.mobs[id];

            switch(action.type)
            {
                case ActionType.MOB_MOVING:
                    Moving(action);
                    break;
                case ActionType.MOB_FLYING:
                    Flying(action, mob.flyingHeight);
                    break;
            }

            //finish
            if(action.currentTime >= action.totalTime)
            {
                actions.RemoveAt(0);
            }
        }
        SetCurrentMapId();
    }
    public override void UpdateUnderAttack()
    {
        
    }

    public override void UpdateDefence()
    {

    }
}