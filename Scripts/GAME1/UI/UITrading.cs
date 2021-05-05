using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UITrading : IUIInterface
{
    GameObject parentLayer;
    Button close;
    Text resource1_name, resource1_buy, resource1_sell;
    Text resource2_name, resource2_buy, resource2_sell;
    // Start is called before the first frame update
    public override void Init()
    {
        parentLayer = GameObject.Find("TradingLayer");

        resource1_name = GameObject.Find("trading_resource1_name").GetComponent<Text>();
        resource1_buy = GameObject.Find("trading_resource1_buy").GetComponent<Text>();
        resource1_sell = GameObject.Find("trading_resource1_sell").GetComponent<Text>();

        resource2_name = GameObject.Find("trading_resource2_name").GetComponent<Text>();
        resource2_buy = GameObject.Find("trading_resource2_buy").GetComponent<Text>();
        resource2_sell = GameObject.Find("trading_resource2_sell").GetComponent<Text>();
        
        close = GameObject.Find("trading_close").GetComponent<Button>();
        close.onClick.AddListener(DisposePopup);
    }
    public override void Show()
    {
        parentLayer.SetActive(true);
    }
    public override void Close()
    {
        parentLayer.SetActive(false);
    }

    private void DisposePopup()
    {
        Close();
        Context.Instance.SetMode(Context.Mode.NONE);
    }

    // Update is called once per frame
    public override void UpdateUI()
    {
        resource1_name.text = MetaManager.Instance.resourceInfo[1];
        resource1_buy.text = Util.GetCurrencyString(TradingManager.Instance.GetBuyPrice(1));
        resource1_sell.text = Util.GetCurrencyString(TradingManager.Instance.GetSellPrice(1));

        resource2_name.text = MetaManager.Instance.resourceInfo[2];
        resource2_buy.text = Util.GetCurrencyString(TradingManager.Instance.GetBuyPrice(2));
        resource2_sell.text = Util.GetCurrencyString(TradingManager.Instance.GetSellPrice(2));
    }
}
