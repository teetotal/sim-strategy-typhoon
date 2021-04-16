using System;
using System.Collections.Generic;
using UnityEngine;
public class ContextCreatingActor : IContext
{
    public int selectedMapId, selectedBuildId;
    public void Init()
    {
        Reset();
    }
    public void Reset()
    {
        selectedMapId = -1;
        selectedBuildId = -1;
    }

    public void OnMove()
    {
    }

    public void OnTouch()
    {
    }

    public void OnTouchRelease()
    {
    }
    public void OnDrag()
    {
    }
    //-------------------------------------
    public void SetSelectedBuilding(int mapId, int buildingId)
    {
        selectedMapId = mapId;
        selectedBuildId = buildingId;
    }
}
public class ContextActor : IContext
{
    public int selectedMapId;
    GameObject point;
    
    public void Init()
    {
        Reset();
    }
    public void Reset()
    {
        selectedMapId = -1;
    }

    public void OnMove()
    {
        GameObject obj = Touch.Instance.GetTouchedObject3D();
        if(obj != null && obj.tag == MetaManager.Instance.GetTag(TAG.BOTTOM))
        {
            Clear();
            int id = Util.GetIntFromGameObjectName(obj.name);
            Vector3 position = MapManager.Instance.GetVector3FromMapId(id);
            Vector2Int pos = MapManager.Instance.GetMapPosition(id);
            GameObject prefab = Context.Instance.greenPrefab;
            if(!MapManager.Instance.IsEmptyMapId(id))
            {
                prefab = Context.Instance.redPrefab;
            }
            
            point = GameObject.Instantiate(prefab, new Vector3(position.x, position.y + 0.1f, position.z), Quaternion.identity);
            
        }
    }
    public void Clear()
    {
        if(point)
            GameObject.DestroyImmediate(point);
        point = null;
    }

    public void OnTouch()
    {
    }

    public void OnTouchRelease()
    {
        Clear();
        
        GameObject obj = Touch.Instance.GetTouchedObject3D();
        if(obj != null)
        {
            Actor actor = ActorManager.Instance.actors[selectedMapId];
            Meta.Actor meta = MetaManager.Instance.actorInfo[actor.id];

            int target = Util.GetIntFromGameObjectName(obj.name);
            TAG tag = MetaManager.Instance.GetTag(obj.tag);

            if(tag == TAG.BOTTOM)
            {
                if(target != selectedMapId && MapManager.Instance.IsEmptyMapId(target))
                {
                    Updater.Instance.AddQ(
                        meta.flying ? ActionType.ACTOR_FLYING : ActionType.ACTOR_MOVING, 
                        selectedMapId, 
                        target, 
                        null,
                        false);
                }
                Context.Instance.SetMode(Context.Mode.NONE);
            } 
            else
            {
                Context.Instance.SetMode(Context.Mode.NONE);
                Context.Instance.onAction(selectedMapId, actor.id, tag, target);
            }
        } 
        else
        {
            Context.Instance.SetMode(Context.Mode.NONE);
        }
    }
    public void OnDrag()
    {
    }
    //-------------------------------------
    public void SetSelectedActor(int mapId)
    {
        selectedMapId = mapId;
    }
}