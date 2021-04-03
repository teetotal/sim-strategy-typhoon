using System.Collections.Generic;
using UnityEngine;

public class ContextNone : IContext
{
    bool isDrag = false;
    Vector3 posFirst;
    Quaternion v3Rotation;
    const float weight = 1.5f;

    public void Init()
    {
        v3Rotation = Camera.main.transform.rotation;
    }

    Vector3 GetDiff(Vector3 v3Direction)
    {
        return v3Rotation * (v3Direction * weight);
    }

    public void OnMove()
    {
        if(isDrag)
        {
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
    }

    public void OnTouch()
    {
        if(!isDrag)
        {
            isDrag = true;
            posFirst = Touch.Instance.GetTouchedPosition();
            //Debug.Log("[ContextNone] OnTouch" + posFirst.ToString());
        }
    }
    
    public void OnTouchRelease()
    {
        if(isDrag)
        {
            isDrag = false;
            Vector3 pos = Touch.Instance.GetTouchedPosition();
            //Debug.Log("[ContextNone] OnTouchRelease" + pos.ToString());
        }
        
    }
}