using UnityEngine.UI;
using UnityEngine;

public class ContextNone : IContext
{
    Vector3 posFirst;
    Quaternion v3Rotation;
    const float weight = 1.5f;
    public GameObject selectedObj;

    public void Init()
    {
        v3Rotation = Camera.main.transform.rotation;
        Reset();
    }
    public void Reset()
    {
        /*
        Context.Instance.selectUIActorTop.SetActive(false);
        Context.Instance.selectUIActorBottom.SetActive(false);
        Context.Instance.selectUIBuildingTop.SetActive(false);
        Context.Instance.selectUIBuildingBottom.SetActive(false);
        */

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
        selectedObj = Touch.Instance.GetTouchedObject3D();
        if(selectedObj)
        {
            MetaManager.TAG tag = MetaManager.Instance.GetTag(selectedObj.tag);
            int mapId = GetSelectedMapId(); 
            int id = -1;
            //Debug.Log(string.Format("{0} - {1}", obj.tag, obj.name));
            switch(tag)
            {
                case MetaManager.TAG.BUILDING:
                    id = GetSelectedBuildingId();
                    break;
                case MetaManager.TAG.ACTOR:
                    id = GetSelectedActorId();
                    Context.Instance.SetMode(Context.Mode.ACTOR);
                    ((ContextActor)Context.Instance.contexts[Context.Mode.ACTOR]).SetSelectedActor(mapId);
                    //SetUI();
                    break;
                case MetaManager.TAG.BOTTOM:
                    id = mapId;
                    break;
                case MetaManager.TAG.MOB:
                    id = GetSelectedMobId();
                    break;
                case MetaManager.TAG.ENVIRONMENT:
                    //요거 처리해야함
                    break;
            }
            Context.Instance.onSelectEvent(tag, mapId, id);
            Reset();
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
    /*
    private void SetUI()
    {
        int mapId = GetSelectedMapId();
        switch(MetaManager.Instance.GetTag(selectedObj.tag))
        {
            case MetaManager.TAG.BUILDING:
                BuildingManager.Instance.objects[mapId].EnableUI(
                    MetaManager.Instance.buildingInfo[GetSelectedBuildingId()].name, 
                    Context.Instance.selectUIBuildingTop, 
                    Context.Instance.selectUIBuildingBottom);
                break;
            case MetaManager.TAG.ACTOR:
                Context.Instance.SetMode(Context.Mode.ACTOR);
                ((ContextActor)Context.Instance.contexts[Context.Mode.ACTOR]).SetSelectedActor(mapId);
                ActorManager.Instance.actors[mapId].EnableUI(
                    MetaManager.Instance.actorInfo[GetSelectedActorId()].name,
                    Context.Instance.selectUIActorTop, 
                    Context.Instance.selectUIActorBottom);
                break;
        }
    }
    */
    private int GetSelectedMapId()
    {
        return Util.GetIntFromGameObjectName(selectedObj.name);
    }
    private int GetSelectedBuildingId()
    {
        return BuildingManager.Instance.objects[GetSelectedMapId()].id;
    }
    private int GetSelectedActorId()
    {
        return ActorManager.Instance.actors[GetSelectedMapId()].id;
    }
    private int GetSelectedMobId()
    {
        return MobManager.Instance.mobs[GetSelectedMapId()].id;
    }
}