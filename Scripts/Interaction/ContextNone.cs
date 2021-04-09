using UnityEngine.UI;
using UnityEngine;

public class ContextNone : IContext
{
    Vector3 posFirst;
    Quaternion v3Rotation;
    const float weight = 1.5f;
    GameObject selectUI, selectUIActor;
    GameObject selectedObj;

    public void Init()
    {
        v3Rotation = Camera.main.transform.rotation;
        //set select ui
        selectUI = GameObject.Instantiate(Context.Instance.selectUIPrefab);
        selectUI.transform.SetParent(Context.Instance.canvas);

        selectUIActor = GameObject.Instantiate(Context.Instance.selectUIActorPrefab);
        selectUIActor.transform.SetParent(Context.Instance.canvas);

        //for building
        Button[] btn = selectUI.GetComponentsInChildren<Button>();
        // X
        btn[0].onClick.AddListener(()=>{ OnClickX(); });

        // R
        btn[1].onClick.AddListener(()=>{ OnClickR(); });

        // B
        btn[2].onClick.AddListener(()=>{ OnClickB(); });

        //for actor
        selectUIActor.GetComponentInChildren<Button>().onClick.AddListener(()=>{ OnClickU();});

        Reset();
    }
    public void Reset()
    {
        selectUI.SetActive(false);
        selectUIActor.SetActive(false);
        selectedObj = null;
    }

    Vector3 GetDiff(Vector3 v3Direction)
    {
        return v3Rotation * (v3Direction * weight);
    }

    public void OnMove()
    {
        
    }

    public void OnTouch()
    {
        posFirst = Touch.Instance.GetTouchedPosition();
        //selectUI.SetActive(false);
    }
    
    public void OnTouchRelease()
    {
        GameObject obj = Touch.Instance.GetTouchedObject3D();
        if(obj)
        {
            //Debug.Log(string.Format("{0} - {1}", obj.tag, obj.name));
            switch(MetaManager.Instance.GetTag(obj.tag))
            {
                case MetaManager.TAG.BUILDING:
                case MetaManager.TAG.ACTOR:
                    selectedObj = obj;
                    SetUI();
                    break;
                default:
                    Reset();
                    break;
            }
        }
        else
        {
            Reset();
        }
        
    }
    public void OnDrag()
    {
        //Reset();
        //Debug.Log("[ContextNone] OnMove");
        Vector3 pos = Touch.Instance.GetTouchedPosition();

        Vector3 diff =  pos - posFirst;
        //Debug.Log("[ContextNone] OnMove" + diff.ToString() + " " + n++.ToString());
        //비율및 회전 방향 적용
        Vector3 v = GetDiff(diff);
        Camera.main.transform.position -= v;
        //카메라 위치 변경
        posFirst = Touch.Instance.GetTouchedPosition();
    }
    //---------------------
    private void SetUI()
    {
        int mapId = GetSelectedMapId();
        switch(MetaManager.Instance.GetTag(selectedObj.tag))
        {
            case MetaManager.TAG.BUILDING:
                selectUI.SetActive(true);
                selectUI.transform.position = Camera.main.WorldToScreenPoint(selectedObj.transform.position);
                selectUI.GetComponentInChildren<Text>().text = 
                        MetaManager.Instance.buildingInfo[GetSelectedBuildingId()].name;
                break;
            case MetaManager.TAG.ACTOR:
                selectUIActor.SetActive(true);
                selectUIActor.transform.position = Camera.main.WorldToScreenPoint(selectedObj.transform.position);
                selectUIActor.GetComponentInChildren<Text>().text = 
                        MetaManager.Instance.actorInfo[GetSelectedActorId()].name;
                Context.Instance.SetMode(Context.Mode.ACTOR);
                ((ContextActor)Context.Instance.contexts[Context.Mode.ACTOR]).SetSelectedActor(mapId);
                    break;
        }
    }
    int GetSelectedMapId()
    {
        return int.Parse(selectedObj.name.Replace("(Clone)", ""));
    }
    int GetSelectedBuildingId()
    {
        return BuildingManager.Instance.objects[GetSelectedMapId()].id;
    }
    int GetSelectedActorId()
    {
        return ActorManager.Instance.actors[GetSelectedMapId()].id;
    }

    void OnClickX()
    {
        if(selectedObj)
        {
            if(BuildingManager.Instance.objects[GetSelectedMapId()].actions.Count == 0)
                Updater.Instance.AddQ(ActionType.BUILDING_DESTROY, GetSelectedMapId(), GetSelectedBuildingId(), null);
            else
                Debug.Log("The Building has actions");
        }
    }
    void OnClickR()
    {
        if(selectedObj)
        {
            Vector3 angles = selectedObj.transform.localEulerAngles;
            selectedObj.transform.localEulerAngles = new Vector3(angles.x, angles.y + 90, angles.z);
        }
    }

    void OnClickB()
    {
        if(selectedObj)
        {
            int mapId = GetSelectedMapId();
            int buildingId = GetSelectedBuildingId();
            Context.Instance.onClickForCreatingActor(mapId, buildingId);
        }
    }
    void OnClickU()
    {
        if(selectedObj)
        {
            int mapId = GetSelectedMapId();
            int actorId = GetSelectedActorId();
            Context.Instance.onClickForUpgradingActor(mapId, actorId);
        }
    }
}