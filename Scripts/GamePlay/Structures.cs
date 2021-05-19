using System.Collections.Generic;
using UnityEngine;

public struct UnderAttack
{
    public Object from;
    public int amount;

    public UnderAttack(Object from, int amount)
    {
        this.from = from;
        this.amount = amount;
    }
}
//request 정보
public struct RequestInfo
{
    public int mySeq;
    public int targetMapId;
    public Object targetObject;
    public Object fromObject;
    public float amount;
}
public struct QNode
{
    public ActionType type;
    public RequestInfo requestInfo;
    public int tribeId;
    public int mapId; //mapid
    public int id;
    public bool immediately;
    public List<int> values;    //caller쪽과 protocol을 맞춰야 한다.
    public int insertIndex;

    public QNode(ActionType type, int tribeId, int mapId, int id, List<int> values, bool immediately, int insertIndex)
    {
        this.tribeId = tribeId;
        this.type = type;
        this.mapId = mapId;
        this.id = id;
        this.values = values;
        this.immediately = immediately;
        this.insertIndex = insertIndex;

        //---------------
        requestInfo = new RequestInfo();
    }
    public QNode(ActionType type, int seq)
    {
        this.tribeId = -1;
        this.type = type;
        this.mapId = -1;
        this.id = -1;
        this.values = null;
        this.immediately = false;
        this.insertIndex = -1;

        //---------------
        requestInfo = new RequestInfo();
        requestInfo.mySeq = seq;
    }
}
/* --------------------------- */

//각종 행위. 공격, 이동
public struct Action
{
    public ActionType type;
    public float totalTime;       //Action이 적용되는 총 시간
    public float currentTime;     //현재까지 진행된 시간
    public RequestInfo requestInfo;
    public List<int> values;      //기타 추가 정보. 이동시 A* route같은거 담는 용도
    public bool immediately;

    public List<Vector3> list;

    public Action(ActionType type, float totalTime = 0, List<int> values = null, bool immediately = false)
    {
        this.type = type;
        this.currentTime = 0;
        this.totalTime = totalTime;
        this.values = values;
        list = null;
        this.immediately = immediately;

        //-----------
        requestInfo = new RequestInfo();
    }
    public Action(ActionType type, RequestInfo requestInfo, float totalTime = 0)
    {
        this.type = type;
        this.currentTime = 0;
        this.totalTime = totalTime;
        this.values = null;
        list = null;
        this.immediately = false;

        //-----------
        this.requestInfo = requestInfo;
    }

    public void SetMovingRoute(Vector3 currentPosition)
    {
        //----------------------------
        if(type == ActionType.ACTOR_MOVING || type == ActionType.MOB_MOVING)
        {
            int totalStep = (this.values.Count - 2) * 2 + 2; //시작과 끝 = 2 + 중간 * 2
            list = new List<Vector3>();
            list.Add(currentPosition);  //첫 위치를 오브젝트의 현 위치로 한다.

            Vector3 prev = list[0];
            for(int n = 1; n < values.Count; n++)
            {
                Vector3 p = MapManager.Instance.GetVector3FromMapId(values[n]);//GetMapPosition(values[n]);
                list.Add( Util.AdjustY((prev + p) / 2, false) );
                
                list.Add( Util.AdjustY(p, false) );
                prev = p;
            }
        }
    }
    public float GetProgression()
    {
        if(this.totalTime == 0)
            return 0;
        if(this.currentTime >= this.totalTime)
            return this.values[this.values.Count - 1];

        return (this.currentTime / this.totalTime) * this.values.Count;
    }

    float GetProgress()
    {
        if(this.totalTime == 0)
            return 0;
        //if(this.currentTime > this.totalTime) return 1; //this.values[this.values.Count - 1];

        return (this.currentTime / this.totalTime);
    }

    public bool GetMovingProgression(ref Vector3 now, ref Vector3 next, ref float ratio)
    {
        if(list == null)
        {
            Debug.Log("list is null");
            return false;
        }

        float progression = GetProgress();

        if(progression >= 1)
        {
            now = list[list.Count-1];
            next = list[list.Count -1];
            ratio = 1;
            return false;
        }
        
        float f = (((float)list.Count -1) * progression);
        int idx = (int)(f);
        ratio = f % 1.0f;
        now = list[idx];

        if(idx <= list.Count-2)
            next = list[idx+1];
        else
            next = now;
        
        //Debug.Log(string.Format("{0} {1} {2} {3}/{4}", to, from, ratio, this.currentTime, this.totalTime));

        return true;
    }
}

/* --------------------------- */
//시대 정보
public struct TimeNode
{
    public string name; //시대 이름
    public int maxTime; //종료 시점
}
