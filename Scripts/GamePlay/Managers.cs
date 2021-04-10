using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MetaManager
{
    public enum TAG
    {
        BOTTOM = 0,
        ENVIRONMENT,
        BUILDING,
        ACTOR,
        MOB,
        MAX
    }
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
    public string GetTag(TAG tag)
    {
        int idx = (int)tag;
        return meta.tags[idx];
    }
    public TAG GetTag(string tag)
    {
        for(int n = 0; n < meta.tags.Count; n++)
        {
            if(tag == meta.tags[n])
                return (TAG)n;
        }
        
        return TAG.MAX;
    }
}
public class MobManager
{
    public List<Mob> mobs = new List<Mob>();
    private static readonly Lazy<MobManager> hInstance = new Lazy<MobManager>(() => new MobManager());
    
    public static MobManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected MobManager()
    {
    }
    public void Regen()
    {
        int[] cnts = new int[MetaManager.Instance.meta.mobs.Count];
        for(int n = 0; n < mobs.Count; n++)
        {
            cnts[mobs[n].id]++;
        }

        for(int n = 0; n < MetaManager.Instance.meta.mobs.Count; n++)
        {
            Meta.Mob mob = MetaManager.Instance.meta.mobs[n];
            if(mob.max <= cnts[n])
                continue;
            
            //probability
            if(UnityEngine.Random.Range(0, mob.regenProbability) > 0)
                continue;

            int mapId = MapManager.Instance.GetEmptyMapId(); 
            if(mapId == -1)
                return;

            Mob obj = new Mob();
            if(obj.Create(mapId, n))
                mobs.Add(obj);
        }
    }
    public void Update()
    {
        //random gen
        Regen();

        for(int n = 0; n < mobs.Count; n++)
        {
            Mob mob = mobs[n];
            if(mob.actions.Count == 0)
            {
                //probability
                if(UnityEngine.Random.Range(0, MetaManager.Instance.meta.mobs[mob.id].movingProbability) == 0)
                    mob.SetMoving(MapManager.Instance.GetRandomNearEmptyMapId(mob.mapId, MetaManager.Instance.meta.mobs[mob.id].movingRange));
            }
            mob.Update();
        }
    }
}

public class ActorManager
{
    public Dictionary<int, Actor> actors = new Dictionary<int, Actor>();
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
        Actor obj = new Actor();
        if(obj.Create(mapId, actorId))
            actors[obj.mapId] = obj;
    }
    public void Moving(int mapId, int to)
    {
        Actor actor = ActorManager.Instance.actors[mapId];
        
        //mapmanager 변경. 
        MapManager.Instance.Move(mapId, to);
        //actormanager변경
        ActorManager.Instance.actors[to] = actor;
        ActorManager.Instance.actors.Remove(mapId);

        actor.SetMoving(to);

        //actor map id변경
        actor.mapId = to;
        GameObject parent = MapManager.Instance.defaultGameObjects[to];
        actor.gameObject.name = actor.mapId.ToString();
        actor.gameObject.transform.SetParent(parent.transform);
    }
    public void Update()
    {
        foreach(KeyValuePair<int, Actor> kv in actors)
        {
            kv.Value.Update();
            kv.Value.UpdateUIPosition();
        }
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
        //화면 처리에 필요한 object 설정
        BuildingObject obj = new BuildingObject();
        //map에 설정 & prefab생성. environment object를 map에 적절히 assign해야 해서 mapmanager에서 처리함
        obj.gameObject = MapManager.Instance.CreateBuilding(mapId, MetaManager.Instance.buildingInfo[buildingId].prefab); //건물의 a* cost는 -1. 지나가지 못함
            
        if(obj.Create(mapId, buildingId))
        {
            objects[obj.mapId] = obj;
        }
        
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