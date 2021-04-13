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
    [Serializable]
    public struct Mob
    {
        public int id;
        public int mapId;
        public int max;
    }

    public List<Prefab> prefabs;
    public Vector3Int grid;
    public Vector2Int dimension;
    public Value defaultVal;
    public List<Node> nodes;
    public List<Mob> mobs;
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
    //for moving
    public void Move(int from, int to)
    {
        Vector2Int fromPos = GetMapPosition(from);
        Vector2Int toPos = GetMapPosition(to);
        map[toPos.x, toPos.y] = map[fromPos.x, fromPos.y];
        map[fromPos.x, fromPos.y] = mapMeta.defaultVal.cost;
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
    public void SetMapId(int mapId, int cost)
    {
        Vector2Int pos = GetMapPosition(mapId);
        map[pos.x, pos.y] = cost;
    }
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
    public bool IsEmptyMapId(int id)
    {
        Vector2Int pos = GetMapPosition(id);
        //Debug.Log(string.Format("IsEmptyMapId {0} - {1}", pos, map[pos.x, pos.y]));
        if(map[pos.x, pos.y] == mapMeta.defaultVal.cost && defaultGameObjects[id].gameObject.transform.childCount == 0)
        {
            return true;
        }
        return false;
    }
    public int GetEmptyMapId()
    {
        int max = mapMeta.dimension.x * mapMeta.dimension.y -1;
        for(int n = 0; n < 10; n++)
        {
            int id = UnityEngine.Random.Range(0, max);
            if(IsEmptyMapId(id))
            {
                return id;
            }
        }
        return -1;
    }
    public int GetRandomNearEmptyMapId(int id, int range)
    {
        List<int> list = new List<int>();
        Vector2Int pos = GetMapPosition(id);
        
        for(int y = pos.y - range; y <= pos.y + range; y++)
        {
            if(y < 0 || map.GetLength(1) <= y)
                continue;

            for(int x = pos.x - range; x <= pos.x + range; x++)
            {
                if(x < 0 || map.GetLength(0) <= x)
                    continue;

                int mapId = GetMapId(new Vector2Int(x, y));
                if(IsEmptyMapId(mapId))
                {
                    list.Add(mapId);
                }
            }
        }
        if(list.Count == 0)
            return -1;

        int idx = UnityEngine.Random.Range(0, list.Count);
        //Debug.Log(string.Format("{0} / {1}", idx, list.Count-1));
        
        return list[idx];
    }
    public int AssignNearEmptyMapId(int id)
    {
        Vector2Int pos = GetMapPosition(id);
        int range = 1;
        while(true)
        {
            int cnt = 0;
            for(int y = pos.y - range; y <= pos.y + range; y++)
            {
                if(y < 0 || map.GetLength(1) <= y)
                    continue;

                for(int x = pos.x - range; x <= pos.x + range; x++)
                {
                    if(x < 0 || map.GetLength(0) <= x)
                        continue;

                    int mapId = GetMapId(new Vector2Int(x, y));
                    if(IsEmptyMapId(mapId))
                    {
                        map[x, y] = mapMeta.defaultVal.cost + 1;
                        return mapId;
                    }
                    else
                    {
                        cnt++;
                    }
                }
            }

            if(cnt == 0)
                    return -1;
            range++;
        }
    }
    public int GetCost(int mapId)
    {
        Vector2Int pos = GetMapPosition(mapId);
        return map[pos.x, pos.y];
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
                defaultPrefab.tag = MetaManager.Instance.GetTag(MetaManager.TAG.BOTTOM);

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
                CreateEnvironment(node.positions[i], prefabInfo.name, prefabInfo.cost);   
            }

            //range
            if(node.range.end > node.range.start)
            {
                for(int id = node.range.start; id <= node.range.end; id++)
                {
                    CreateEnvironment(id, prefabInfo.name, prefabInfo.cost);   
                }
            }
        }
    }
    public void DestroyBuilding(int mapId)
    {
        Vector2Int pos = GetMapPosition(mapId);
        map[pos.x, pos.y] = mapMeta.defaultVal.cost;
        GameObject.DestroyImmediate(buildingObjects[mapId]);
        buildingObjects.Remove(mapId);
    }
    public GameObject CreateBuilding(int id, string prefab)
    {
        return Construct(id, prefab, -1, true);
    }
    private GameObject CreateEnvironment(int id, string prefab, int mapCost)
    {
        return Construct(id, prefab, mapCost, false);
    }
    private GameObject Construct(int id, string prefab, int mapCost, bool isBuilding)
    {
        GameObject parent = defaultGameObjects[id];
        Vector2Int position = GetMapPosition(id);
        map[position.x, position.y] = mapCost;
        GameObject obj = Resources.Load<GameObject>(prefab);
        obj = GameObject.Instantiate(obj, new Vector3(parent.transform.position.x, parent.transform.position.y + 0.1f, parent.transform.position.z), Quaternion.identity);
        obj.tag = isBuilding ? MetaManager.Instance.GetTag(MetaManager.TAG.BUILDING) : MetaManager.Instance.GetTag(MetaManager.TAG.ENVIRONMENT);
        obj.name = id.ToString();
        obj.transform.SetParent(parent.transform);
        if(buildingObjects.ContainsKey(id) == true)
        {
            throw new Exception("Already assigned position. conflict creating building");
        }
        buildingObjects[id] = obj;

        return obj;
    }
}