using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MetaManager
{
    public Meta meta;
    public Dictionary<int, Meta.Building> buildingInfo = new Dictionary<int, Meta.Building>(); // 빌딩 id, 레벨별 
    public Dictionary<int, string> resourceInfo = new Dictionary<int, string>();         
    private static readonly Lazy<MetaManager> hInstance = new Lazy<MetaManager>(() => new MetaManager());
 
    public static MetaManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }

    protected MetaManager()
    {
    }
    public void Load()
    {
        meta = Json.LoadJsonFile<Meta>("meta");
        //buildingInfo
        for(int n = 0; n < meta.buildings.Count; n++)
        {
            Meta.Building b = meta.buildings[n];
            buildingInfo[b.id] = b;
        }
        //resourcesInfo
        for(int n = 0; n < meta.resources.Count; n++)
        {
            Meta.IdName r = meta.resources[n];
            resourceInfo[r.id] = r.name;
        }
    }
}

//노드
public class NodeManager
{
    public Dictionary<int, Node> nodes = new Dictionary<int, Node>();
    public List<string> names;
    private int currentIdx;
    public string GetName()
    {
        return names[currentIdx++];
    }

    public Object Create(int pos, string name)
    {
        Node node = new Node(pos, name);
        nodes[pos] = node;
        return node;
    }
    /*
    public Object Destroy(int pos)
    {
        
    }
    public Object Upgrade(int pos, int level)
    {
        
    }
    */
}

public class BuildingManager
{
    private Dictionary<int, BuildingObject> objects = new Dictionary<int, BuildingObject>();
    private static readonly Lazy<BuildingManager> hInstance = new Lazy<BuildingManager>(() => new BuildingManager());
    
    public static BuildingManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected BuildingManager()
    {
    }
    
    public void Construct(int mapId, int buildingId)
    {
        //map에 설정
        MapManager.Instance.CreateBuilding(mapId, MetaManager.Instance.buildingInfo[buildingId].prefab, -1); //건물의 a* cost는 9999로 일단 고정
        //화면 처리에 필요한 object 설정
        BuildingObject obj = new BuildingObject(mapId, buildingId);
        obj.SetConstruction();
        objects[mapId] = obj;
        
    }
    public void Update()
    {
        foreach(KeyValuePair<int, BuildingObject> kv in objects)
        {
            kv.Value.Update();
        }
    }
}

public class TimeManager
{
    public List<TimeNode> timeNodes = new List<TimeNode>(); //시대에 대한 정보
    public float currentTime;
    public int currentTimeNodeIndex;
    public float timeRatio; //1초당 얼마의 시간을 흘려보낼 것인가

    public string GetDateTimeString()
    {
        return ""; //(int)currentTime;
    }
}