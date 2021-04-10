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
        public int building;
        public int farming;
        public int carring;
        public int attack;
        public int depense;
        public int moving;
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
        public int level;
        public int regenTime, regenProbability; //regenTime 간격으로 1/regenProbability 확률로 생성
        public int max;         //생성가능한 수
        public int mapCost;
        public int movingRange, movingProbability; //움직임 범위
        public Ability ability;
        public List<ResourceIdAmount> reward;
    }
    [Serializable]
    public struct Actor
    {
        public int id;
        public string name;
        public string prefab;
        public bool flying;
        public int level;
        public int createTime;
        public Ability ability;
        public List<ResourceIdAmount> wage;
    }
    [Serializable]
    public struct Building
    {
        public int id;
        public string name;
        public string prefab;
        public int level;
        public Vector2Int dimension;
        public int buildTime;
        public List<ResourceIdAmount> costs;
        public List<ResourceIdAmount> output;
        public List<ActorIdMax> actors;
    }

    public List<string> tags;
    public List<IdName> resources;
    public List<Mob> mobs;
    public List<Actor> actors;
    public List<Building> buildings;
}

