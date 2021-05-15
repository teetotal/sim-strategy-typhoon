using System;
using UnityEngine;
public class InitManager
{
    private static readonly Lazy<InitManager> hInstance = new Lazy<InitManager>(() => new InitManager());
    public string mapFilePath, savedFilePath;
    public static InitManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected InitManager()
    {
    }

    public void SetJsonPath(string map, string saved)
    {
        mapFilePath = map;
        savedFilePath = saved;
    }

    public bool Initialize()
    {
        ItemManager.Instance.Load();
        InventoryManager.Instance.Load();
        MetaManager.Instance.Load();
        TradingManager.Instance.Load();

        /*
        BuildingManager.Instance.Clear();
        ActorManager.Instance.Clear();
        MobManager.Instance.Clear();
        NeutralManager.Instance.Clear();
        */
        ObjectManager.Instance.Clear();
        EnvironmentManager.Instance.Clear();

        MapManager.Instance.Load(mapFilePath);
        GameStatusManager.Instance.Load(savedFilePath);

        return true;
    }
    public bool Instantiate()
    {
        GameObjectPooling.Instance.Reset();
        MapManager.Instance.CreatePrefabs();
        /*
        BuildingManager.Instance.Instantiate();
        NeutralManager.Instance.Instantiate();
        MobManager.Instance.Instantiate();
        */
        
        EnvironmentManager.Instance.Instantiate();
        ObjectManager.Instance.Instantiate();

        return true;
    }
}