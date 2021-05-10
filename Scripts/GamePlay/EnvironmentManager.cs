using System;
using System.Collections.Generic;
using UnityEngine;
public class EnvironmentManager
{
    public struct Environment
    {
        public int id;
        public float rotation;
        public GameObject gameObject;

        public Environment(int id, float rotation, GameObject gameObject)
        {
            this.id = id;
            this.rotation = rotation;
            this.gameObject = gameObject;
        }
    }
    public Dictionary<int, Environment> environments = new Dictionary<int, Environment>(); //mapId , id
    private static readonly Lazy<EnvironmentManager> hInstance = new Lazy<EnvironmentManager>(() => new EnvironmentManager());
    
    public static EnvironmentManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected EnvironmentManager()
    {
    }
    public void Clear()
    {
        environments.Clear();
    }
    public void Create(int mapId, int id, float rotation, bool isInstantiate)
    {
        Meta.Environment meta = MetaManager.Instance.environmentInfo[id];
        MapManager.Instance.SetMapId(mapId, meta.cost);
        GameObject obj = null;
        
        if(isInstantiate)
        {
            obj = Instantiate(mapId, id, rotation);
        }
        environments[mapId] = new Environment(id, rotation, obj);
    }
    private GameObject Instantiate(int mapId, int id, float rotation)
    {
        Meta.Environment meta = MetaManager.Instance.environmentInfo[id];
        GameObject parent = MapManager.Instance.defaultGameObjects[mapId];

        GameObject gameObject = MapManager.Instance.CreateInstance(mapId, 
                                                                    meta.name, 
                                                                    parent.transform.position + new Vector3(0, 0.1f, 0), 
                                                                    mapId.ToString(),
                                                                    TAG.ENVIRONMENT,
                                                                    parent
                                                                    );
        gameObject.transform.localEulerAngles += new Vector3(0, rotation, 0);
        return gameObject;
    }
    public void Instantiate()
    {
        foreach(KeyValuePair<int, Environment> kv in environments)
        {
            Environment p = kv.Value;
            p.gameObject = Instantiate(kv.Key, kv.Value.id, kv.Value.rotation);
        }
    }
    public void DestroyEnvironment(int mapId)
    {
        MapManager.Instance.DestroyBuilding(mapId);
        GameObject.Destroy(environments[mapId].gameObject);
        environments.Remove(mapId);
        MapManager.Instance.Remove(mapId, TAG.ENVIRONMENT);
        //MapManager.Instance.buildingObjects.Remove(mapId);
    }
}