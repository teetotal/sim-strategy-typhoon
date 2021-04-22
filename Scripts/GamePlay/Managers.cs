using System;
using System.Collections.Generic;
using UnityEngine;
public class MetaManager
{
    public Meta meta;
    public Dictionary<int, Meta.Building> buildingInfo = new Dictionary<int, Meta.Building>(); // 빌딩 정보
    public Dictionary<int, Meta.Actor> actorInfo = new Dictionary<int, Meta.Actor>(); // actor 정보
    public Dictionary<int, Meta.Mob> mobInfo = new Dictionary<int, Meta.Mob>(); // mob 정보
    public Dictionary<int, Meta.Neutral> neutralInfo = new Dictionary<int, Meta.Neutral>(); // 중립 건물 정보
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
        //neutralInfo
        for(int n = 0; n < meta.neutrals.Count; n++)
        {
            Meta.Neutral nu = meta.neutrals[n];
            neutralInfo[nu.id] = nu;
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
            if(obj.Create(q.tribeId, mapId, q.id))
            {
                mobs[obj.mapId] = obj;
            }

            //routine 추가
            obj.routine = new List<QNode>()
            {
                new QNode(
                    meta.flyingHeight == 0 ? ActionType.MOB_MOVING : ActionType.MOB_FLYING, 
                    -1, -1, -1, null, false, -1)
            };
            Context.Instance.onCreationEvent(q.type, TAG.MOB, obj.mapId, obj.id);
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
            if(meta.max == 0)
                continue;

            if(cnts.ContainsKey(meta.mapId) && cnts[meta.mapId].ContainsKey(meta.id) && meta.max < cnts[meta.mapId][meta.id])
                continue;

            Updater.Instance.AddQ(ActionType.MOB_CREATE, -1, meta.mapId, meta.id, null, true);
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
            list[n].UpdateUnderAttack();
            list[n].UpdateDefence();
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
            BuildingObject building = BuildingManager.Instance.objects[mapId];
            Actor obj = new Actor();
            if(obj.Create(building.tribeId, mapId, q.id))
            {
                mapId = obj.mapId; // 빈 공간으로 생성시킨다.
                //actor등록
                actors[mapId] = obj;
                //building에 actor등록
                building.actors.Add(obj);
                //actor에 building등록
                obj.attachedBuilding = building;
                Context.Instance.onCreationEvent(q.type, TAG.ACTOR, mapId, obj.id);
            }
            else
            {
                return;
            }
        }
        
        if(actors.ContainsKey(mapId) == false)
        {
            Debug.Log(string.Format("Invalid mapId. {0}", q.type.ToString()));
        }
        else
        {
            actors[mapId].AddAction(q);
        }
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
            list[n].UpdateUnderAttack();
            list[n].UpdateDefence();
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
        //destroy되기 전에 호출. 그래야 파티클을 보여주던 이벤트를 발생시켜도 아직 오브젝트가 살아있어야 뭘 하지
        if(q.type == ActionType.BUILDING_DESTROY)
            Context.Instance.onCreationEvent(q.type, TAG.BUILDING, q.mapId, q.id);

        switch(q.type)
        {
            case ActionType.BUILDING_CREATE:
                Construct(q);
                Context.Instance.onCreationEvent(q.type, TAG.BUILDING, q.mapId, q.id);
                break;
            default:
                if(objects.ContainsKey(q.mapId))
                {
                    objects[q.mapId].AddAction(q);
                }
                return;
        }
    }
    
    public void Construct(QNode q)
    {
        //화면 처리에 필요한 object 설정
        BuildingObject obj = new BuildingObject();
        //map에 설정 & prefab생성. environment object를 map에 적절히 assign해야 해서 mapmanager에서 처리함
        obj.gameObject = MapManager.Instance.CreateBuilding(q.mapId, MetaManager.Instance.buildingInfo[q.id].level[0].prefab); //건물의 a* cost는 -1. 지나가지 못함
            
        if(obj.Create(q.tribeId, q.mapId, q.id))
        {
            obj.actions.Add(new Action(ActionType.BUILDING_CREATE, q.immediately ? 0 : MetaManager.Instance.buildingInfo[q.id].level[0].buildTime, null));   
            objects[obj.mapId] = obj;

            //roatation
            if(q.values != null && q.values.Count == 1)
            {
                obj.gameObject.transform.localEulerAngles += new Vector3(0, q.values[0], 0);
            }
        }
    }
    public void Update()
    {
        List<BuildingObject> list = new List<BuildingObject>();
        foreach(KeyValuePair<int, BuildingObject> kv in objects)
        {
            list.Add(kv.Value);
        }

        for(int n = 0; n < list.Count; n++)
        {
            list[n].Update();
            list[n].UpdateUIPosition();
            list[n].UpdateUnderAttack();
            list[n].UpdateDefence();
        }
    }
}
public class NeutralManager
{
    public Dictionary<int, NeutralBuilding> objects = new Dictionary<int, NeutralBuilding>();
    private static readonly Lazy<NeutralManager> hInstance = new Lazy<NeutralManager>(() => new NeutralManager());
    
    public static NeutralManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected NeutralManager()
    {
    }

