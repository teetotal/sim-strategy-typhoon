using System.Collections.Generic;
using UnityEngine;

public class ContextBuild : IContext
{
    List<GameObject> areaObjects = new List<GameObject>();
    GameObject green, red;

    public void Init()
    {
        green = Resources.Load<GameObject>("CubeGreen");
        red = Resources.Load<GameObject>("CubeRed");
    }
    public void OnMove()
    {
        GameObject obj = Touch.Instance.GetTouchedObject3D();
        if(obj != null && obj.tag == "Bottom")
        {
            Clear();
            int id = int.Parse(obj.name.Replace("(Clone)", ""));
            CreateAreaCubes(id, new Vector2Int(2, 2));
        }
    }

    public void OnTouch()
    {
        Debug.Log("[ContextBuild] OnTouch");
    }
    
    public void OnTouchRelease()
    {
        Debug.Log("[ContextBuild] OnTouchRelease");
    }

    private void Clear()
    {
        for(int n=0; n < areaObjects.Count; n++)
        {
            GameObject.DestroyImmediate(areaObjects[n]);
        }
        areaObjects.Clear();
    }

    private bool CreateAreaCubes(int _id, Vector2Int size)
    {
        bool ret = true;
        Vector2Int defaultPos = MapManager.Instance.GetMapPosition(_id);
        List<Vector2Int> list = new List<Vector2Int>();
        
        for(int y = 0; y < size.y; y++)
        {
            for(int x = 0; x < size.x; x++)
            {
                Vector2Int pos = new Vector2Int(defaultPos.x + x, defaultPos.y + y);
                list.Add(pos);
            }
        }

        for(int n = 0; n < list.Count; n++)
        {
            int id = MapManager.Instance.GetMapId(list[n]);
            if(id != -1)
            {
                Vector3 position = MapManager.Instance.GetVector3FromMapId(id);
                GameObject obj = green;
                if(MapManager.Instance.GetBuildingObject(id) != null)
                {
                    obj = red;
                }
                areaObjects.Add(GameObject.Instantiate(obj, new Vector3(position.x, position.y + 0.1f, position.z), Quaternion.identity));
            }
            else
            {
                ret = false;
            }
            
        }
        return ret;
    }
}