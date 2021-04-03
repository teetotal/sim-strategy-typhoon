using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public int pos;     //node 위치와 building위치를 같게 할지 빌딩은 내부 위치로 나눠서 할지...
    public string name;
    public int level;
}
//각종 행위. 공격, 이동
public struct Action
{
    public ActionType type;
    public int totalTime;       //Action이 적용되는 총 시간
    public int currentTime;     //현재까지 진행된 시간
}
//건물 정보
public class BuildingObject : Object
{
    public Node attachedNode;               //소속된 node 정보
    public BuildingTypes type;
    public List<Action> actions; //현재 겪고 있는 액션 리스트.
    public List<Actor> actors;
    public BuildingObject(int pos, string name, BuildingTypes type)
    {
        this.pos = pos;
        this.level = 1;
        this.name = name;
        this.type = type;
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

    public Node(int pos, string name)
    {
        this.pos = pos;
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
    public int nodePos;
    public int buildingPos;
    public List<int> values;    //caller쪽과 protocol을 맞춰야 한다.

    public QNode(ActionType type, int node, int building, List<int> values)
    {
        this.type = type;
        this.nodePos = node;
        this.buildingPos = building;
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