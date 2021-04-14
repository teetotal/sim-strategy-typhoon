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
    //float time = 0;
    GameObject actor_scrollview;
    // Start is called before the first frame update
    void Start()
    {
        GameObject o = GameObject.Find("Dragon");
        o.GetComponentInChildren<Renderer>().material.color = Color.grey;

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

        //UI
        string[] arr = new string[4] {"select_ui_building", "select_ui_actor", "text_title", "text_title"};
        GameObject[] objs = new GameObject[4];
        for(int n = 0; n < arr.Length; n++)
        {
            objs[n] = GameObject.Instantiate(Resources.Load<GameObject>(arr[n]));
            objs[n].transform.SetParent(canvas);
        }

        //for building
        Button[] btns = objs[0].GetComponentsInChildren<Button>();
        for(int n = 0; n < btns.Length; n++)
        {
            Button obj = btns[n];
            obj.onClick.AddListener(()=>{ OnClickButton(obj.gameObject); });
        }
        
        //for actor
        Button btn = objs[1].GetComponentInChildren<Button>();
        btn.onClick.AddListener(()=>{ OnClickButton(btn.gameObject);});


        Context.Instance.Init( OnActorEvent,
                                ref canvas, 
                                "progress_default", 
                                "text_default",
                                "CubeGreen", 
                                "CubeRed", 
                                objs[2],
                                objs[0],
                                objs[3],
                                objs[1]
                                );
    }
    void HideLayers()
    {
        buildingLayer.SetActive(false);
        actorLayer.SetActive(false);

        
        GameObject.DestroyImmediate(actor_scrollview);
    }
    /*
        Set scroll view 
    */
    void OnCreatePost(GameObject obj, string layerName)
    {
        switch(obj.name)
        {
            case "scrollview_building":
                LoaderPerspective.Instance.CreateScrollViewItems(GetBuildingScrollItems(), 15, OnClickButton, obj);
                break;
            case "scrollview_actor":
                actor_scrollview = obj;
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

    List<GameObject> GetActorScrollItems(int buildingId)
    {
        List<GameObject> list = new List<GameObject>();
        for(int n = 0; n < MetaManager.Instance.meta.buildings[buildingId].actors.Count; n++)
        {
            GameObject obj = Resources.Load<GameObject>("button_default");
            Meta.ActorIdMax actorIdMax = MetaManager.Instance.meta.buildings[buildingId].actors[n];
            Meta.Actor info = MetaManager.Instance.meta.actors[actorIdMax.actorId];
            obj.GetComponentInChildren<Text>().text = string.Format("{0}\nLv.{1}", info.name, info.level);
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
        if(actorLayer.activeSelf == true)
            return;

        actor_scrollview = LoaderPerspective.Instance.CreateByPrefab("scrollview_default", 
                                                                        actorLayer.transform, 
                                                                        actorLayer.GetComponent<RectTransform>().sizeDelta,
                                                                        actorLayer.transform.position
                                                                        );
        LoaderPerspective.Instance.CreateScrollViewItems(GetActorScrollItems(buildingId), 15, OnClickButton, actor_scrollview);
                
        actorLayer.SetActive(true);
        Context.Instance.SetMode(Context.Mode.UI_ACTOR);
        ((ContextCreatingActor)Context.Instance.contexts[Context.Mode.UI_ACTOR]).SetSelectedBuilding(mapId, buildingId);
    }
    void OnClickForUpgradingActor(int mapId, int actorId)
    {
        Actor actor = ActorManager.Instance.actors[mapId];
        Meta.Actor meta = MetaManager.Instance.actorInfo[actor.id];
        //짐 싣기 테스트 코드
        GameObject selectedObj = actor.gameObject;
        Debug.Log(string.Format("OnClickForUpgradingActor {0}-{1}", mapId, actorId));
        GameObject o = Resources.Load<GameObject>("load");
        o = GameObject.Instantiate(o);
        
        o.transform.SetParent(selectedObj.transform);

        Vector3 size = selectedObj.GetComponent<BoxCollider>().size;
        o.transform.localPosition = new Vector3(0, size.y, 0);

        ActionType type = meta.flying ? ActionType.ACTOR_FLYING: ActionType.ACTOR_MOVING;
        //routine 추가
        actor.SetRoutine(new List<QNode>()
        {
            new QNode(type, mapId, 0, null),
            new QNode(type, mapId, 255, null)
        });
        ((ContextActor)Context.Instance.contexts[Context.Mode.ACTOR]).Clear();
        Context.Instance.SetMode(Context.Mode.NONE);
    }
    void OnClickButton(GameObject obj)
    {
        Debug.Log(string.Format("OnClick {0}, {1}", obj.name, Context.Instance.mode));
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
            case Context.Mode.ACTOR:
                OnClick_ACTOR(obj, name);
                break;
            default:
                break;
        }
    }

    void OnClick_NONE(GameObject obj, string name)
    {
        ContextNone context = (ContextNone)Context.Instance.contexts[Context.Mode.NONE];
        switch(name)
        {
            case "btn_building":
                buildingLayer.SetActive(true);
                Context.Instance.SetMode(Context.Mode.UI_BUILD);
                break;
            case "zoomin":
                if(Camera.main.fieldOfView > 5)
                    Camera.main.fieldOfView -= 5;
                break;
            case "zoomout":
                if(Camera.main.fieldOfView < 25)
                    Camera.main.fieldOfView += 5;
                break;
            case "buttonX":
                if(context.selectedObj)
                {
                    if(BuildingManager.Instance.objects[context.GetSelectedMapId()].actions.Count == 0)
                        Updater.Instance.AddQ(ActionType.BUILDING_DESTROY, context.GetSelectedMapId(), context.GetSelectedBuildingId(), null);
                    else
                        Debug.Log("The Building has actions");
                }
                break;
            case "buttonR":
                if(context.selectedObj)
                {
                    Vector3 angles = context.selectedObj.transform.localEulerAngles;
                    context.selectedObj.transform.localEulerAngles = new Vector3(angles.x, angles.y + 90, angles.z);
                }
                break;
            case "buttonB":
                if(context.selectedObj)
                {
                    int mapId = context.GetSelectedMapId();
                    int buildingId = context.GetSelectedBuildingId();
                    OnClickForCreatingActor(mapId, buildingId);
                }
                break;
            
            default:
                break;
        }
    }
    void OnClick_ACTOR(GameObject obj, string name)
    {
        ContextNone context = (ContextNone)Context.Instance.contexts[Context.Mode.NONE];
        switch(name)
        {
            case "buttonU":
                if(context.selectedObj)
                {
                    int mapId = context.GetSelectedMapId();
                    int actorId = context.GetSelectedActorId();
                    OnClickForUpgradingActor(mapId, actorId);
                }
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
    void OnActorEvent(Actor actor, string targetTag, int targetMapId)
    {
        //Debug.Log(string.Format("OnActorEvent {0} -> {1}, {2}", actor.mapId, targetTag, targetMapId));
        switch(targetTag)
        {
            case "Environment":
            break;
            case "Building":
            GameSystem.Instance.OnTargetingBuilding(actor, targetMapId);
            break;
            case "Actor":
            break;
            case "Mob":
            break;
            case "Bottom":
            break;
            default:
            break;
        }
    }

    // UI canceling
    
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            //------------------------------
            switch(Context.Instance.mode)
            {
                case Context.Mode.UI_BUILD:
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
    }
    
     GameObject OnCreate(string layerName,string name, string tag, Vector2 position, Vector2 size)
    {
        return null;
    } 
}
