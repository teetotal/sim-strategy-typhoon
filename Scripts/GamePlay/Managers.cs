using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MetaManager
{
    public Meta meta;
    public Dictionary<int, Meta.Building> buildingInfo = new Dictionary<int, Meta.Building>(); // 빌딩 정보
    public Dictionary<int, Meta.Actor> actorInfo = new Dictionary<int, Meta.Actor>(); // actor 정보
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
        //actorInfo
        for(int n = 0; n < meta.actors.Count; n++)
        {
            Meta.Actor b = meta.actors[n];
            actorInfo[b.id] = b;
        }
        //resourcesInfo
        for(int n = 0; n < meta.resources.Count; n++)
        {
            Meta.IdName r = meta.resources[n];
            resourceInfo[r.id] = r.name;
        }
    }
}

public class ActorManager
{
    private Dictionary<int, Actor> actors = new Dictionary<int, Actor>();
    private static readonly Lazy<ActorManager> hInstance = new Lazy<ActorManager>(() => new ActorManager());
    
    public static ActorManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected ActorManager()
    {
    }
    public void Create(int mapId, int actorId)
    {
        //화면 처리에 필요한 object 설정
        Actor obj = new Actor();
        if(obj.Create(mapId, actorId))
            actors[mapId] = obj;
    }
}

public class BuildingManager
{
    public Dictionary<int, Object> objects = new Dictionary<int, Object>();
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
        //map에 설정 & prefab생성. environment object를 map에 적절히 assign해야 해서 mapmanager에서 처리함
        MapManager.Instance.CreateBuilding(mapId, MetaManager.Instance.buildingInfo[buildingId].prefab); //건물의 a* cost는 -1. 지나가지 못함
        //화면 처리에 필요한 object 설정
        BuildingObject obj = new BuildingObject();
        if(obj.Create(mapId, buildingId))
            objects[mapId] = obj;
    }
    public void Destroy(int mapId)
    {
        MapManager.Instance.DestroyBuilding(mapId);
        objects.Remove(mapId);
    }
    public void Update()
    {
        foreach(KeyValuePair<int, Object> kv in objects)
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