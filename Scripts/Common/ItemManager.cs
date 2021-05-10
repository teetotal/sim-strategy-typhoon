using System;
using System.Collections.Generic;
using UnityEngine;

/* --------------------------- */
//아이템 정보
[Serializable]
public struct Item
{
    public int id;
    public ItemType type;
    public string name;
    public string prefab;
    public float v; //resource일경우 resource id, gacha일경우 item level
    public float amount;
}
[Serializable]
public class ItemMeta
{
    public List<Item> items;
}

public class ItemManager 
{
    ItemMeta meta;
    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    private static readonly Lazy<ItemManager> hInstance = new Lazy<ItemManager>(() => new ItemManager());
    
    public static ItemManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected ItemManager()
    {
    }

    public void Load()
    {
        items.Clear();
        meta = Json.LoadJsonFile<ItemMeta>("items");
        for(int n=0; n < meta.items.Count; n++)
        {
            Item item = meta.items[n];
            items[item.id] = item;
        }
    }
}

public class InventoryManager
{
    [Serializable]
    public struct ItemIdQuantity
    {
        public int itemId;
        public int quantity;
    }
    [Serializable]
    public struct TribeItems
    {
        public int tribeId;
        public List<ItemIdQuantity> items;
    }
    [Serializable]
    public struct Inventory
    {
        public List<TribeItems> inventory;
    }
    
    public Dictionary<int, Dictionary<int, int>> items = new Dictionary<int, Dictionary<int, int>>(); // tribeId, item id, quantity
    private static readonly Lazy<InventoryManager> hInstance = new Lazy<InventoryManager>(() => new InventoryManager());
    
    public static InventoryManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected InventoryManager()
    {
    }
    public void Load()
    {
        items.Clear();
        
        Inventory meta = Json.LoadJsonFile<Inventory>("inventory");
        for(int n=0; n < meta.inventory.Count; n++)
        {
            int tribeId = meta.inventory[n].tribeId;
            for(int i = 0; i < meta.inventory[n].items.Count; i++)
            {
                ItemIdQuantity iq = meta.inventory[n].items[i];
                Add(tribeId, iq.itemId, iq.quantity);
            }
        }
    }
    public Dictionary<int, int> GetInventory(int tribeId)
    {
        if(!items.ContainsKey(tribeId))
            return null;
        
        return items[tribeId];
    }
    public void Add(int tribeId, int itemId, int quantity)
    {
        if(!items.ContainsKey(tribeId))
            items[tribeId] = new Dictionary<int, int>();

        if(!items[tribeId].ContainsKey(itemId))
            items[tribeId][itemId] = quantity;
        else
            items[tribeId][itemId] += quantity;
    }

    public bool Reduce(int tribeId, int itemId, int quantity)
    {
        if(!items.ContainsKey(tribeId) || !items[tribeId].ContainsKey(itemId) || items[tribeId][itemId] < quantity)
            return false;
        
        items[tribeId][itemId] -= quantity;
        return true;
    }
}