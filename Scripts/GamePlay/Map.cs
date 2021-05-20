using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Map
{
    [Serializable]
    public struct Value
    {
        public int cost;
        public int prefabId;
        public float AstarNodeCost;
    }
    /*
    [Serializable]
    public struct Prefab
    {
        public int cost;
        public string name;
    }
    [Serializable]
    public struct Range
    {
        public int start;
        public int end;
    }
    
    [Serializable]
    public struct Node
    {
        public List<int> positions;
        public Range range;
        public int prefabId;
    }
    [Serializable]
    public struct Mob
    {
        public int id;
        public int mapId;
        public int max;
    }
    [Serializable]
    public struct Neutral
    {
        public int id;
        public int mapId;
        public int rotation;
    }
    */
    //public List<Prefab> prefabs;
    public Vector3Int grid;
    public Vector2Int dimension;
    public Value defaultVal;
    //public List<Node> nodes;
    //public List<Mob> mobs;
    //public List<Neutral> neutrals;
}
