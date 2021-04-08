using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class GamePlay : MonoBehaviour
{
    public Camera camera;
    public Transform canvas;
    private GameObject buildingLayer, actorLayer;

    //a*
    List<int> route = new List<int>();
    float time = 0;
    GameObject actor;
    // Start is called before the first frame update
    void Start()
    {
        Context.Instance.Init(ref canvas, 
                                "progress_default", 
                                "CubeGreen", 
                                "CubeRed", 
                                "select_ui", 
                                "select_ui_actor", 
                                OnClickForCreatingActor,
                                OnClickForUpgradingActor
                                );
        
        LoaderPerspective.Instance.SetUI(camera, ref canvas, OnClickButton);
        if(!LoaderPerspective.Instance.LoadJsonFile("ui"))
        {
            Debug.LogError("ui.json loading failure");
        } 
        else
        {
            LoaderPerspective.Instance.AddComponents(OnCreate, OnCreatePost);
            buildingLayer = GameObject.Find("buildings");
            actorLayer = GameObject.Find("actors");

            HideLayers();
        }

        actor = GameObject.Find("actor");
    }
    void HideLayers()
    {
        buildingLayer.SetActive(false);
        actorLayer.SetActive(false);
    }
    /*
        Set scroll view 
    */
    void OnCreatePost(GameObject obj, string layerName)
    {
        switch(obj.name)
        {
            case "scrollview_building":
                LoaderPerspective.Instance.CreateScrollViewItems(GetBuildingScrollItems(), 15, obj.GetComponent<RectTransform>().sizeDelta, OnClickButton, obj);
                break;
            case "scrollview_actor":
                LoaderPerspective.Instance.CreateScrollViewItems(GetActorScrollItems(), 15, obj.GetComponent<RectTransform>().sizeDelta, OnClickButton, obj);
                break;
            default:
                break;
        }
    }
     List<GameObject> GetBuildingScrollItems()
    {
        List<GameObject> list = new List<GameObject>();
        for(int n = 0; n < MetaManager.Instance.meta.buildings.Count; n++)
        {
            GameObject obj = Resources.Load<GameObject>("button_default");
            Meta.Building buildingInfo = MetaManager.Instance.meta.buildings[n];
            obj.GetComponentInChildren<Text>().text = buildingInfo.name;
            obj.name = string.Format("building-{0}", buildingInfo.id);
            list.Add(Instantiate(obj));
        }

        return list;
    }

    List<GameObject> GetActorScrollItems()
    {
        List<GameObject> list = new List<GameObject>();
        for(int n = 0; n < MetaManager.Instance.meta.actors.Count; n++)
        {
            GameObject obj = Resources.Load<GameObject>("button_default");
            Meta.Actor info = MetaManager.Instance.meta.actors[n];
            obj.GetComponentInChildren<Text>().text = info.name;
            obj.name = string.Format("actor-{0}", info.id);
            list.Add(Instantiate(obj));
        }

        return list;
    }
    /*
        OnClick 처리
    */
    void OnClickForCreatingActor(int mapId, int buildingId)
    {
        //Debug.Log(string.Format("{0}-{1}", mapId, buildingId));
        actorLayer.SetActive(true);
        Context.Instance.SetMode(Context.Mode.UI_ACTOR);
        ((ContextCreatingActor)Context.Instance.contexts[Context.Mode.UI_ACTOR]).SetSelectedBuilding(mapId, buildingId);
    }
    void OnClickForUpgradingActor(int mapId, int actorId)
    {
        Debug.Log(string.Format("OnClickForUpgradingActor {0}-{1}", mapId, actorId));
    }
    void OnClickButton(GameObject obj)
    {
        Debug.Log("OnClick " + obj.name);
        string name = obj.name.Replace("(Clone)", "");
        switch(Context.Instance.mode)
        {
            case Context.Mode.NONE:
                OnClick_NONE(obj, name);
                break;
            case Context.Mode.UI_BUILD:
                OnClick_UI_BUILD(obj, name);
                break;
            case Context.Mode.UI_ACTOR:
                OnClick_UI_ACTOR(obj, name);
                break;
            default:
                break;
        }
    }

    void OnClick_NONE(GameObject obj, string name)
    {
        switch(name)
        {
            case "btn_building":
                buildingLayer.SetActive(true);
                Context.Instance.SetMode(Context.Mode.UI_BUILD);
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

    void OnClick_UI_BUILD(GameObject obj, string name)
    {
        string[] arr = name.Split('-');
        if(arr.Length >= 2 && arr[0] == "building")
        {
            int id = int.Parse(arr[1]);
            Context.Instance.SetMode(Context.Mode.BUILD);
            ((ContextBuild)Context.Instance.contexts[Context.Mode.BUILD]).SetBuildingId(id);
            HideLayers();
        }
    }

    void OnClick_UI_ACTOR(GameObject obj, string name)
    {
        string[] arr = name.Split('-');
        if(arr.Length >= 2 && arr[0] == "actor")
        {
            int id = int.Parse(arr[1]);
            //actor에 대한 cost 처리
            int mapId = ((ContextCreatingActor)Context.Instance.contexts[Context.Mode.UI_ACTOR]).selectedMapId;

            Updater.Instance.AddQ(ActionType.ACTOR_CREATE, mapId, id, null);
            Context.Instance.SetMode(Context.Mode.NONE);
            HideLayers();
        }
    }

    // UI canceling
    void Update()
    {
        /*
        foreach(KeyValuePair<int, Actor> kv in ActorManager.Instance.actors)
        {
            kv.Value.gameObject.GetComponent<Animator>().SetInteger("Speed", 2);
        }
        */

        if(Input.GetMouseButtonUp(0))
        {
            /*
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
                //Debug.Log(MapManager.Instance.GetMapPosition(id));
            }
            */
            //------------------------------
            switch(Context.Instance.mode)
            {
                case Context.Mode.UI_BUILD:
                case Context.Mode.UI_ACTOR:
                    if(!EventSystem.current.IsPointerOverGameObject()) //UI가 클릭되지 않은 경우
                    {
                        HideLayers();
                        Context.Instance.SetMode(Context.Mode.NONE);
                    }
                    break;
                default:
                    break;
            }
        }
        //a*
        /*
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
        */
    }
     GameObject OnCreate(string layerName,string name, string tag, Vector2 position, Vector2 size)
    {
        return null;
    } 
}
