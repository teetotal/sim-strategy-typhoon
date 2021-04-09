using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//각종 행위. 공격, 이동
public struct Action
{
    public ActionType type;
    public float totalTime;       //Action이 적용되는 총 시간
    public float currentTime;     //현재까지 진행된 시간
    public List<int> values;      //기타 추가 정보. 이동시 A* route같은거 담는 용도
}

/* --------------------------- */
//건물 정보
public class BuildingObject : Object
{   
    public List<Actor> actors = new List<Actor>();
    
    public override bool Create(int mapId, int id)
    {
        this.mapId = mapId;
        this.id = id;

        Action action = new Action();
        action.type = ActionType.BUILDING_CREATE;
        action.currentTime = 0;
        action.totalTime = MetaManager.Instance.buildingInfo[this.id].buildTime;
        actions.Add(action);

        //progress
        Vector3 pos = GetProgressPosition();
        progress = GameObject.Instantiate(Context.Instance.progressPrefab, pos, Quaternion.identity);
        progress.name = string.Format("progress-{0}-{1}", mapId, this.id);
        progress.transform.SetParent(Context.Instance.canvas);

        return true;
    }

    public override void Update()
    {
        List<int> removeActionIds = new List<int>();
        for(int n = 0; n < actions.Count; n++)
        {
            Action action = actions[n];
            //progress
            if(progress != null)
                progress.transform.position = GetProgressPosition(); //position

            action.currentTime += Time.deltaTime;
            actions[n] = action;

            switch(action.type)
            {
                case ActionType.BUILDING_CREATE:
                    progress.GetComponent<Slider>().value = action.currentTime / action.totalTime;
                    //Debug.Log(string.Format("{0}-{1}/{2}", mapId, action.currentTime, action.totalTime));
                    break;
            }

            //finish
            if(action.currentTime >= action.totalTime)
            {
                removeActionIds.Add(n);
                switch(action.type)
                {
                    case ActionType.BUILDING_CREATE:
                        GameObject.Destroy(progress);
                        progress = null;
                        break;
                }
            }
        }

        RemoveActions(removeActionIds);
    }
}

/* --------------------------- */
public struct QNode
{
    public ActionType type;
    public int mapId; //mapid
    public int id;
    public List<int> values;    //caller쪽과 protocol을 맞춰야 한다.

    public QNode(ActionType type, int mapId, int id, List<int> values)
    {
        this.type = type;
        this.mapId = mapId;
        this.id = id;
        this.values = values;
    }
}
/* --------------------------- */
//시대 정보
public struct TimeNode
{
    public string name; //시대 이름
    public int maxTime; //종료 시점
}