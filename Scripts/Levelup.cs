using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Levelup : MonoBehaviour
{
    public Transform canvas;
    float elapse = 0;
    bool isStart = false;
    GameObject slider, shaman, scrollviewMaterial;
    void Awake()
    {
        LoaderPerspective.Instance.SetUI(Camera.main, ref canvas, OnClickButton);
        if(!LoaderPerspective.Instance.LoadJsonFile("levelup"))
        {
            Debug.LogError("levelup.json loading failure");
        } 
        else
        {
            LoaderPerspective.Instance.AddComponents(OnCreate, OnCreatePost);
        }

        shaman = GameObject.Find("shaman"); //.GetComponent<Animator>().SetBool("levelUp", true);
        SetMaterialScrollview();
    }
    void Update()
    {
        if(!isStart)
            return;
        elapse += Time.deltaTime;
        if(elapse > 6)
        {
            Camera.main.GetComponent<Animation>().Stop();
            if(GachaManager.Instance.Run())
            {
                //성공 이벤트
            }
            else
            {
                //실패 이벤트
            }
            elapse = 0;
            isStart = false;
            shaman.GetComponent<Animator>().SetBool("levelUp", false);
            shaman.GetComponent<Animator>().SetBool("idle", true);
            SetMaterialScrollview();
            SetSlider();
            //SceneManager.LoadScene("GamePlay");
        }
        else if(elapse > 4)
        {
            Camera.main.GetComponent<Animation>().Play();
            
        }
    }

    void OnCreatePost(GameObject obj, string layerName)
    {
        switch(obj.name)
        {
            case "scrollview":
                scrollviewMaterial = obj;
                //SetMaterialScrollview();
                break;
            case "scrollview_actors":
                LoaderPerspective.Instance.CreateScrollViewItems(GeScrollItemsActors(), 10, OnClickButton, obj, false);
                break;
            case "slider":
                slider = obj;
                SetSlider();
                break;
            default:
                break;
        }
    }
    void OnClickButton(GameObject obj)
    {
        //Debug.Log(string.Format("OnClick {0}, {1}", obj.name, Context.Instance.mode));
        string name = Util.GetObjectName(obj);
        switch(name)
        {
            case "levelup":
                shaman.GetComponent<Animator>().SetBool("levelUp", true);
                isStart = true;
            break;
            case "play":
                SceneManager.LoadScene("GamePlay");
            break;
            default:
            break;
        }
    }
    GameObject OnCreate(string layerName,string name, string tag, Vector2 position, Vector2 size)
    {
        return null;
    }
    void OnChangeMaterialQuantity(GameObject obj, int itemId, bool isAdd)
    {
        string name = ItemManager.Instance.items[itemId].name;
        int quantity = InventoryManager.Instance.items[itemId];
        int added = GachaManager.Instance.GetAssignedMaterialCount(itemId);

        if(isAdd)
        {
            if(added >= quantity)
                return;
            GachaManager.Instance.AddMaterial(itemId);
        }
        else
        {
            if(added <= 0)
                return;
            GachaManager.Instance.SubtractMaterial(itemId);
        }
            
        added = GachaManager.Instance.GetAssignedMaterialCount(itemId);

        obj.GetComponentInChildren<Text>().text = string.Format("{0} {1}/{2}", name, added, quantity);
        SetSlider();
    }

    void SetMaterialScrollview()
    {
        GameObject content = scrollviewMaterial.transform.Find("Viewport").transform.Find("Content").gameObject;
        for(int n = 0; n < content.transform.childCount; n++)
        {
            GameObject.Destroy(content.transform.GetChild(n).gameObject);
        }
        //content.transform.DetachChildren();
        
        LoaderPerspective.Instance.CreateScrollViewItems(GeScrollItems(), 10, OnClickButton, scrollviewMaterial, false);
    }

    void SetSlider()
    {
        float v = Mathf.Min(1, GachaManager.Instance.GetSuccessProbability());
        slider.GetComponent<Slider>().value = v;
        slider.GetComponentInChildren<Text>().text = string.Format("{0}%", Mathf.Round(v*100));
    }
    //----------------------------------------------------------
    List<GameObject> GeScrollItems()
    {
        List<GameObject> list = new List<GameObject>();
        
        foreach(KeyValuePair<int, int> kv in InventoryManager.Instance.items)
        {
            if(kv.Value == 0)
                continue;

            Item item = ItemManager.Instance.items[kv.Key];

            GameObject obj = Resources.Load<GameObject>("LevelUp/levelup_element");
            obj = Instantiate(obj);
            //GameObject obj = Resources.Load<GameObject>("button_default");
            obj.GetComponentInChildren<Text>().text = string.Format("{0} 0/{1}", item.name, kv.Value);
            obj.name = string.Format("item-{0}", kv.Key);

            Button[] btns = obj.GetComponentsInChildren<Button>();
            Button increase = btns[0];
            Button decrease = btns[1];
            increase.onClick.AddListener(() => { OnChangeMaterialQuantity(obj, kv.Key, true);});
            decrease.onClick.AddListener(() => { OnChangeMaterialQuantity(obj, kv.Key, false);});

            list.Add(obj); 
        }

        return list;
    }
    List<GameObject> GeScrollItemsActors()
    {
        List<GameObject> list = new List<GameObject>();
        foreach(KeyValuePair<int, Actor> kv in ActorManager.Instance.actors)
        {
            GameObject obj = Resources.Load<GameObject>("button_default");
            //GameObject obj = Resources.Load<GameObject>("button_default");
            if(kv.Value.tribeId == 0)
            {
                Meta.Actor meta = MetaManager.Instance.actorInfo[kv.Value.id];
                obj.GetComponentInChildren<Text>().text = meta.name + " lv." + kv.Value.level.ToString();
                obj.name = string.Format("item-{0}", kv.Key);
                list.Add(Instantiate(obj)); 
            }
            
        }

        return list;
    }
}