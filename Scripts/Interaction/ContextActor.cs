using System;
using System.Collections.Generic;
using UnityEngine;
public class ContextCreatingActor : IContext
{
    public BuildingObject selectedBuilding;
    public void Init()
    {
        Reset();
    }
    public void Reset()
    {
        selectedBuilding = null;
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
    public void SetSelectedBuilding(BuildingObject selectedBuilding)
    {
        this.selectedBuilding = selectedBuilding;
    }
}
public class ContextActor : IContext
{
    public Object selectedObject;
    GameObject point;
    
    public void Init()
    {
        Reset();
    }
    public void Reset()
    {
        selectedObject = null;
    }

    public void OnMove()
    {
        Clear();

        Vector3 mousePosition = Input.mousePosition;
        Vector3 pos = Util.GetTouchedPosition(mousePosition);
        int mapId = MapManager.Instance.GetMapId(pos);

        Vector3 position = MapManager.Instance.GetVector3FromMapId(mapId);

        string prefab = Context.Instance.greenPrefab;    
        if(!MapManager.Instance.IsEmptyMapId(mapId))
        {
            prefab = Context.Instance.redPrefab;
        }
        point = GameObjectPooling.Instance.Get(prefab, new Vector3(position.x, position.y + 0.1f, position.z), Quaternion.identity);
        point.name = prefab;

        /*
        GameObject obj = Touch.Instance.GetTouchedObject3D();
        if(obj != null && obj.tag == MetaManager.Instance.GetTag(TAG.BOTTOM))
        {
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
        */
    }
    public void Clear()
    {
        if(point)
            GameObjectPooling.Instance.Release(point.name, point);
            //GameObject.DestroyImmediate(point);
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
            Vector3 mousePosition = Input.mousePosition;
            Vector3 pos = Util.GetTouchedPosition(mousePosition);
            int mapId = MapManager.Instance.GetMapId(pos);
            Debug.Log(string.Format("{0}, {1}, {2}", mousePosition, pos, mapId));

            Context.Instance.SetMode(Context.Mode.NONE);
            Context.Instance.onAction((Actor)selectedObject, obj, mapId);
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
    public void SetSelectedActor(Object obj)
    {
        selectedObject = obj;
    }
}