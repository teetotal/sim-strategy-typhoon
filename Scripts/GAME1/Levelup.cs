using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//최고 레벨인지 체크하는 로직 필요
public class Levelup : MonoBehaviour
{
    float accumulate = 50; 
    Vector3 defaultCameraPos;
    GameObject messageArea;
    Text message;

    public Transform canvas;
    float elapse = 0;
    bool isStart = false;
    GameObject slider, shaman, scrollviewMaterial;
    GameObject[] slots;
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

        message = GameObject.Find("message").GetComponent<Text>();
        SetMessageTargetInfo();

        slots = new GameObject[5] {
            GameObject.Find("slot-1"),
            GameObject.Find("slot-2"),
            GameObject.Find("slot-3"),
            GameObject.Find("slot-4"),
            GameObject.Find("slot-5")
        };
        for(int n = 0; n < slots.Length; n++)
        {
            slots[n].SetActive(false);
        }

        shaman = GameObject.Find("shaman"); //.GetComponent<Animator>().SetBool("levelUp", true);
        SetMaterialScrollview();
        SetSlider();

        defaultCameraPos = Camera.main.transform.position;
    }
    void Update()
    {
        if(!isStart)
            return;
        accumulate++;
        float amount = Time.deltaTime * accumulate;
        elapse += amount;
        //float ratio = Time.deltaTime * 100;
        //elapse += 0.1f;
        if(elapse >= 360 * 3)
        {
            Camera.main.transform.position = defaultCameraPos;
            Camera.main.transform.rotation = Quaternion.Euler(0, 0, 0);
            //Debug.Log(elapse);
            //Camera.main.GetComponent<Animation>().Stop();
            if(GachaManager.Instance.Levelup(GachaManager.Instance.target.tribeId))
            {
                //성공 이벤트
                SetMessage("성공");
            }
            else
            {
                //실패 이벤트
                SetMessage("실패");
            }
            elapse = 0;
            accumulate = 50;
            isStart = false;
            shaman.GetComponent<Animator>().SetBool("levelUp", false);
            shaman.GetComponent<Animator>().SetBool("idle", true);
            SetMaterialScrollview();
            SetSlider();
            //SceneManager.LoadScene("GamePlay");
        }
        else //if(elapse > 4)
        {
            //Camera.main.GetComponent<Animation>().Play();
            //Camera.main.fieldOfView = 20;// - (5 * cmRatio);
            Camera.main.transform.RotateAround(shaman.transform.position, 
                                                shaman.transform.rotation.eulerAngles, 
                                                amount
                                                );
        }
    }

    void OnCreatePost(GameObject obj, string layerName)
    {
        switch(obj.name)
        {
            case "message":
                break;
            case "scrollview":
                scrollviewMaterial = obj;
                //SetMaterialScrollview();
                break;
            case "scrollview_actors":
                LoaderPerspective.Instance.CreateScrollViewItems(GeScrollItemsActors()
                                                                , new Vector2(15, 15)
                                                                , new Vector2(10, 10)
                                                                , OnClickButton
                                                                , obj
                                                                , 1);
                break;
            case "slider":
                slider = obj;
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
        int assignedMaterials = GachaManager.Instance.GetAssignedMaterialCount();
        if(isAdd && assignedMaterials >= 5)
            return;
        if(!isAdd && assignedMaterials <= 0)
            return;

        string name = ItemManager.Instance.items[itemId].name;
        int quantity = InventoryManager.Instance.items[GachaManager.Instance.target.tribeId][itemId];
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
        
        LoaderPerspective.Instance.CreateScrollViewItems(GeScrollItems()
                                                        , new Vector2(15, 15)
                                                        , new Vector2(10, 10)
                                                        , OnClickButton
                                                        , scrollviewMaterial
                                                        , 1);
    }

    void SetSlider()
    {
        if(GachaManager.Instance.target != null)
        {
            float v = Mathf.Min(1, GachaManager.Instance.GetSuccessProbability());
            slider.GetComponent<Slider>().value = v;
            slider.GetComponentInChildren<Text>().text = string.Format("{0}%", Mathf.Round(v*100));

            //icon
            for(int n = 0; n < slots.Length; n++)
            {
                slots[n].SetActive(false);
            }
            List<int> list = GachaManager.Instance.GetAssignedMaterialList();
            for(int n = 0; n < list.Count; n++)
            {
                slots[n].SetActive(true);
                slots[n].GetComponent<RawImage>().texture = Resources.Load<Sprite>(ItemManager.Instance.items[list[n]].prefab).texture;
            }
        }
        else
        {
            slider.GetComponent<Slider>().value = 0;
            slider.GetComponentInChildren<Text>().text = "";
        }
        
    }
    //----------------------------------------------------------
    List<GameObject> GeScrollItems()
    {
        List<GameObject> list = new List<GameObject>();
        Dictionary<int, int> items = InventoryManager.Instance.GetInventory(GachaManager.Instance.target.tribeId);
        if(items == null)
            return list;
        
        foreach(KeyValuePair<int, int> kv in items)
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

            Sprite sprite = Resources.Load<Sprite>(item.prefab);
            //sprite = Instantiate(sprite);
            //GameObject.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("MagicpotionsFree/potionblue01");
            obj.GetComponentInChildren<RawImage>().texture = sprite.texture;

            list.Add(obj); 
        }

        return list;
    }
    List<GameObject> GeScrollItemsActors()
    {
        List<GameObject> list = new List<GameObject>();
        List<int> seqs = ObjectManager.Instance.GetObjectSeqs(TAG.ACTOR);
        for(int n = 0; n < seqs.Count; n++)
        {
            int seq = seqs[n];
            Object p = ObjectManager.Instance.Get(seq);

            GameObject obj = Resources.Load<GameObject>("button_default");
            if(p.tribeId == GachaManager.Instance.target.tribeId)
            {
                Meta.Actor meta = MetaManager.Instance.actorInfo[p.id];
                obj.GetComponentInChildren<Text>().text = meta.name + " lv." + p.level.ToString();
                obj.name = string.Format("item-{0}", p.seq);
                list.Add(Instantiate(obj)); 
            }  
        }
        /*
        foreach(KeyValuePair<int, Actor> kv in ActorManager.Instance.actors)
        {
            GameObject obj = Resources.Load<GameObject>("button_default");
            //GameObject obj = Resources.Load<GameObject>("button_default");
            if(kv.Value.tribeId == GachaManager.Instance.target.tribeId)
            {
                Meta.Actor meta = MetaManager.Instance.actorInfo[kv.Value.id];
                obj.GetComponentInChildren<Text>().text = meta.name + " lv." + kv.Value.level.ToString();
                obj.name = string.Format("item-{0}", kv.Key);
                list.Add(Instantiate(obj)); 
            }  
        }
        */
        return list;
    }

    private void SetMessageTargetInfo()
    {
        //target 정보
        if(GachaManager.Instance.target != null)
        {
            string name = MetaManager.Instance.actorInfo[GachaManager.Instance.target.id].name;
            SetMessage(string.Format("{0} Lv.{1}", name, GachaManager.Instance.target.level));
        }
        else
        {
            SetMessage("");
        }
    }

    private void SetMessage(string sz)
    {
        message.text = sz;
    }
}