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
        public string tag;
    }
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
    public List<Prefab> prefabs;
    public Vector3Int grid;
    public Vector2Int dimension;
    public Value defaultVal;
    public List<Node> nodes;
}

public class MapManager
{
    public Map mapMeta;
    public List<GameObject> defaultGameObjects = new List<GameObject>();
    public Dictionary<int, GameObject> buildingObjects = new Dictionary<int, GameObject>();
    public int[,] map;
    private static readonly Lazy<MapManager> hInstance = new Lazy<MapManager>(() => new MapManager());
    public static MapManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }

    protected MapManager()
    {
    }
    //regarding object
    public GameObject GetBuildingObject(int id)
    {
        if(buildingObjects.ContainsKey(id) == false)
        {
            return null;
        }

        return buildingObjects[id];
    }
    //regarding position
    public int GetMapId(Vector2Int pos)
    {
        if(pos.x >= mapMeta.dimension.x || pos.y >= mapMeta.dimension.y)
        {
            return -1;
        }
        int v = pos.y * mapMeta.dimension.x;
        v += pos.x;
        return v;
    }
    public Vector2Int GetMapPosition(int id)
    {
        int y = id / mapMeta.dimension.x;
        int x = id % mapMeta.dimension.x;

        return new Vector2Int(x, y);
    }
    public Vector3 GetVector3FromMapId(int id)
    {
        return defaultGameObjects[id].transform.position;
    }
    public void Load()
    {
        mapMeta = Json.LoadJsonFile<Map>("map");
        map = new int[mapMeta.dimension.x, mapMeta.dimension.y];

        Vector2Int startPosition = new Vector2Int(mapMeta.dimension.x / 2, mapMeta.dimension.y / 2);
        Map.Prefab prefabInfo = mapMeta.prefabs[mapMeta.defaultVal.prefabId];
        //init map
        int idx = 0;
        for(int i = 0; i < map.GetLength(1); i++)
        {
            for(int j = 0; j < map.GetLength(0); j++)
            {
                map[j, i] = mapMeta.defaultVal.cost;
                GameObject defaultPrefab = Resources.Load<GameObject>(prefabInfo.name);
                defaultPrefab = GameObject.Instantiate(defaultPrefab, new Vector3(j - startPosition.x, -0.1f, i - startPosition.y), Quaternion.identity);
                defaultPrefab.name = idx++.ToString();
                defaultPrefab.tag = mapMeta.defaultVal.tag;

                defaultGameObjects.Add(defaultPrefab);

                //defaultPrefab.transform.SetParent(plane.transform);
            }
        }

        SetSpecificObject();
    }
    private void SetSpecificObject()
    {
        //set specific
        for(int n = 0; n < mapMeta.nodes.Count; n++)
        {
            Map.Node node = mapMeta.nodes[n];
            Map.Prefab prefabInfo = mapMeta.prefabs[node.prefabId];

            //positions
            for(int i = 0; i < node.positions.Count; i++)
            {
                CreateBuilding(node.positions[i], node, prefabInfo);   
            }

            //range
            if(node.range.end > node.range.start)
            {
                for(int id = node.range.start; id <= node.range.end; id++)
                {
                    CreateBuilding(id, node, prefabInfo);   
                }
            }
        }
    }
    private void CreateBuilding(int id, Map.Node node, Map.Prefab prefabInfo)
    {
        GameObject parent = defaultGameObjects[id];
        Vector2Int position = GetMapPosition(id);
        map[position.x, position.y] = prefabInfo.cost;
        GameObject obj = Resources.Load<GameObject>(prefabInfo.name);
        obj = GameObject.Instantiate(obj, new Vector3(parent.transform.position.x, parent.transform.position.y + 0.1f, parent.transform.position.z), Quaternion.identity);

        obj.transform.SetParent(parent.transform);
        if(buildingObjects.ContainsKey(id) == true)
        {
            throw new Exception("Already assigned position. conflict creating building");
        }
        buildingObjects[id] = obj;
    }
}