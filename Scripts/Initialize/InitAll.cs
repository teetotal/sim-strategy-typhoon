using UnityEngine;

public class InitAll : MonoBehaviour
{
    public string map, savedFile;
    void Awake()
    {
        ItemManager.Instance.Load();
        InventoryManager.Instance.Load();
        
        MetaManager.Instance.Load();
        TradingManager.Instance.Load();

        MapManager.Instance.Load(map);
        GameStatusManager.Instance.Load(savedFile);

        GameObjectPooling.Instance.Reset();
        MapManager.Instance.CreatePrefabs();
        BuildingManager.Instance.Instantiate();
        NeutralManager.Instance.Instantiate();
        MobManager.Instance.Instantiate();
    }
}
