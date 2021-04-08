using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//자원
public struct Resource
{
    public int id;          //resource 고유 ID
    public string amount;   //소비량
}
//건물이 생산할 수 있는 액터 
public struct ResourceActor
{
    public ActorType type;
    public int level;
    public int amount;
}

//각종 행위. 공격, 이동
public struct Action
{
    public ActionType type;
    public float totalTime;       //Action이 적용되는 총 시간
    public float currentTime;     //현재까지 진행된 시간
}

/* --------------------------- */
//기본 struct
public abstract class Object
{
    public int mapId;     //이동시 업데이트 필요
    public int id;
    public int level;
    public List<Action> actions = new List<Action>(); //현재 겪고 있는 액션 리스트.
    public GameObject progress;
    public abstract bool Create(int mapId, int id);
    public abstract void Update();
    protected Vector3 GetProgressPosition()
    {
        return Camera.main.WorldToScreenPoint(MapManager.Instance.defaultGameObjects[mapId].transform.position + new Vector3(0, 1.0f, 0));
    }
    protected void RemoveActions(List<int> removeActionIds)
    {
        for(int n = 0; n < removeActionIds.Count; n++)
        {   
            actions.RemoveAt(removeActionIds[n]);
        }
    }
}
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
//움직이는 객체 정보
public class Actor : Object
{
    public ActorType type;
    public BuildingObject attachedBuilding; //소속된 건물 정보
    //전투의 경우, 수량 정보도 필요할 수 있음.
    public int currentHeadcount; //현재 인원. 전체 인원은 type과 level로 파악

    public override bool Create(int mapId, int id)
    {
        //생성 위치 찾기.
        mapId = MapManager.Instance.AssignNearEmptyMapId(mapId);
        if(mapId == -1)
            return false;

        //prefab 생성
        Vector3 position = MapManager.Instance.GetVector3FromMapId(mapId);
        GameObject obj = Resources.Load<GameObject>(MetaManager.Instance.actorInfo[id].prefab);
        obj = GameObject.Instantiate(obj, new Vector3(position.x, position.y + 0.1f, position.z), Quaternion.identity);
        obj.tag = "Actor";
        obj.name = string.Format("actor-{0}", mapId); //id어떻게 설정하지? seq를 할당해야 하나? 아님 현재 위치 기반으로 해야하나?
        GameObject parent = MapManager.Instance.defaultGameObjects[mapId];
        obj.transform.SetParent(parent.transform);

        return true;
    }

    public override void Update()
    {
        
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