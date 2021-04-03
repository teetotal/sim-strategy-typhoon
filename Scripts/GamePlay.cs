using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
            Meta.Building buildingInfo = MetaManager.Instance.meta.buildings[n];
            obj.GetComponentInChildren<Text>().text = buildingInfo.name;
            obj.name = string.Format("building-{0}", buildingInfo.id);
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
        string name = obj.name.Replace("(Clone)", "");
        switch(name)
        {
            case "building":
                buildingLayer.SetActive(true);
                Context.Instance.mode = Context.Mode.UI_BUILD;
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
        string[] arr = name.Split('-');
        if(arr.Length >= 2)
        {
            if(arr[0] == "building")
            {
                int id = int.Parse(arr[1]);
                
                Context.Instance.SetMode(Context.Mode.BUILD);
                ((ContextBuild)Context.Instance.contexts[Context.Mode.BUILD]).SetBuildingId(id);
                buildingLayer.SetActive(false);
                
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            switch(Context.Instance.mode)
            {
                case Context.Mode.UI_BUILD:
                    if(!EventSystem.current.IsPointerOverGameObject()) //UI가 클릭되지 않은 경우
                    {
                        buildingLayer.SetActive(false);
                        Context.Instance.SetMode(Context.Mode.NONE);
                    }
                    break;
                default:
                    break;
            }
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
