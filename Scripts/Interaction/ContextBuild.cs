using System.Collections.Generic;
using UnityEngine;

public class ContextBuild : IContext
{
    List<GameObject> areaObjects = new List<GameObject>();
    
    int selectedTribeId = -1;
    Meta.Building selectedBuilding;
    int selectedMapId = -1;
    bool isAvailableChoice = false;
    public bool immediately = false;

    public void Init()
    {
    }
    public void Reset()
    {
        selectedMapId = -1;
        isAvailableChoice = false;
    }
    public void OnMove()
    {
        GameObject obj = Touch.Instance.GetTouchedObject3D();
        if(obj != null && obj.tag == MetaManager.Instance.GetTag(TAG.BOTTOM))
        {
            Clear();
            selectedMapId = Util.GetIntFromGameObjectName(obj.name);
            isAvailableChoice = CreateAreaCubes(selectedMapId, selectedBuilding.dimension);
        }
    }

    public void OnTouch()
    {
        //Debug.Log("[ContextBuild] OnTouch");
    }
    
    public void OnTouchRelease()
    {
        //Debug.Log("[ContextBuild] OnTouchRelease");
        if(isAvailableChoice)
        {
            Updater.Instance.AddQ(ActionType.BUILDING_CREATE, selectedTribeId, selectedMapId, selectedBuilding.id, null, immediately);
            Context.Instance.SetMode(Context.Mode.NONE);
            Clear();
        }
    }
    public void OnDrag()
    {
        
    }
    //----------------------------------------------------
    public void SetBuildingId(int tribeId, int id, bool immediately)
    {
        selectedTribeId = tribeId;
        selectedBuilding = MetaManager.Instance.buildingInfo[id];
        this.immediately = immediately;
    }
    private void Clear()
    {
        for(int n=0; n < areaObjects.Count; n++)
        {
            GameObjectPooling.Instance.Release(areaObjects[n].name, areaObjects[n]);
            //GameObject.DestroyImmediate(areaObjects[n]);
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
                
                string prefab = Context.Instance.greenPrefab;
                if(!MapManager.Instance.IsEmptyMapId(id))
                {
                    prefab = Context.Instance.redPrefab;
                    ret = false;
                }
                areaObjects.Add(
                    GameObjectPooling.Instance.Get(prefab, new Vector3(position.x, position.y + 0.1f, position.z), Quaternion.identity)
                    );
            }
            else
            {
                ret = false;
            }
            
        }
        return ret;
    }
}