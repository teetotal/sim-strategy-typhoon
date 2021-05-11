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
        public int max;//최대 regen
        public int movingRange, movingProbability; //움직임 범위
        public Ability ability;
        public Booty booty;
    }
    [Serializable]
    public struct IdQuantity
    {
        public int id;
        public int quantity;
    }
    [Serializable]
    public struct Booty //죽었을때 떨구는 전리품 정보
    {
        public int count;   //items들 중 몇개를 떨굴 것인가
        public List<int> probability; //items중 선택될 확률. float로 가면 소수점 이하가 무한히 길어질때 퍼센트 확률과 차이가 크게난다.
        public List<IdQuantity> items;  //item id, quantity
    }
    [Serializable]
    public struct ActorLevelInfo
    {
        public string prefab;
        public int createTime;
        public float probability; //다음 단계 강화 확률
        public Ability ability;
        public List<ResourceIdAmount> wage;
        public Booty booty;
    }
    [Serializable]
    public struct Actor
    {
        public int id;
        public string name;
        public float earningTime;
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
        public float earningTime;
        public float probability; //다음 단계 강화 확률
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
    [Serializable]
    public struct Environment
    {
        public int id;
        public string name;
        public int cost;
    }

    public List<string> tags;
    public List<Environment> environments;
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
    public struct MapIdActorIdHP
    {
        public int mapId;
        public int actorId;
        public float HP;
        public float rotation;
    }
    [Serializable]
    public struct ResourceIdAmount
    {
        public int resourceId;
        public int amount;
    }
    [Serializable]
    public class Building : MapIdBuildingId
    {
        public List<MapIdActorIdHP> actors;
    }
    [Serializable]
    public struct Tribe
    {
        public List<ResourceIdAmount> resources;
        public List<Building> buildings;
    }

    [Serializable]
    public struct Neutral
    {
        public int mapId;
        public int neutralId;
        public float rotation;
    }
    [Serializable]
    public struct Mob
    {
        public int mapId;
        public int mobId;
        public int amount;
    }
    [Serializable]
    public struct Environment
    {
        public int mapId;
        public int environmentId;
        public float rotation;
    }
    public List<Tribe> tribes;
    public List<Neutral> neutrals;
    public List<Mob> mobs;
    public List<Environment> environments;
}
