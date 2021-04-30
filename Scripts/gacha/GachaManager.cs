using System;
using System.Collections.Generic;
using UnityEngine;


public class GachaManager 
{
    public Object target;
    public TAG targetTag;
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

    public void SetGachaTarget(Object target, TAG tag)
    {
        this.target = target;
        this.targetTag = tag;
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
    public int GetAssignedMaterialCount()
    {
        int n = 0;
        foreach(KeyValuePair<int, int> kv in material)
        {
            n += kv.Value;
        }

        return n;
    }
    public List<int> GetAssignedMaterialList()
    {
        List<int> list = new List<int>();
        foreach(KeyValuePair<int, int> kv in material)
        {
            for(int n = 0; n < kv.Value; n++)
            {
                list.Add(kv.Key);
            }
        }
        return list;
    }
    void ConsumeMaterialInInventory()
    {
        foreach(KeyValuePair<int, int> kv in material)
        {
            Item item = ItemManager.Instance.items[kv.Key];
            InventoryManager.Instance.items[kv.Key] -= kv.Value;
        }   
    }
    float GetMaterialPower()
    {
        float sum = 0;
        foreach(KeyValuePair<int, int> kv in material)
        {
            Item item = ItemManager.Instance.items[kv.Key];
            //item level * amount 
            sum += item.v * item.amount * kv.Value;
        }   
        return sum;
    }
    float GetProbability()
    {
        float probability = 0;
        //확률 계산
        switch(targetTag)
        {
            case TAG.ACTOR:
                probability = MetaManager.Instance.actorInfo[target.id].level[target.level].probability;
                break;
            default:
                break;
        }
        return probability;
    }
    public float GetSuccessProbability()
    {
        return GetMaterialPower() / GetProbability();
    }

    public bool Levelup()
    {
        if(RunGacha())
        {
            Debug.Log(string.Format("Levelup success"));
            target.level++;
            return true;
        }

        return false;
    }
    //--------------------------------------------------------------
    private bool RunGacha()
    {
        float sum = GetMaterialPower();
        float probability = GetProbability();

        ConsumeMaterialInInventory();

        material.Clear();

        float f = UnityEngine.Random.Range(0.0f, probability);
        Debug.Log(string.Format("{0}, {1}", f, sum));
        if(f > sum)
            return false;

        return true;
    }
}