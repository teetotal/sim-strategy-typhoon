using System;
using System.Collections.Generic;
using UnityEngine;


public class GachaManager 
{
    public Object target;
    public Dictionary<int, int> material = new Dictionary<int, int>(); // 강화 재료. item id, count
    private static readonly Lazy<GachaManager> hInstance = new Lazy<GachaManager>(() => new GachaManager());
    
    public static GachaManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected GachaManager()
    {
    }

    public void SetGachaTarget(Object target)
    {
        this.target = target;
    }
    public bool AddMaterial(int itemId, int amount = 1)
    {
        if(ItemManager.Instance.items[itemId].type != ItemType.GACHA_MATERIAL)
            return false;
        
        if(!material.ContainsKey(itemId))
            material[itemId] = 0;

        material[itemId] += amount;
        return true;
    }

    public bool SubtractMaterial(int itemId, int amount = 1)
    {
        if(ItemManager.Instance.items[itemId].type != ItemType.GACHA_MATERIAL)
            return false;

        if(!material.ContainsKey(itemId) || material[itemId] < amount)
            return false;

        material[itemId] -= amount;
        return true;
    }
    public int GetAssignedMaterialCount(int itemId)
    {
        if(!material.ContainsKey(itemId))
            return 0;
        return material[itemId];
    }

    public bool Run()
    {
        float sum = 0;
        foreach(KeyValuePair<int, int> kv in material)
        {
            Item item = ItemManager.Instance.items[kv.Key];
            //item level * amount 
            sum += item.v * item.amount * kv.Value;
        }

        //확률 계산

        return true;
    }
}