using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameStatus
{
    [Serializable]
    public class MapIdBuildingId
    {
        public int mapId;
        public int buildingId;
        public float rotation;
    }
    [Serializable]
    public class MapIdActorIdHP
    {
        public int mapId;
        public int actorId;
        public int HP;
    }
    [Serializable]
    public class ResourceIdAmount
    {
        public int resourceId;
        public int amount;
    }
    [Serializable]
    public class Building : MapIdBuildingId
    {
        public List<MapIdActorIdHP> actors;
        public int tribeId; //json에서 로드되지 않고 load 시점에 할당 됨
    }
    [Serializable]
    public class Tribe
    {
        public List<ResourceIdAmount> resources;
        public List<Building> buildings;
    }
    public List<Tribe> tribes;

    /*
    TribeId,
    ResourceId
    amount
    */
    public Dictionary<int, Dictionary<int, int>> resourceInfo = new Dictionary<int, Dictionary<int, int>>();
    /*
    mapId,
    Bilding Info
    */
    public Dictionary<int, Building> buildingInfo = new Dictionary<int, Building>();
    public static GameStatus Load(string jsonFileName)
    {
        GameStatus gameStatus = Json.LoadJsonFile<GameStatus>(jsonFileName);
        //tribes
        for(int n = 0; n < gameStatus.tribes.Count; n++)
        {
            
            //resources
            if(!gameStatus.resourceInfo.ContainsKey(n))
            {
                gameStatus.resourceInfo[n] = new Dictionary<int, int>();
            }

            Tribe tribe = gameStatus.tribes[n];
            for(int m = 0; m < tribe.resources.Count; m++)
            {
                ResourceIdAmount r = tribe.resources[m];
                gameStatus.resourceInfo[n][r.resourceId] = r.amount;
            }

            //building
            for(int m = 0; m < tribe.buildings.Count; m++)
            {
                Building b = tribe.buildings[m];
                b.tribeId = n;
                gameStatus.buildingInfo[b.mapId] = b;
            }
        }

        return gameStatus;
    }
    public int GetTribeCount()
    {
        return tribes.Count;
    }
    public int GetResource(int tribeId, int resourceId)
    {
        if(!resourceInfo[tribeId].ContainsKey(resourceId))
            return 0;
        return resourceInfo[tribeId][resourceId];
    }
    public Building GetBuilding(int mapId)
    {
        return buildingInfo[mapId];
    }
}

[Serializable]
public class MarketStatus
{
    [Serializable]
    public struct ResourceIdRate
    {
        public int resourceId;
        public float rate;
    }
    
    [Serializable]
    public struct Market
    {
        public int mapId;
        public List<ResourceIdRate> exchanges;
    }

   
    public int standardResourceId;
    public List<Market> markets;
    /*
    market mapId, resourceId, rate
    */
    public Dictionary<int, Dictionary<int, float>> exchangeInfo;

    public static MarketStatus Load(string jsonFileName)
    {
        MarketStatus p = Json.LoadJsonFile<MarketStatus>(jsonFileName);

        p.exchangeInfo = new Dictionary<int, Dictionary<int, float>>();
        for(int n = 0; n < p.markets.Count; n++)
        {
            Market market = p.markets[n];
            if(p.exchangeInfo.ContainsKey(market.mapId) == false)
            {
                p.exchangeInfo[market.mapId] = new Dictionary<int, float>();
            }
            for(int m = 0; m < market.exchanges.Count; m++)
            {
                p.exchangeInfo[market.mapId][market.exchanges[m].resourceId] = market.exchanges[m].rate;
            }
        }

        return p;
    }
    public float GetExchangeRatio(int marketMapId, int resourceId)
    {
        if(!exchangeInfo.ContainsKey(marketMapId) || !exchangeInfo[marketMapId].ContainsKey(resourceId))
            return -1.0f;
        return exchangeInfo[marketMapId][resourceId];
    }
}