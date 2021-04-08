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
        if(obj != null && obj.tag == MetaManager.Instance.GetTag(MetaManager.TAG.BOTTOM))
        {
            Clear();
            int id = int.Parse(obj.name.Replace("(Clone)", ""));
            Vector3 position = MapManager.Instance.GetVector3FromMapId(id);
            GameObject prefab = Context.Instance.greenPrefab;
            if(MapManager.Instance.GetBuildingObject(id) != null)
            {
                prefab = Context.Instance.redPrefab;
            }
            
            point = GameObject.Instantiate(prefab, new Vector3(position.x, position.y + 0.1f, position.z), Quaternion.identity);
            
        }
    }
    private void Clear()
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
        //이동중이면 어쩔거야?
        GameObject obj = Touch.Instance.GetTouchedObject3D();
        if(obj != null && obj.tag == MetaManager.Instance.GetTag(MetaManager.TAG.BOTTOM))
        {
            int target = int.Parse(obj.name.Replace("(Clone)", ""));
            if(target == selectedMapId || !MapManager.Instance.IsEmptyMapId(target))
            {
                Context.Instance.SetMode(Context.Mode.NONE);
                return;
            }
            Vector2Int from = MapManager.Instance.GetMapPosition(selectedMapId);
            Vector2Int to = MapManager.Instance.GetMapPosition(target);

            Debug.Log(string.Format("A* from {0} - {1}", from, to));

            //Astar test
            List<int> route = new List<int>();
            Astar astar = new Astar(MapManager.Instance.map);
            Stack<Astar.Pos> stack = astar.Search(new Astar.Pos(from.x, from.y), new Astar.Pos(to.x, to.y));
            if(stack == null)
                return;
            
            while(stack.Count > 0)
            {
                int id = MapManager.Instance.GetMapId(new Vector2Int(stack.Peek().x, stack.Peek().y));
                route.Add(id);
                stack.Pop();
            }
            Updater.Instance.AddQ(ActionType.ACTOR_MOVING, selectedMapId, -1, route);
        }
        Context.Instance.SetMode(Context.Mode.NONE);
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