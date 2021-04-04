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
/* --------------------------- */
//기본 struct
public class Object
{
    public int mapId;     //node 위치와 building위치를 같게 할지 빌딩은 내부 위치로 나눠서 할지...
    public string name;
    public int level;
}
//각종 행위. 공격, 이동
public struct Action
{
    public ActionType type;
    public float totalTime;       //Action이 적용되는 총 시간
    public float currentTime;     //현재까지 진행된 시간
}
//건물 정보
public class BuildingObject : Object
{
    public Node attachedNode;               //소속된 node 정보
    public int buildingId;
    public List<Action> actions = new List<Action>(); //현재 겪고 있는 액션 리스트.
    public List<Actor> actors = new List<Actor>();
    public GameObject progress;
    public BuildingObject(int mapId, int buildingId)
    {
        this.mapId = mapId;
        this.buildingId = buildingId;
    }
    public void SetConstruction()
    {
        Action action = new Action();
        action.type = ActionType.BUILDING_CREATE;
        action.currentTime = 0;
        action.totalTime = MetaManager.Instance.buildingInfo[buildingId].buildTime;
        actions.Add(action);

        //progress
        Vector3 pos = GetProgressPosition();
        progress = GameObject.Instantiate(Context.Instance.progressPrefab, pos, Quaternion.identity);
        progress.name = string.Format("progress-{0}-{1}", mapId, buildingId);
        progress.transform.SetParent(Context.Instance.canvas);
    }

    public void Update()
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

        for(int n = 0; n < removeActionIds.Count; n++)
        {   
            actions.RemoveAt(removeActionIds[n]);
        }
    }
    private Vector3 GetProgressPosition()
    {
        return Camera.main.WorldToScreenPoint(MapManager.Instance.defaultGameObjects[mapId].transform.position + new Vector3(0, 1.0f, 0));
    }
}
//움직이는 객체 정보
public class Actor : Object
{
    public ActorType type;
    public Node attachedNode;               //소속된 node 정보
    public BuildingObject attachedBuilding; //소속된 건물 정보
    public Action action; //현재 액션
    public int targetPos;   //액션 타겟 위치. ex) 옆 건물로 이동시 pos가 from, targetPos가 to
    //전투의 경우, 수량 정보도 필요할 수 있음.
    public int currentHeadcount; //현재 인원. 전체 인원은 type과 level로 파악
}
//건물들이 모인 하나의 그룹. 지역
public class Node : Object
{
    public List<Node> connections;          // 연결된 노드들
    public Dictionary<int, int> necessaryResource;   // 주기적으로 필요한 자원 id, amount
    public Dictionary<int, int> currentResource;        // 현재 보유 자원
    public List<BuildingObject> builtObjects;   // 소속된 건물들

    public Node(int mapId, string name)
    {
        this.mapId = mapId;
        this.level = 1;
        this.name = name;
        connections = new List<Node>();
        necessaryResource = new Dictionary<int, int>();
        currentResource = new Dictionary<int, int>();
        builtObjects = new List<BuildingObject>();
    }

    public void Update()
    {
        //necessaryResource
        //currentResource
    }
}
/* --------------------------- */
public struct QNode
{
    public ActionType type;
    public int mapId; //mapid
    public int buildingId;
    public List<int> values;    //caller쪽과 protocol을 맞춰야 한다.

    public QNode(ActionType type, int mapId, int buildingId, List<int> values)
    {
        this.type = type;
        this.mapId = mapId;
        this.buildingId = buildingId;
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