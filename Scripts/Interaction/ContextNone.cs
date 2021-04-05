using UnityEngine.UI;
using UnityEngine;

public class ContextNone : IContext
{
    Vector3 posFirst;
    Quaternion v3Rotation;
    const float weight = 1.5f;
    GameObject selectUI;
    GameObject selectedObj;

    public void Init()
    {
        v3Rotation = Camera.main.transform.rotation;
        //set select ui
        GameObject obj = Context.Instance.selectUIPrefab;
        selectUI = GameObject.Instantiate(obj);
        selectUI.transform.SetParent(Context.Instance.canvas);

        Button[] btn = selectUI.GetComponentsInChildren<Button>();
        // X
        btn[0].onClick.AddListener(()=>{ OnClickX(); });

        // R
        btn[1].onClick.AddListener(()=>{ OnClickR(); });

        selectUI.SetActive(false);


    }
    public void Reset()
    {
        selectUI.SetActive(false);
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
        if(obj && obj.tag == "Building")
        {
            selectUI.SetActive(true);
            selectedObj = obj;
            SetUI();
            
            //Debug.Log(string.Format("{0} - {1}", obj.tag, obj.name));
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
        selectUI.transform.position = Camera.main.WorldToScreenPoint(selectedObj.transform.position);
        int mapId = GetSelectedMapId();
        int buildingId = GetSelectedBuildingId();
        selectUI.GetComponentInChildren<Text>().text = 
                MetaManager.Instance.buildingInfo[buildingId].name;
        
    }
    int GetSelectedMapId()
    {
        return int.Parse(selectedObj.name.Replace("(Clone)", ""));
    }
    int GetSelectedBuildingId()
    {
        return BuildingManager.Instance.objects[GetSelectedMapId()].buildingId;
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
}