
using System.Collections.Generic;
using UnityEngine;

public class ContextMob : IContext
{
    List<GameObject> areaObjects = new List<GameObject>();
    
    int selectedTribeId = -1;
    Meta.Mob selectedMob;
    int selectedMapId = -1;
    bool isAvailableChoice = false;

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
            isAvailableChoice = CreateAreaCubes(selectedMapId);
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
            //Updater.Instance.AddQ(ActionType.BUILDING_CREATE, selectedTribeId, selectedMapId, selectedBuilding.id, null);
            /*
            GameObject parent = MapManager.Instance.defaultGameObjects[selectedMapId];
            MapManager.Instance.CreateInstance(selectedMapId, 
                        selectedMob.prefab, 
                        parent.transform.position + new Vector3(0, 0.1f, 0), 
                        selectedMapId.ToString(),
                        TAG.ENVIRONMENT,
                        parent
                        );
            
            */
            MobManager.Instance.Create(selectedMapId, selectedMapId, selectedMob.id, 0, true);

            Context.Instance.SetMode(Context.Mode.NONE);
            Clear();
        }
    }
    public void OnDrag()
    {
        
    }
    //----------------------------------------------------
    public void SetMobId(int id)
    {
        selectedMob = MetaManager.Instance.mobInfo[id];
    }
    private void Clear()
    {
        for(int n=0; n < areaObjects.Count; n++)
        {
            GameObject.DestroyImmediate(areaObjects[n]);
        }
        areaObjects.Clear();
    }

    private bool CreateAreaCubes(int _id)
    {
        bool ret = true;
        Vector2Int defaultPos = MapManager.Instance.GetMapPosition(_id);
        List<Vector2Int> list = new List<Vector2Int>();
        
        Vector2Int pos = new Vector2Int(defaultPos.x, defaultPos.y);
        list.Add(pos);

        for(int n = 0; n < list.Count; n++)
        {
            int id = MapManager.Instance.GetMapId(list[n]);
            if(id != -1)
            {
                Vector3 position = MapManager.Instance.GetVector3FromMapId(id);
                
                GameObject obj = Context.Instance.greenPrefab;
                if(!MapManager.Instance.IsEmptyMapId(id))
                {
                    obj = Context.Instance.redPrefab;
                    ret = false;
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