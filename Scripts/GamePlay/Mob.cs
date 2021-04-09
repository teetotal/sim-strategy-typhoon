using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mob : ActingObject
{
    
    public override bool Create(int mapId, int id)
    {
        Meta.Mob mob = MetaManager.Instance.meta.mobs[id];
        this.Instantiate(mapId, id, mob.prefab, MetaManager.TAG.MOB);
        MapManager.Instance.SetMapId(mapId, mob.mapCost);
        
        return true;
    }

    public override void Update()
    {
        if(this.actions.Count == 0)
        {
            int from = this.mapId;
            SetMoving(MapManager.Instance.GetRandomNearEmptyMapId(this.mapId, MetaManager.Instance.meta.mobs[this.id].movingRange));
            int to = this.mapId;

            MobManager.Instance.mobs[to] = this;
            MobManager.Instance.mobs.Remove(from);

            return;
        }

        List<int> removeActionIds = new List<int>();
        for(int n = 0; n < actions.Count; n++)
        {
            Action action = actions[n];
            
            action.currentTime += Time.deltaTime;
            actions[n] = action;

            switch(action.type)
            {
                case ActionType.ACTOR_MOVING:
                    Moving(action);
                    break;
            }

            //finish
            if(action.currentTime >= action.totalTime)
            {
                removeActionIds.Add(n);
            }
        }

        RemoveActions(removeActionIds);
    }
}