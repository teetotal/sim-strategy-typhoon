﻿using UnityEngine;

public class Initializer : MonoBehaviour
{
    public string map, savedFile;

    // Start is called before the first frame update
    void Awake()
    {
        ItemManager.Instance.Load();
        InventoryManager.Instance.Load();
        
        MetaManager.Instance.Load();
        TradingManager.Instance.Load();

        MapManager.Instance.Load(map);
        GameStatusManager.Instance.Load(savedFile);
    }
}
