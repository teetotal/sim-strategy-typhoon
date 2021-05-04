using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIInventory : IUIInterface
{
    Button close;
    int myTribeId;
    LoaderButtonOnClickCallBack onClickButton;
    GameObject inventory_scrollview;
    GameObject parentLayer;
    // Start is called before the first frame update
    public override void Init()
    {
        parentLayer = GameObject.Find("InventoryLayer");
        close = GameObject.Find("inventory_close").GetComponent<Button>();

        inventory_scrollview = GameObject.Find("inventory_scrollview");

        close.onClick.AddListener( Close );
        
    }

    public void Init(int myTribeId, LoaderButtonOnClickCallBack onClickButton)
    {
        this.myTribeId = myTribeId;
        this.onClickButton = onClickButton;
    }

    // Update is called once per frame
    public override void UpdateUI()
    {
        LoaderPerspective.Instance.CreateScrollViewItems(GetInventoryItems()
                    , new Vector2(15, 15)
                    , new Vector2(10, 10)
                    , this.onClickButton
                    , inventory_scrollview
                    , 4);
    }
    public override void Show()
    {
        parentLayer.SetActive(true);
    }
    public override void Close()
    {
        parentLayer.SetActive(false);
    }
    List<GameObject> GetInventoryItems()
    {
        List<GameObject> list = new List<GameObject>();
        foreach(KeyValuePair<int, int> kv in InventoryManager.Instance.GetInventory(myTribeId))
        {
            GameObject obj = GameObjectPooling.Instance.Get("inventory_default");
            Item item = ItemManager.Instance.items[kv.Key];
            obj.GetComponentInChildren<RawImage>().texture = Resources.Load<Sprite>(item.prefab).texture;
            obj.transform.Find("Text").GetComponent<Text>().text = "x"+kv.Value.ToString();
            obj.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = item.name;
            list.Add(obj);
        }
        return list;
    }
}