    public void Fetch(QNode q)
    {
        switch(q.type)
        {
            case ActionType.NEUTRAL_CREATE:
                Construct(q);
                break;
            case ActionType.NEUTRAL_DESTROY:
                Destroy(q.mapId);
                break;
            default:
                return;
        }
        Context.Instance.onCreationEvent(q.type, TAG.NEUTRAL, q.mapId, q.id);
    }
    
    public void Construct(QNode q)
    {
        //화면 처리에 필요한 object 설정
        NeutralBuilding obj = new NeutralBuilding();
        //map에 설정 & prefab생성. environment object를 map에 적절히 assign해야 해서 mapmanager에서 처리함
        obj.gameObject = MapManager.Instance.CreateNeutral(q.mapId, MetaManager.Instance.neutralInfo[q.id].prefab); //건물의 a* cost는 -1. 지나가지 못함
            
        if(obj.Create(q.tribeId, q.mapId, q.id))
        {
            obj.actions.Add(new Action(ActionType.NEUTRAL_CREATE, 0, null));   
            objects[obj.mapId] = obj;

            //roatation
            if(q.values != null && q.values.Count == 1)
            {
                obj.gameObject.transform.localEulerAngles += new Vector3(0, q.values[0], 0);
            }
        }
    }
    public void Destroy(int mapId)
    {
        MapManager.Instance.DestroyBuilding(mapId);
        objects.Remove(mapId);
    }
    public void Update()
    {
        foreach(KeyValuePair<int, NeutralBuilding> kv in objects)
        {
            kv.Value.Update();
            kv.Value.UpdateUIPosition();
            kv.Value.UpdateUnderAttack();
            kv.Value.UpdateDefence();
        }
    }
}
public class MarketManager
{
    public MarketStatus marketStatus;
    public Dictionary<int, Dictionary<int, float>> exchangeInfo = new Dictionary<int, Dictionary<int, float>>();
    private static readonly Lazy<MarketManager> hInstance = new Lazy<MarketManager>(() => new MarketManager());
    public static MarketManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected MarketManager()
    {
    }
    public void Load()
    {
        marketStatus = Json.LoadJsonFile<MarketStatus>("market_price");

        for(int n = 0; n < marketStatus.markets.Count; n++)
        {
            MarketStatus.Market market = marketStatus.markets[n];
            if(exchangeInfo.ContainsKey(market.mapId) == false)
            {
                exchangeInfo[market.mapId] = new Dictionary<int, float>();
            }
            for(int m = 0; m < market.exchanges.Count; m++)
            {
                exchangeInfo[market.mapId][market.exchanges[m].resourceId] = market.exchanges[m].rate;
            }
        }
    }
    public float GetExchangeRatio(int marketMapId, int resourceId)
    {
        if(!exchangeInfo.ContainsKey(marketMapId) || !exchangeInfo[marketMapId].ContainsKey(resourceId))
            return -1.0f;
        return exchangeInfo[marketMapId][resourceId];
    }
    public float Exchange(int marketMapId, int resourceId, float amount)
    {
        float ratio = GetExchangeRatio(marketMapId, resourceId);
        if(ratio == -1)
            return -1;
        return ratio * amount;
    }
    public int GetStandardResource()
    {
        return marketStatus.standardResourceId;
    }
}
public class GameStatusManager
{
    GameStatus gameStatus;
    /*
    TribeId,
    ResourceId
    amount
    */
    public Dictionary<int, Dictionary<int, float>> resourceInfo = new Dictionary<int, Dictionary<int, float>>();
    
    private static readonly Lazy<GameStatusManager> hInstance = new Lazy<GameStatusManager>(() => new GameStatusManager());
    public static GameStatusManager Instance { get { return hInstance.Value; } }
    protected GameStatusManager() {}
    public void Load()
    {
        gameStatus = Json.LoadJsonFile<GameStatus>("map_played");
        //tribes
        for(int n = 0; n < gameStatus.tribes.Count; n++)
        {
            //resources
            if(!resourceInfo.ContainsKey(n))
            {
                resourceInfo[n] = new Dictionary<int, float>();
            }

            GameStatus.Tribe tribe = gameStatus.tribes[n];
            for(int m = 0; m < tribe.resources.Count; m++)
            {
                GameStatus.ResourceIdAmount r = tribe.resources[m];
                resourceInfo[n][r.resourceId] = r.amount;
            }

            for(int m = 0; m < tribe.buildings.Count; m++)
            {
                GameStatus.Building building = tribe.buildings[n];
                Updater.Instance.AddQ(ActionType.BUILDING_CREATE, 
                building.tribeId,
                building.mapId, building.buildingId, new List<int>() {  (int)building.rotation }, true);

                for(int i = 0; i < building.actors.Count; i++)
                {
                    GameStatus.MapIdActorIdHP p = building.actors[n];
                    Updater.Instance.AddQ(ActionType.ACTOR_CREATE, n, building.mapId, p.actorId, new List<int>() { p.HP }, true);
                }
            }
        }
    }
    public float GetResource(int tribeId, int resourceId)
    {
        if(!resourceInfo[tribeId].ContainsKey(resourceId))
            return 0;
        return resourceInfo[tribeId][resourceId];
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