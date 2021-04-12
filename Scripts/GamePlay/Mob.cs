using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mob : ActingObject
{
    public int attachedId;
    public override void AddAction(QNode node)
    {
        Meta.Mob meta =  MetaManager.Instance.mobInfo[this.id];

        switch(node.type)
        {
            case ActionType.MOB_CREATE:
                break;
            case ActionType.ACTOR_MOVING:
            case ActionType.ACTOR_FLYING:
            {
                if(node.id == -1)
                    return;

                //mapmanager 변경. 
                MapManager.Instance.Move(mapId, node.id);
                //actormanager변경
                MobManager.Instance.mobs[node.id] = this;
                MobManager.Instance.mobs.Remove(mapId);

                Action  action = (node.type == ActionType.MOB_MOVING) ? GetMovingAction(node.id, meta.ability, node.type) : GetFlyingAction(node.id, meta.ability, node.type);
                if(action.type == ActionType.MAX)
                    return;

                RemoveActionType(node.type); //이전 이동 액션을 제거
                actions.Add(action);
                isMovingStarted = false;

                //actor map id변경
                this.mapId = node.id;
                GameObject parent = MapManager.Instance.defaultGameObjects[node.id];
                gameObject.name = this.mapId.ToString();
                gameObject.transform.SetParent(parent.transform);

                break;
            }
            case ActionType.ACTOR_ATTACK:
                break;
        }
    }
    
    public override bool Create(int mapId, int id)
    {
        Meta.Mob mob = MetaManager.Instance.meta.mobs[id];
        this.Instantiate(mapId, id, mob.prefab, MetaManager.TAG.MOB, mob.flyingHeight > 0 ? true: false);
        MapManager.Instance.SetMapId(mapId, mob.mapCost);
        
        return true;
    }

    public override void Update()
    {
        List<int> removeActionIds = new List<int>();
        if(actions.Count > 0)
        {
            Action action = actions[0];
            
            action.currentTime += Time.deltaTime;
            actions[0] = action;

            Meta.Mob mob = MetaManager.Instance.meta.mobs[id];

            switch(action.type)
            {
                case ActionType.ACTOR_MOVING:
                    Moving(action);
                    break;
                case ActionType.ACTOR_FLYING:
                    Flying(action, mob.flyingHeight);
                    break;
            }

            //finish
            if(action.currentTime >= action.totalTime)
            {
                actions.RemoveAt(0);
            }
        }

        //RemoveActions(removeActionIds);
    }
}