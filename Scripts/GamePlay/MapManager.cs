using System;
using System.Collections.Generic;
using UnityEngine;
public class MapManager
{
    public Map mapMeta;
    public List<GameObject> defaultGameObjects = new List<GameObject>();
    public Dictionary<int, GameObject> buildingObjects = new Dictionary<int, GameObject>();
    
    public int[,] map;
    public List<Object[,]> currentMap = new List<Object[,]>(); //현재 위치 저장 맵. 일단은 여러개가 같은 위치에 있어도 하나의 객체만 저장한다.
    private Vector2 startPosition;
    private GameObject plane;
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
        //return defaultGameObjects[id].transform.position;
        int y = id / mapMeta.dimension.x;
        int x = id % mapMeta.dimension.x;

        return new Vector3(x - startPosition.x, 0, y - startPosition.y);
    }
    public int GetMapId(Vector3 position)
    {
        int start = (int)(mapMeta.dimension.x / 2);
        Vector2Int pos = new Vector2Int((int)position.x + start, (int)position.z + (start));
        return GetMapId(pos);
    }
    public bool IsEmptyMapId(int id)
    {
        Vector2Int pos = GetMapPosition(id);
        //Debug.Log(string.Format("IsEmptyMapId {0} - {1}", pos, map[pos.x, pos.y]));
        if(map[pos.x, pos.y] == mapMeta.defaultVal.cost) // && defaultGameObjects[id].gameObject.transform.childCount == 0)
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
    public void Load(string fileName)
    {
        currentMap.Clear();
        
        mapMeta = Json.LoadJsonFile<Map>(fileName);
        map = new int[mapMeta.dimension.x, mapMeta.dimension.y];
        for(int n = 0; n < (int)TAG.MAX; n++)
        {
            currentMap.Add(new Object[mapMeta.dimension.x, mapMeta.dimension.y]);
        }

        //Vector2Int startPosition = new Vector2Int(mapMeta.dimension.x / 2, mapMeta.dimension.y / 2);
        //Map.Prefab prefabInfoDefault = mapMeta.prefabs[mapMeta.defaultVal.prefabId];
        //init map
        
        for(int i = 0; i < map.GetLength(1); i++)
        {
            for(int j = 0; j < map.GetLength(0); j++)
            {
                map[j, i] = mapMeta.defaultVal.cost;
            }
        }

        //set start position
        startPosition = new Vector2(mapMeta.dimension.x / 2, mapMeta.dimension.y / 2);

        if(mapMeta.dimension.x % 2 == 0)
        {
            startPosition.x -= 0.5f;
        }

        if(mapMeta.dimension.y % 2 == 0)
        {
            startPosition.y -= 0.5f;
        }

        /*
        //set specific
        for(int n = 0; n < mapMeta.nodes.Count; n++)
        {
            Map.Node node = mapMeta.nodes[n];
            //Map.Prefab prefabInfo = mapMeta.prefabs[node.prefabId];
            Meta.Environment prefabInfo = MetaManager.Instance.environmentInfo[node.prefabId];

            //positions
            for(int i = 0; i < node.positions.Count; i++)
            {
                int mapId = node.positions[i];
                EnvironmentManager.Instance.Create(mapId, node.prefabId, 0, false);
            }

            //range
            if(node.range.end > node.range.start)
            {
                for(int id = node.range.start; id <= node.range.end; id++)
                {
                    int mapId = id;
                    EnvironmentManager.Instance.Create(mapId, prefabInfo.id, 0, false);
                }
            }
        }

        //SetNeutrals
        for(int n = 0; n < mapMeta.neutrals.Count; n++)
        {
            Map.Neutral ne = mapMeta.neutrals[n];
            NeutralManager.Instance.Create(ne.mapId, ne.id, ne.rotation, false);
            AssignBuilding(ne.mapId);
        }
        */
    }
    
    public void CreatePrefabs()
    {
        defaultGameObjects.Clear();
        buildingObjects.Clear();

        Vector2Int startPosition = new Vector2Int(mapMeta.dimension.x / 2, mapMeta.dimension.y / 2);
        //Map.Prefab prefabInfoDefault = mapMeta.prefabs[mapMeta.defaultVal.prefabId];
        Meta.Environment prefabInfoDefault = MetaManager.Instance.environmentInfo[mapMeta.defaultVal.prefabId];

        //plane
        GameObject p = Resources.Load<GameObject>(prefabInfoDefault.name);
        plane = GameObject.Instantiate(p, Vector3.zero, Quaternion.identity);
        plane.name = "plane";
        plane.tag = MetaManager.Instance.GetTag(TAG.BOTTOM);
        plane.transform.localScale = new Vector3(mapMeta.dimension.x, 1, mapMeta.dimension.y);

        //init map
        /*
        int idx = 0;
        for(int i = 0; i < map.GetLength(1); i++)
        {
            for(int j = 0; j < map.GetLength(0); j++)
            {
                //CreateInstance(idx, prefabInfoDefault.name, new Vector3(j - startPosition.x, -0.1f, i - startPosition.y), idx.ToString(), TAG.BOTTOM, null);
                CreateInstance(idx, 
                                Bottom.GetBottomPrefab(j, i), 
                                new Vector3(j - startPosition.x, -0.1f, i - startPosition.y), 
                                idx.ToString(), 
                                TAG.BOTTOM, 
                                null);
                idx++;
            }
        }
        */
        /*
        //set specific
        for(int n = 0; n < mapMeta.nodes.Count; n++)
        {
            Map.Node node = mapMeta.nodes[n];
            //Map.Prefab prefabInfo = mapMeta.prefabs[node.prefabId];
            Meta.Environment prefabInfo = MetaManager.Instance.environmentInfo[node.prefabId];

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
        */
    }
    public GameObject CreateInstance(string prefab, Vector3 position, string name, TAG tag)
    {
        GameObject p = Resources.Load<GameObject>(prefab);
        p = GameObject.Instantiate(p, position, Quaternion.identity);
        p.name = name;
        p.tag = MetaManager.Instance.GetTag(tag);
        /*
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
                    throw new Exception(string.Format("Already assigned position. {0}, {1}, {2}", mapId, tag, prefab));
                }
                buildingObjects[mapId] = p;
            break;
        } 
        */  
        if(plane != null)
            p.transform.SetParent(plane.transform);

        return p;
    }
    
    //잡다한 destroy 함수들 죄다 정리할 필요 있음
    public void Remove(int mapId, TAG tag)
    {
        Vector2Int pos = GetMapPosition(mapId);
        map[pos.x, pos.y] = mapMeta.defaultVal.cost;
        currentMap[(int)tag][pos.x, pos.y] = null;
        if(buildingObjects.ContainsKey(mapId))
        {
            buildingObjects.Remove(mapId);
        }
    }
    public void AssignBuilding(int mapId)
    {
        Vector2Int position = GetMapPosition(mapId);
        map[position.x, position.y] = -1;
    }
}