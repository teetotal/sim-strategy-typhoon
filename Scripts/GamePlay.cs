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

    //a*
    List<int> route = new List<int>();
    float time = 0;
    GameObject actor;
    // Start is called before the first frame update
    void Start()
    {
        Context.Instance.Init(ref canvas, "progress_default", "CubeGreen", "CubeRed");
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

        actor = GameObject.Find("actor");
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
            time = 0;
            //Astar test
            route.Clear();
            Astar astar = new Astar(MapManager.Instance.map);
            Stack<Astar.Pos> stack = astar.Search(new Astar.Pos(1, 3), new Astar.Pos(12, 6));
            if(stack == null)
                return;
            
            while(stack.Count > 0)
            {
                int id = MapManager.Instance.GetMapId(new Vector2Int(stack.Peek().x, stack.Peek().y));
                route.Add(id);
                stack.Pop();
                Debug.Log(MapManager.Instance.GetMapPosition(id));
                
            }
            //------------------------------
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
        //a*
        time += Time.deltaTime;
        int idx = (int)time;
        if(idx > route.Count - 2)
            return;
        float ratio = time % 1.0f;
        Vector3 posNext;
        Vector3 pos = MapManager.Instance.GetVector3FromMapId(route[idx]);//GetRoutePosition(idx);
        if(idx < route.Count - 3)
        {
            actor.GetComponent<Animator>().SetInteger("Speed", 2);
            posNext = MapManager.Instance.GetVector3FromMapId(route[idx + 1]);//GetRoutePosition(idx + 1);
        } 
        else if(idx <= route.Count - 2)
        {
            actor.GetComponent<Animator>().SetInteger("Speed", 2);
            posNext = MapManager.Instance.GetVector3FromMapId(route[idx + 1]);
        }
        else
        {
            return;
        }

        //Vector3 diff = (posNext - pos) * ratio * 1.0f;
        actor.transform.position = Vector3.Lerp(pos, posNext, ratio) + new Vector3(0, 0.1f, 0);//pos + diff + new Vector3(0, 0.1f, 0);

        Vector3 target = posNext + new Vector3(0, 0.1f, 0);
        
        Vector3 dir = target - actor.transform.position;
        actor.transform.rotation = Quaternion.Lerp(actor.transform.rotation, Quaternion.LookRotation(dir), ratio);
    }

    Vector3 GetRoutePosition(int id)
    {
        Vector3 pos = MapManager.Instance.GetVector3FromMapId(route[id]);
        if(id > 0)
        {
            Vector3 posPre = MapManager.Instance.GetVector3FromMapId(route[id - 1]);
            Vector3 posNext = MapManager.Instance.GetVector3FromMapId(route[id + 1]);
            return posPre + ((posNext - posPre) * 0.5f);
        }
        else
        {
            return pos;
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
