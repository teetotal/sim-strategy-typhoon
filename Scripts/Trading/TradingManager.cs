using System;
using System.Collections.Generic;
using UnityEngine;
public class TradingManager 
{
    [Serializable]
    struct TradingResource
    {
        public int resourceId;
        public int holding;
        public int standardHolding;
        public float buyFee;
        public float sellFee;
        public List<float> history;
    }
    [Serializable]
    struct TradingInfo
    {
        public int standardResourceId;
        public List<TradingResource> resources;
    }
    int standardResourceId = 0;
    int callCount = 0;
    float revenue = 1000; //보유 금액 한도 내에서 매입이 가능
    Dictionary<int, int> holdings = new Dictionary<int, int>(); //resource id, 보유량
    Dictionary<int, int> standardHoldings = new Dictionary<int, int>();//현금과 1:1이 되는 기준보유 량
    Dictionary<int, float> buyFees = new Dictionary<int, float>(); //살때 수수료
    Dictionary<int, float> sellFees = new Dictionary<int, float>(); //팔때 수수료
    Dictionary<int, List<float>> marketPriceHistory = new Dictionary<int, List<float>>(); //시세 흐름
    float elapse = 0;
    float marketPriceUpdateInterval = 60 * 1;

    private static readonly Lazy<TradingManager> hInstance = new Lazy<TradingManager>(() => new TradingManager());
    
    public static TradingManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected TradingManager()
    {
    }

    public void Load()
    {
        TradingInfo info = Json.LoadJsonFileFromResources<TradingInfo>("trading");

        standardResourceId = info.standardResourceId;
        for(int n = 0; n < info.resources.Count; n++)
        {
            TradingResource p = info.resources[n];
            holdings[p.resourceId] = p.holding;
            standardHoldings[p.resourceId] = p.standardHolding;
            buyFees[p.resourceId] = p.buyFee;
            sellFees[p.resourceId] = p.sellFee;
            marketPriceHistory[p.resourceId] = p.history;
        }
    }

    public bool Buy(int tribeId, int resourceId, int amount)
    {
        //단가 
        float unitPrice = GetBuyPrice(resourceId);
        float price = unitPrice * amount;
        if(holdings[resourceId] - amount < 0)
            return false;
        if(GameStatusManager.Instance.GetResource(tribeId, standardResourceId) - price < 0)
            return false;

        revenue += price;
        //보유량 감소
        holdings[resourceId] -= amount;
        //구매자에게 차감
        GameStatusManager.Instance.ReduceResource(tribeId, standardResourceId, price);

        callCount++;
        return true;

    }
    public bool Sell(int tribeId, int resourceId, int amount)
    {
        //단가 
        float unitPrice = GetSellPrice(resourceId);
        float price = unitPrice * amount;
        if(revenue - price < 0)
            return false;
        
        revenue -= price;
        //보유량 증가
        holdings[resourceId] += amount;
        //판매자에게 지급
        GameStatusManager.Instance.AddResource(tribeId, standardResourceId, price);

        callCount++;
        return true;
    }
    //살때 가격
    public float GetBuyPrice(int resourceId)
    {
        float x = GetStandardPrice(resourceId);
        return x + (x * buyFees[resourceId]); 
    }
    //팔때 가격
    public float GetSellPrice(int resourceId)
    {
        float x = GetStandardPrice(resourceId);
        return x - (x * sellFees[resourceId]); 
    }
    private float GetStandardPrice(int resourceId)
    {
        //1:1/standardHoldings = x:1/holdings
        float h = (float)holdings[resourceId];
        float s = (float)standardHoldings[resourceId];

        //float x = (1 / holdings[resourceId]) / (1/standardHoldings[resourceId]);
        float x = (1.0f/h) / (1.0f/s);
        return x;
    }
    public int GetHoding(int resourceId)
    {
        return holdings[resourceId];
    }
    //시세 기록
    public void Update()
    {
        elapse += Time.deltaTime;
        if(elapse < marketPriceUpdateInterval || callCount == 0)
            return;

        elapse = 0;
        callCount = 0;
        foreach(KeyValuePair<int, int> kv in holdings)
        {
            int resourceId = kv.Key;
            float price = GetSellPrice(resourceId);

            marketPriceHistory[resourceId].Add(price);
        }
    }
}