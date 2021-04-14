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
    public Dictionary<int, Meta.Mob> mobInfo = new Dictionary<int, Meta.Mob>(); // mob 정보
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
        //mobInfo
        for(int n = 0; n < meta.mobs.Count; n++)
        {
            Meta.Mob b = meta.mobs[n];
            mobInfo[b.id] = b;
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
    float time;
    int lastRegenTime = 0;
    public Dictionary<int, Mob> mobs = new Dictionary<int, Mob>();
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
    public void Fetch(QNode q)
    {
        if(q.type == ActionType.MOB_CREATE)
        {   
            Meta.Mob meta = MetaManager.Instance.mobInfo[q.id];
            //probability
            if(!Util.Random(meta.regenProbability))
                return;

            int mapId = MapManager.Instance.GetRandomNearEmptyMapId(q.mapId, meta.movingRange); 
            if(mapId == -1)
                return;

            Mob obj = new Mob();
            obj.attachedId = q.mapId;   //소속 위치 
            if(obj.Create(mapId, q.id))
            {
                mobs[obj.mapId] = obj;
            }

            //routine 추가
            obj.routine = new List<QNode>()
            {
                new QNode(
                    meta.flyingHeight == 0 ? ActionType.MOB_MOVING : ActionType.MOB_FLYING, 
                    -1, -1, null)
            };
        }
        else
        {
            mobs[q.mapId].AddAction(q);
        }
    }

    public void Regen()
    {
        time += Time.deltaTime;
        int t = (int)time;
        if(lastRegenTime == t || t % 2 != 0)
            return;

        //소속된 위치 정보가 있어야 함
        Dictionary<int, Dictionary<int, int>> cnts = new Dictionary<int, Dictionary<int, int>>(); //소속 위치, mob id, count
        foreach(KeyValuePair<int, Mob> kv in mobs)
        {
            if(!cnts.ContainsKey(kv.Value.attachedId))
            {
                cnts[kv.Value.attachedId] = new Dictionary<int, int>();
                cnts[kv.Value.attachedId][kv.Value.id] = 0;
            }
                

            cnts[kv.Value.attachedId][kv.Value.id]++;
        }

        for(int n = 0; n < MapManager.Instance.mapMeta.mobs.Count; n++)
        {
            Map.Mob meta = MapManager.Instance.mapMeta.mobs[n];
            if(cnts.ContainsKey(meta.mapId) && cnts[meta.mapId].ContainsKey(meta.id) && meta.max <= cnts[meta.mapId][meta.id])
                continue;

            Updater.Instance.AddQ(ActionType.MOB_CREATE, meta.mapId, meta.id, null);
        }
    }
    public void Update()
    {
        List<Mob> list = new List<Mob>();
        foreach(KeyValuePair<int, Mob> kv in mobs)
        {
            list.Add(kv.Value);
        }

        for(int n = 0; n < list.Count; n++)
        {
            list[n].Update();
            list[n].UpdateUIPosition();
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
    public void Fetch(QNode q)
    {
        int mapId = q.mapId;
        if(q.type == ActionType.ACTOR_CREATE)
        {
            Actor obj = new Actor();
            if(obj.Create(mapId, q.id))
                actors[obj.mapId] = obj;

            mapId = obj.mapId; // 빈 공간으로 생성시킨다.
        }
        actors[mapId].AddAction(q);
    }
    
    public void Update()
    {
        List<Actor> list = new List<Actor>();
        foreach(KeyValuePair<int, Actor> kv in actors)
        {
            list.Add(kv.Value);
        }

        for(int n = 0; n < list.Count; n++)
        {
            list[n].Update();
            list[n].UpdateUIPosition();
        }
    }
}

public class BuildingManager
{
    public Dictionary<int, BuildingObject> objects = new Dictionary<int, BuildingObject>();
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

    public void Fetch(QNode q)
    {
        switch(q.type)
        {
            case ActionType.BUILDING_CREATE:
                Construct(q.mapId, q.id);
                break;
            case ActionType.BUILDING_DESTROY:
                Destroy(q.mapId);
                break;
            default:
                return;
        }
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
        foreach(KeyValuePair<int, BuildingObject> kv in objects)
        {
            kv.Value.Update();
            kv.Value.UpdateUIPosition();
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