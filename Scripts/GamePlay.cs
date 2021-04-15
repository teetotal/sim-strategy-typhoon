using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class GamePlay : MonoBehaviour
{
    public Transform canvas;
    private GameObject buildingLayer, actorLayer;

    //a*
    List<int> route = new List<int>();
    //float time = 0;
    GameObject actor_scrollview;
    // Start is called before the first frame update
    void Start()
    {
        GameSystem.Instance.Init();

        LoaderPerspective.Instance.SetUI(Camera.main, ref canvas, OnClickButton);
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

        InitSelectionUI();

        //Load save game status
        foreach(KeyValuePair<int, GameStatus.Building> kv in GameSystem.Instance.gameStatus.buildingInfo)
        {
            Updater.Instance.AddQ(ActionType.BUILDING_CREATE, kv.Key, kv.Value.buildingId, null, true);
            for(int n = 0; n < kv.Value.actors.Count; n++)
            {
                GameStatus.MapIdActorId p = kv.Value.actors[n];
                Updater.Instance.AddQ(ActionType.ACTOR_CREATE, p.mapId, p.actorId, null, true);
            }
        }
        
        //Context
        Context.Instance.Init(  OnCreationEvent,
                                OnSelected,
                                OnAction,
                                ref canvas, 
                                "progress_default", 
                                "text_default",
                                "CubeGreen", 
                                "CubeRed"
                                );
    }
    void InitSelectionUI()
    {
        //UI
        string[] arr = new string[4] {"text_title", "select_ui_building", "text_title", "select_ui_actor"};
        GameObject[] uiObjs = new GameObject[4];
        for(int n = 0; n < arr.Length; n++)
        {
            uiObjs[n] = GameObject.Instantiate(Resources.Load<GameObject>(arr[n]));
            uiObjs[n].transform.SetParent(canvas);
        }

        //for building
        Button[] btns = uiObjs[1].GetComponentsInChildren<Button>();
        for(int n = 0; n < btns.Length; n++)
        {
            Button obj = btns[n];
            obj.onClick.AddListener(()=>{ OnClickButton(obj.gameObject); });
        }
        
        //for actor
        Button btn = uiObjs[3].GetComponentInChildren<Button>();
        btn.onClick.AddListener(()=>{ OnClickButton(btn.gameObject);});

        SelectionUI.Instance.Init(
            new List<SelectionUI.UI>(){
                new SelectionUI.UI(MetaManager.TAG.BUILDING, uiObjs[0], uiObjs[1]),
                new SelectionUI.UI(MetaManager.TAG.ACTOR, uiObjs[2], uiObjs[3]),
                new SelectionUI.UI(MetaManager.TAG.MOB, uiObjs[2], null)
            }
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
            case "resource1":
                obj.GetComponentInChildren<Text>().text = string.Format("{0} {1}", 
                    MetaManager.Instance.resourceInfo[0], 
                    GameSystem.Instance.gameStatus.GetResource(0, 0));
                break;
            case "resource2":
                obj.GetComponentInChildren<Text>().text = string.Format("{0} {1}", 
                    MetaManager.Instance.resourceInfo[1], 
                    GameSystem.Instance.gameStatus.GetResource(0, 1));
                break;
            case "resource3":
                obj.GetComponentInChildren<Text>().text = string.Format("{0} {1}", 
                    MetaManager.Instance.resourceInfo[2], 
                    GameSystem.Instance.gameStatus.GetResource(0, 2));
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
    void OnClickButton(GameObject obj)
    {
        //Debug.Log(string.Format("OnClick {0}, {1}", obj.name, Context.Instance.mode));
        string name = Util.GetObjectName(obj);
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
                if(SelectionUI.Instance.selectedObject != null)
                {
                    int mapId = SelectionUI.Instance.GetSelectedMapId();
                    if(BuildingManager.Instance.objects[mapId].actions.Count == 0)
                        Updater.Instance.AddQ(ActionType.BUILDING_DESTROY, mapId, -1, null, true);
                    else
                        Debug.Log("The Building has actions");
                }
                SelectionUI.Instance.Hide();
                break;
            case "buttonR":
                if(SelectionUI.Instance.selectedObject != null)
                {
                    Vector3 angles = SelectionUI.Instance.selectedObject.transform.localEulerAngles;
                    SelectionUI.Instance.selectedObject .transform.localEulerAngles = new Vector3(angles.x, angles.y + 90, angles.z);
                }
                SelectionUI.Instance.Hide();
                break;
            case "buttonB":
                {
                    int mapId = SelectionUI.Instance.GetSelectedMapId();
                    if(mapId != -1)
                    {
                        int buildingId = BuildingManager.Instance.objects[mapId].id;
                        OnClickForCreatingActor(mapId, buildingId);
                    }
                    SelectionUI.Instance.Hide();
                }
                break;
            
            default:
                break;
        }
    }
    void OnClick_ACTOR(GameObject obj, string name)
    {
        switch(name)
        {
            case "buttonU":
                if(SelectionUI.Instance.selectedObject != null)
                {
                    int mapId = SelectionUI.Instance.GetSelectedMapId();
                    int actorId = ActorManager.Instance.actors[mapId].id;
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

            Updater.Instance.AddQ(ActionType.ACTOR_CREATE, mapId, id, null, false);
            Context.Instance.SetMode(Context.Mode.NONE);
            HideLayers();
        }
    }
    
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
            new QNode(type, mapId, 0, null, false),
            new QNode(type, mapId, 255, null, false)
        });
        ((ContextActor)Context.Instance.contexts[Context.Mode.ACTOR]).Clear();
        Context.Instance.SetMode(Context.Mode.NONE);
    }

    // 모든 선택 이벤트 통합.
    void OnSelected(MetaManager.TAG tag, int mapId, int id)
    {
        //Debug.Log(string.Format("OnSelected {0} {1} {2}", tag, mapId, id));
        GameObject gameObject = null;
        string[] sz = new string[1];
        switch(tag)
        {
            case MetaManager.TAG.BUILDING:
                gameObject = BuildingManager.Instance.objects[mapId].gameObject;
                sz[0] = MetaManager.Instance.buildingInfo[id].name;
                break;
            case MetaManager.TAG.ACTOR:
                gameObject = ActorManager.Instance.actors[mapId].gameObject;
                sz[0] = MetaManager.Instance.actorInfo[id].name;
                break;
            case MetaManager.TAG.MOB:
                gameObject = MobManager.Instance.mobs[mapId].gameObject;
                sz[0] = MetaManager.Instance.mobInfo[id].name;
                break;
        }
        SelectionUI.Instance.Activate(tag, gameObject, sz);
    }
    //모든 행동 이벤트
    void OnAction(int mapId, int id, MetaManager.TAG tag, int targetMapId)
    {
        switch(tag)
        {
            case MetaManager.TAG.BUILDING:
                GameStatus.Building building = GameSystem.Instance.gameStatus.buildingInfo[targetMapId];
                Debug.Log(string.Format("tribe {0}, {1}", building.tribeId, building.buildingId));
                break;
        }
    }
    //생성, 소멸등의 이벤트
    void OnCreationEvent(ActionType type, MetaManager.TAG tag, int mapId, int id)
    {
        Debug.Log(string.Format("OnCreationEvent {0}, {1}, {2}, {3}", type, tag, mapId, id));
        //game system과 연결 시켜 줘야함
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
