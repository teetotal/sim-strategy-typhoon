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
    [Serializable]
    public struct Neutral
    {
        public int id;
        public int mapId;
        public int rotation;
    }

    public List<Prefab> prefabs;
    public Vector3Int grid;
    public Vector2Int dimension;
    public Value defaultVal;
    public List<Node> nodes;
    public List<Mob> mobs;
    public List<Neutral> neutrals;
}

public class MapManager
{
    public Map mapMeta;
    public List<GameObject> defaultGameObjects = new List<GameObject>();
    public Dictionary<int, GameObject> buildingObjects = new Dictionary<int, GameObject>();
    public int[,] map;
    public List<Object[,]> currentMap = new List<Object[,]>(); //현재 위치 저장 맵. 일단은 여러개가 같은 위치에 있어도 하나의 객체만 저장한다.
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
    public bool SetCurrentMap(Object obj, TAG tag)
    {
        Vector2Int pos = GetMapPosition(obj.GetCurrentMapId());
        if(currentMap[(int)tag][pos.x, pos.y] == null)
        {
            currentMap[(int)tag][pos.x, pos.y] = obj;
            return true;
        }
        return false;
    }
    public bool MoveCurrentMap(int beforeMapId, Object obj, TAG tag)
    {
        //Debug.Log(string.Format("MoveCurrentMap {0} -> {1}", beforeMapId, obj.currentMapId));
        //위치에 내가 있을 수도 있고 없을 수도 있다.
        Vector2Int pos = GetMapPosition(beforeMapId);
        Object beforeObj = currentMap[(int)tag][pos.x, pos.y];
        if(beforeObj != null && beforeObj.mapId == obj.mapId)
            currentMap[(int)tag][pos.x, pos.y] = null;
        
        return SetCurrentMap(obj, tag);
    }
    //get map id which is not empty
    public List<GameObject> GetFilledMapId(int mapId, int range, List<TAG> skipTags = null)
    {
        List<GameObject> list = new List<GameObject>();
        Vector2Int pos = GetMapPosition(mapId);
        
        for(int y = pos.y - range; y <= pos.y + range; y++)
        {
            if(y < 0 || map.GetLength(1) <= y)
                continue;

            for(int x = pos.x - range; x <= pos.x + range; x++)
            {
                if(x < 0 || map.GetLength(0) <= x)
                    continue;
                if(pos.x == x && pos.y == y)
                    continue;

                GameObject obj = null;
                TAG tag = TAG.MAX;
                if(currentMap[(int)TAG.ACTOR][x, y] != null)
                {
                    obj = currentMap[(int)TAG.ACTOR][x, y].gameObject;
                    tag = TAG.ACTOR;
                }
                else if(currentMap[(int)TAG.MOB][x, y] != null)
                {
                    obj = currentMap[(int)TAG.MOB][x, y].gameObject;
                    tag = TAG.MOB;
                }
                    

                if(obj != null)
                {
                    bool isAdd = true;
                    if(skipTags != null)
                    {
                        for(int n = 0; n < skipTags.Count; n++)
                        {
                            if(skipTags[n] == tag)
                            {
                                isAdd = false;
                                break;
                            }
                        }
                    }
                    if(isAdd)
                        list.Add(obj);
                }
            }
        }
        return list;
    }
    public float GetDistance(int from, int to)
    {
        return Vector2Int.Distance(GetMapPosition(from), GetMapPosition(to));
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
        for(int n = 0; n < (int)TAG.MAX; n++)
        {
            currentMap.Add(new Object[mapMeta.dimension.x, mapMeta.dimension.y]);
        }

        Vector2Int startPosition = new Vector2Int(mapMeta.dimension.x / 2, mapMeta.dimension.y / 2);
        Map.Prefab prefabInfoDefault = mapMeta.prefabs[mapMeta.defaultVal.prefabId];
        //init map
        
        for(int i = 0; i < map.GetLength(1); i++)
        {
            for(int j = 0; j < map.GetLength(0); j++)
            {
                map[j, i] = mapMeta.defaultVal.cost;
            }
        }
        //set specific
        for(int n = 0; n < mapMeta.nodes.Count; n++)
        {
            Map.Node node = mapMeta.nodes[n];
            Map.Prefab prefabInfo = mapMeta.prefabs[node.prefabId];

            //positions
            for(int i = 0; i < node.positions.Count; i++)
            {
                int mapId = node.positions[i];
                Vector2Int position = GetMapPosition(mapId);
                map[position.x, position.y] = prefabInfo.cost;
            }

            //range
            if(node.range.end > node.range.start)
            {
                for(int id = node.range.start; id <= node.range.end; id++)
                {
                    int mapId = id;
                    Vector2Int position = GetMapPosition(mapId);
                    map[position.x, position.y] = prefabInfo.cost;
                }
            }
        }

        //SetNeutrals
        for(int n = 0; n < mapMeta.neutrals.Count; n++)
        {
            Map.Neutral ne = mapMeta.neutrals[n];
            int mapId = NeutralManager.Instance.CreateObjectOnly(ne.mapId, ne.id);
            Vector2Int position = GetMapPosition(mapId);
            map[position.x, position.y] = -1;
        }
    }
    public void CreatePrefabs()
    {
        Vector2Int startPosition = new Vector2Int(mapMeta.dimension.x / 2, mapMeta.dimension.y / 2);
        Map.Prefab prefabInfoDefault = mapMeta.prefabs[mapMeta.defaultVal.prefabId];
        //init map
        int idx = 0;
        for(int i = 0; i < map.GetLength(1); i++)
        {
            for(int j = 0; j < map.GetLength(0); j++)
            {
                CreateInstance(idx, prefabInfoDefault.name, new Vector3(j - startPosition.x, -0.1f, i - startPosition.y), idx.ToString(), TAG.BOTTOM, null);
                idx++;
            }
        }
        //set specific
        for(int n = 0; n < mapMeta.nodes.Count; n++)
        {
            Map.Node node = mapMeta.nodes[n];
            Map.Prefab prefabInfo = mapMeta.prefabs[node.prefabId];

            //positions
            for(int i = 0; i < node.positions.Count; i++)
            {
                int mapId = node.positions[i];
                GameObject parent = defaultGameObjects[mapId];

                CreateInstance(mapId, 
                                prefabInfo.name, 
                                parent.transform.position + new Vector3(0, 0.1f, 0), 
                                mapId.ToString(),
                                TAG.ENVIRONMENT,
                                parent
                                );
            }

            //range
            if(node.range.end > node.range.start)
            {
                for(int id = node.range.start; id <= node.range.end; id++)
                {
                    int mapId = id;
                    GameObject parent = defaultGameObjects[mapId];

                    CreateInstance(mapId, 
                                prefabInfo.name, 
                                parent.transform.position + new Vector3(0, 0.1f, 0), 
                                mapId.ToString(),
                                TAG.ENVIRONMENT,
                                parent
                                );
                }
            }
        }

        //SetNeutrals
        foreach(KeyValuePair<int, NeutralBuilding> kv in NeutralManager.Instance.objects)
        {
            int mapId = kv.Value.mapId;
            GameObject parent = defaultGameObjects[mapId];
            kv.Value.gameObject = CreateInstance(mapId, 
                                                MetaManager.Instance.neutralInfo[kv.Value.id].prefab, 
                                                parent.transform.position + new Vector3(0, 0.1f, 0), 
                                                mapId.ToString(),
                                                TAG.NEUTRAL,
                                                parent
                                                );
        }
    }
    public GameObject CreateInstance(int mapId, string prefab, Vector3 position, string name, TAG tag, GameObject parent)
    {
        GameObject p = Resources.Load<GameObject>(prefab);
        p = GameObject.Instantiate(p, position, Quaternion.identity);
        p.name = name;
        p.tag = MetaManager.Instance.GetTag(tag);
        switch(tag)
        {
            case TAG.BOTTOM:
                defaultGameObjects.Add(p);
            break;
            case TAG.BUILDING:
            case TAG.ENVIRONMENT:
            case TAG.NEUTRAL:
                if(buildingObjects.ContainsKey(mapId) == true)
                {
                    throw new Exception("Already assigned position. conflict creating building");
                }
                buildingObjects[mapId] = p;
            break;
        } 
            
        if(parent != null)
            p.transform.SetParent(parent.transform);

        return p;
    }
    /*
    private void SetNeutrals()
    {
        for(int n = 0; n < mapMeta.neutrals.Count; n++)
        {
            Map.Neutral ne = mapMeta.neutrals[n];
            Updater.Instance.AddQ(ActionType.NEUTRAL_CREATE, -1, ne.mapId, ne.id, new List<int>() { ne.rotation }, true);
        }
    }
    */
    public void Remove(int mapId, TAG tag)
    {
        Vector2Int pos = GetMapPosition(mapId);
        map[pos.x, pos.y] = mapMeta.defaultVal.cost;
        currentMap[(int)tag][pos.x, pos.y] = null;
    }
    public void DestroyBuilding(int mapId)
    {
        Vector2Int pos = GetMapPosition(mapId);
        map[pos.x, pos.y] = mapMeta.defaultVal.cost;
        GameObject.DestroyImmediate(buildingObjects[mapId]);
        buildingObjects.Remove(mapId);
    }
    public GameObject CreateNeutral(int id, string prefab)
    {
        return Construct(id, prefab, -1, TAG.NEUTRAL);
    }
    public GameObject CreateBuilding(int id, string prefab)
    {
        return Construct(id, prefab, -1, TAG.BUILDING);
    }
    
    private GameObject Construct(int id, string prefab, int mapCost, TAG tag)
    {
        GameObject parent = defaultGameObjects[id];
        Vector2Int position = GetMapPosition(id);
        map[position.x, position.y] = mapCost;
        GameObject obj = Resources.Load<GameObject>(prefab);
        obj = GameObject.Instantiate(obj, new Vector3(parent.transform.position.x, parent.transform.position.y + 0.1f, parent.transform.position.z), Quaternion.identity);
        obj.tag = MetaManager.Instance.GetTag(tag);
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