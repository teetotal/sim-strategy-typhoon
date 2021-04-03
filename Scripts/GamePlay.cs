using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GamePlay : MonoBehaviour
{
    public Camera camera;
    public Transform canvas;
    private GameObject buildingLayer;
    private List<GameObject> listBuildingItems = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        for(int n = 0; n < MetaManager.Instance.meta.buildings.Count; n++)
        {
            GameObject obj = Resources.Load<GameObject>("button_default");
            obj.GetComponentInChildren<Text>().text = MetaManager.Instance.meta.buildings[n].name;
            obj.name = "building-" + MetaManager.Instance.meta.buildings[n].id.ToString();
            listBuildingItems.Add(Instantiate(obj));
        }
        
        LoaderPerspective.Instance.SetUI(camera, ref canvas, OnClickButton);
        if(!LoaderPerspective.Instance.LoadJsonFile("ui"))
        {
            Debug.LogError("ui.json loading failure");
        } 
        else
        {
            LoaderPerspective.Instance.AddComponents(OnCreate, OnCreatePost);
            buildingLayer = GameObject.Find("buildings");
            buildingLayer.SetActive(false);
        }
        
    }
    void OnCreatePost(GameObject obj, string layerName)
    {
        switch(obj.name)
        {
            case "scrollview":
                LoaderPerspective.Instance.CreateScrollViewItems(listBuildingItems, 15, obj.GetComponent<RectTransform>().sizeDelta, OnClickButton);
                break;
            default:
                break;
        }
    }

    GameObject OnCreate(string layerName,string name, string tag, Vector2 position, Vector2 size)
    {
        return null;
    } 

    void OnClickButton(GameObject obj)
    {
        Debug.Log("OnClick " + obj.name);
        switch(obj.name)
        {
            case "building":
                buildingLayer.SetActive(true);
                Context.Instance.mode = Context.Mode.UI_BUILD;
                break;
            case "building_close":
                buildingLayer.SetActive(false);
                break;
            case "zoomin":
                if(Camera.main.fieldOfView > 10)
                    Camera.main.fieldOfView -= 5;
                break;
            case "zoomout":
                if(Camera.main.fieldOfView < 25)
                    Camera.main.fieldOfView += 5;
                break;
            default:
                break;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            GameObject obj = Touch.Instance.GetTouchedObject3D();
            Debug.Log(obj);
        }
    }
    
    void DisplayEvent(List<Object> list)
    {
        if(list == null)
        {
            return;
        }
        //display event
        for(int n = 0; n < list.Count; n++)
        {
            Object o = list[n];
            if(o is Actor)
            {
                EventActor((Actor)o);
            }
            else if(o is BuildingObject)
            {
                EventBuilding((BuildingObject)o);
            }
            else if(o is Node)
            {
                EventNode((Node)o);
            }
        }
    }

    void EventActor(Actor actor)
    {

    }

    void EventBuilding(BuildingObject building)
    {

    }

    void EventNode(Node node)
    {

    }
    
}
