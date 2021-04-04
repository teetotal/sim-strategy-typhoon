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
    public struct Actor
    {
        public int id;
        public string name;
        public int level;
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

    public List<IdName> resources;
    public List<Actor> actors;
    public List<Building> buildings;
}

