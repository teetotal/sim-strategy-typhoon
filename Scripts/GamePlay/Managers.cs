using System;
using System.Collections.Generic;
using UnityEngine;
public class MetaManager
{
    public Meta meta;
    public Dictionary<int, Meta.Environment> environmentInfo = new Dictionary<int, Meta.Environment>(); 
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
        environmentInfo.Clear();
        buildingInfo.Clear();
        actorInfo.Clear();
        mobInfo.Clear();
        resourceInfo.Clear();
        neutralInfo.Clear();


        meta = Json.LoadJsonFile<Meta>("meta");
        //environment
        for(int n = 0; n < meta.environments.Count; n++)
        {
            Meta.Environment e = meta.environments[n];
            environmentInfo[e.id] = e;
        }
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
    public List<Meta.IdQuantity> GetActorBooty(int actorId, int level)
    {
        return GetBootyList(MetaManager.Instance.actorInfo[actorId].level[level].booty);
    }
    public List<Meta.IdQuantity> GetMobBooty(int mobId)
    {
        return GetBootyList(MetaManager.Instance.mobInfo[mobId].booty);
    }
    private List<Meta.IdQuantity> GetBootyList(Meta.Booty booty)
    {
        List<Meta.IdQuantity> list = new List<Meta.IdQuantity>();

        int totalProbability = 0;
        for(int n = 0; n < booty.probability.Count; n++)
        {
            totalProbability += booty.probability[n];
        }

        for(int n = 0; n < booty.count; n++)
        {
            int winningNumber = UnityEngine.Random.Range(0, totalProbability);
            //Debug.Log(string.Format("winningNumber {0}, {1}", winningNumber, totalProbability));

            int sum = 0;
            for(int i = 0; i < booty.probability.Count; i++)
            {
                sum += booty.probability[i];
                if(sum >= winningNumber)
                {
                    list.Add(booty.items[i]);
                    break;
                }
            }
        }

        return list;
    }
}
/*
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
*/

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