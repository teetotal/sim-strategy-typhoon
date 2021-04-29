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
    public struct Inventory
    {
        public List<ItemIdQuantity> inventory;
    }

    public Dictionary<int, int> items = new Dictionary<int, int>(); //item id, quantity
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
        Inventory meta = Json.LoadJsonFile<Inventory>("inventory");
        for(int n=0; n < meta.inventory.Count; n++)
        {
            ItemIdQuantity i = meta.inventory[n];
            items[i.itemId] = i.quantity;
        }
    }
}