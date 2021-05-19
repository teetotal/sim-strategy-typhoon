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

        public Environment(int id, float rotation, GameObject gameObject = null)
        {
            this.id = id;
            this.rotation = rotation;
            this.gameObject = gameObject;
        }
        public GameObject Instantiate(int mapId)
        {
            return null;
            
            Meta.Environment meta = MetaManager.Instance.environmentInfo[id];
            GameObject parent = MapManager.Instance.defaultGameObjects[mapId];

            this.gameObject = MapManager.Instance.CreateInstance(mapId, 
                                                                        meta.name, 
                                                                        parent.transform.position + new Vector3(0, 0.1f, 0), 
                                                                        mapId.ToString(),
                                                                        TAG.ENVIRONMENT,
                                                                        parent
                                                                        );
            this.gameObject.transform.localEulerAngles += new Vector3(0, rotation, 0);
            return this.gameObject;
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
        Environment p = new Environment(id, rotation, null);
        
        if(isInstantiate)
        { 
            p.Instantiate(mapId);
        }
        environments[mapId] = p;
    }
    
    public void Instantiate()
    {
        List<int> keys = new List<int>();
        
        foreach(KeyValuePair<int, Environment> kv in environments)
        {
            keys.Add(kv.Key);
        }

        for(int n = 0; n < keys.Count; n++)
        {
            int mapId = keys[n];
            Environment p = environments[mapId];
            p.Instantiate(mapId);
            environments[mapId] = p;
        }

        
    }
    public void DestroyEnvironment(int mapId)
    {
        MapManager.Instance.Remove(mapId, TAG.ENVIRONMENT);
        GameObject.Destroy(environments[mapId].gameObject);
        environments.Remove(mapId);
        
        //MapManager.Instance.buildingObjects.Remove(mapId);
    }
}