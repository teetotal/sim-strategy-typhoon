using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Levelup : MonoBehaviour
{
    public Transform canvas;
    float elapse = 0;
    bool isStart = false;
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
    }
    void Update()
    {
        if(!isStart)
            return;
        elapse += Time.deltaTime;
        if(elapse > 6)
        {
            SceneManager.LoadScene("GamePlay");
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
                LoaderPerspective.Instance.CreateScrollViewItems(GeScrollItems(), 10, OnClickButton, obj, false);
                break;
            case "scrollview_actors":
                LoaderPerspective.Instance.CreateScrollViewItems(GeScrollItemsActors(), 10, OnClickButton, obj, false);
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
                GameObject.Find("shaman").GetComponent<Animator>().SetBool("levelUp", true);
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
    //----------------------------------------------------------
    List<GameObject> GeScrollItems()
    {
        List<GameObject> list = new List<GameObject>();
        for(int n = 0; n < 10; n++)
        {
            GameObject obj = Resources.Load<GameObject>("LevelUp/levelup_element");
            //GameObject obj = Resources.Load<GameObject>("button_default");
            obj.GetComponentInChildren<Text>().text = n.ToString()+" A급 재료";
            obj.name = string.Format("item-{0}", n);
            list.Add(Instantiate(obj)); 
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