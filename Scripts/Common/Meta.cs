using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Meta
{
    [Serializable]
    public struct IdName
    {
        public int id;
        public string name;
    }
    [Serializable]
    public struct ActorIdMax
    {
        public int actorId;
        public int max;
    }
    [Serializable]
    public struct Ability
    {
        public int HP;
        public float building;
        public float farming;
        public int carring;
        public int attack;
        public float attackDistance;
        public float attackSpeed;
        public float depense;
        public float moving;
    }
    [Serializable]
    public struct ResourceIdAmount
    {
        public int resourceId;
        public int amount; 
    }
    [Serializable]
    public struct Mob
    {
        public int id;
        public string name;
        public string prefab;
        public float flyingHeight;
        public int level;
        public int regenTime, regenProbability; //regenTime 간격으로 1/regenProbability 확률로 생성
        public int mapCost;
        public int movingRange, movingProbability; //움직임 범위
        public Ability ability;
        public List<ResourceIdAmount> reward;
    }
    [Serializable]
    public struct ActorLevelInfo
    {
        public string prefab;
        public int createTime;
        public Ability ability;
        public List<ResourceIdAmount> wage;
    }
    [Serializable]
    public struct Actor
    {
        public int id;
        public string name;
        public bool flying;
        public List<ActorLevelInfo> level;
    }
    [Serializable]
    public struct Defense
    {
        public int attack;
        public float patrolTime; //일정 주기로 range내 object를 탐색
        public int speed;
        public float range;
    }
    [Serializable]
    public struct BuildingLevelInfo
    {
        public string prefab;
        public int buildTime;
        public int HP;
        public Defense defense;
        public List<ResourceIdAmount> costs;
        public List<ResourceIdAmount> output;
        public List<ActorIdMax> actors;
    }
    
    [Serializable]
    public struct Building
    {
        public int type;
        public int id;
        public string name;
        public List<BuildingLevelInfo> level;
        public Vector2Int dimension;
        
    }
    [Serializable]
    public struct Neutral
    {
        public int type;
        public int id;
        public string name;
        public string prefab;
        public int level;
        public Vector2Int dimension;
        public List<ActorIdMax> actors;
    }

    public List<string> tags;
    public List<IdName> resources;
    public List<Mob> mobs;
    public List<Actor> actors;
    public List<Building> buildings;
    public List<Neutral> neutrals;
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
}

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
}
