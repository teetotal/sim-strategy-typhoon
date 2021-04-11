using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mob : ActingObject
{
    
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