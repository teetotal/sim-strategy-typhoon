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
            TAG tag = MetaManager.Instance.GetTag(selectedObj.tag);
            int mapId = GetSelectedMapId(); 
            int id = -1;
            //Debug.Log(string.Format("{0} - {1}", obj.tag, obj.name));
            switch(tag)
            {
                case TAG.BUILDING:
                    id = BuildingManager.Instance.objects[mapId].id;
                    break;
                case TAG.ACTOR:
                    Actor actor = ActorManager.Instance.actors[mapId];
                    id = actor.id;
                    if(actor.currentHP > 0)
                    {
                        Context.Instance.SetMode(Context.Mode.ACTOR);
                        ((ContextActor)Context.Instance.contexts[Context.Mode.ACTOR]).SetSelectedActor(mapId);
                    }
                    break;
                case TAG.BOTTOM:
                    id = mapId;
                    break;
                case TAG.MOB:
                    id = MobManager.Instance.mobs[mapId].id;
                    break;
                case TAG.NEUTRAL:
                    id = NeutralManager.Instance.objects[mapId].id;
                    break;
                case TAG.ENVIRONMENT:
                    id = MapManager.Instance.environments[mapId].id;
                    break;
            }
            Context.Instance.onSelectEvent(tag, mapId, id, selectedObj);
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
    private int GetSelectedMapId()
    {
        return Util.GetIntFromGameObjectName(selectedObj.name);
    }
}