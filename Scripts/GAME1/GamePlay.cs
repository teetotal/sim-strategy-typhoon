using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GamePlay : MonoBehaviour
{
    enum POPUPID
    {
        INVENTORY,
        TRADING
    }
    Callbacks callbacks = new Callbacks();
    //-------------------
    public Transform canvas;

    Dictionary<POPUPID, IUIInterface> popupUI;
    private GameObject buildingLayer, actorLayer, inventoryLayer, tradingLayer;

    //
    int myTribeId = 0; 

    //a*
    List<int> route = new List<int>();
    GameObject actor_scrollview;
    UITrading uiTrading;
    UIInventory uiInventory;
    // Start is called before the first frame update
    void Start()
    {
        InitManager.Instance.Instantiate();
        
        LoaderPerspective.Instance.SetUI(Camera.main, ref canvas, OnClickButton);
        if(!LoaderPerspective.Instance.LoadJsonFile("ui"))
        {
            Debug.LogError("ui.json loading failure");
            return;
        } 

        LoaderPerspective.Instance.AddComponents(null, OnCreatePost);
        buildingLayer = GameObject.Find("buildings");
        actorLayer = GameObject.Find("actors");

        popupUI = new Dictionary<POPUPID, IUIInterface>()
        {
            { POPUPID.INVENTORY, GameObject.Find("inventory").GetComponent<IUIInterface>() },
            { POPUPID.TRADING, GameObject.Find("trading").GetComponent<IUIInterface>() }
        };
        InitPopup();
        
        HideLayers();
        
        callbacks.Init();
        InitSelectionUI();
        callbacks.UpdateResourceUI();

        Context.CallbackFunctions functions = new Context.CallbackFunctions(
            callbacks.OnCreationEvent,
            callbacks.OnCreationFinish,
            callbacks.OnSelected,
            callbacks.OnActorAction,
            callbacks.OnAttack,
            callbacks.OnLoadResource,
            callbacks.OnDelivery,
            callbacks.CheckDefenseAttack,
            callbacks.OnEarning,
            callbacks.OnDie
        );

    
        //Context
        Context.Instance.Init(  functions,
                                ref canvas, 
                                "progress_default", 
                                "CubeGreen", 
                                "CubeRed"
                                );

        //camera
        float h = 42;
        float angle = 32;
        
        int mapId = MapManager.Instance.GetMapId(MapManager.Instance.mapMeta.dimension / 2);
        Vector3 pos = MapManager.Instance.GetVector3FromMapId(mapId);
        pos.y = h;
        
        float radian = (90 - angle) * Mathf.Deg2Rad;
        float t = Mathf.Tan(radian);
        pos.z = pos.z - (h * t);
        Camera.main.transform.position = pos;
        Camera.main.transform.rotation = Quaternion.Euler(angle, 0 , 0);
    }
    void InitSelectionUI()
    {
        //UI
        string[] arr = new string[4] {"text_title", "select_ui_building", "select_ui_actor", "select_ui"};
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
        Button btn = uiObjs[2].GetComponentInChildren<Button>();
        btn.onClick.AddListener(()=>{ OnClickButton(btn.gameObject);});

        //for neutral
        Button btnNeutral = uiObjs[3].GetComponentInChildren<Button>();
        btnNeutral.onClick.AddListener(()=>{ OnClickButton(btnNeutral.gameObject);});


        SelectionUI.Instance.Init(
            new List<SelectionUI.UI>(){
                new SelectionUI.UI(TAG.BUILDING, uiObjs[0], uiObjs[1]),
                new SelectionUI.UI(TAG.ACTOR, uiObjs[0], uiObjs[2]),
                new SelectionUI.UI(TAG.MOB, uiObjs[0], uiObjs[3]),
                new SelectionUI.UI(TAG.NEUTRAL, uiObjs[0], uiObjs[3])
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
                List<GameObject> buildingScrollItems = GetBuildingScrollItems();
                LoaderPerspective.Instance.CreateScrollViewItems(buildingScrollItems
                                                                , new Vector2(15, 15)
                                                                , new Vector2(10, 10)
                                                                , OnClickButton
                                                                , obj
                                                                , buildingScrollItems.Count);
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

    List<GameObject> GetActorScrollItems(int buildingId, int level)
    {
        List<GameObject> list = new List<GameObject>();
        for(int n = 0; n < MetaManager.Instance.meta.buildings[buildingId].level[level].actors.Count; n++)
        {
            GameObject obj = Resources.Load<GameObject>("button_default");
            Meta.ActorIdMax actorIdMax = MetaManager.Instance.meta.buildings[buildingId].level[level].actors[n];
            Meta.Actor info = MetaManager.Instance.meta.actors[actorIdMax.actorId];

            string wage = "";
            for(int m = 0; m < info.level[0].wage.Count; m++)
            {
                Meta.ResourceIdAmount r = info.level[0].wage[m];
                wage += string.Format("\n{0} {1}", MetaManager.Instance.resourceInfo[r.resourceId], r.amount);
            }
            obj.GetComponentInChildren<Text>().text = info.name + "\n" + wage;
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
        switch(name)
        {
            case "zoomin":
                if(Camera.main.fieldOfView > 5)
                    Camera.main.fieldOfView -= 5;
                return;
            case "zoomout":
                if(Camera.main.fieldOfView < 25)
                    Camera.main.fieldOfView += 5;
                return;
            case "tribe0":
                myTribeId = 0;
                return;
            case "tribe1":
                myTribeId = 1;
                return;
            case "tribe2":
                myTribeId = 2;
                return;
            case "btn_inventory":
                ShowPopup(POPUPID.INVENTORY);
                return;
            case "btn_lobby":
                SceneManager.LoadScene("Lobby");
                return;
            default:
                break;
        }

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
            case "buttonX":
                if(SelectionUI.Instance.selectedObject != null)
                {
                    int seq = SelectionUI.Instance.GetSelectedObjectId();
                    BuildingObject building = (BuildingObject)ObjectManager.Instance.Get(seq);//BuildingManager.Instance.objects[mapId];
                    
                    if(building.actions.Count == 0)
                    {
                        QNode q = new QNode(ActionType.BUILDING_DESTROY, building.seq);
                        q.immediately = true;
                        Updater.Instance.AddQ(q);
                    }
                    else
                        Debug.Log("The Building has actions");
                }
                SelectionUI.Instance.Hide();
                break;
            case "buttonR":
                if(SelectionUI.Instance.selectedObject != null)
                {
                    Vector3 angles = SelectionUI.Instance.selectedObject.transform.localEulerAngles;
                    SelectionUI.Instance.selectedObject.transform.localEulerAngles = new Vector3(angles.x, angles.y + 45, angles.z);
                }
                SelectionUI.Instance.Hide();
                break;
            case "buttonB":
                {
                    int seq = SelectionUI.Instance.GetSelectedObjectId();
                    if(seq != -1)
                    {
                        OnClickForCreatingActor((BuildingObject)ObjectManager.Instance.Get(seq));
                    }
                    SelectionUI.Instance.Hide();
                }
                break;
            case "buttonI":
                {
                    TAG tag = MetaManager.Instance.GetTag(SelectionUI.Instance.selectedObject.tag);
                    int seq = SelectionUI.Instance.GetSelectedObjectId();
                   
                    Debug.Log(string.Format("buttonI {0} {1}", tag, seq));
                    SelectionUI.Instance.Hide();
                    if(tag == TAG.NEUTRAL)
                    {
                        ShowPopup(POPUPID.TRADING);
                    }
                    break;
                }
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
                    int seq = SelectionUI.Instance.GetSelectedObjectId();
                    OnClickForUpgradingActor(ObjectManager.Instance.Get(seq));
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
            ((ContextBuild)Context.Instance.contexts[Context.Mode.BUILD]).SetBuildingId(myTribeId, id, false);
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
            BuildingObject building = ((ContextCreatingActor)Context.Instance.contexts[Context.Mode.UI_ACTOR]).selectedBuilding;
            //building정보에서 tribe정보를 actor에 반영하기 때문에 tribeId는 몰라도 된다. 
            //Updater.Instance.AddQ(ActionType.ACTOR_CREATE, -1, mapId, id, null, false);
            QNode q = new QNode();
            q.type = ActionType.ACTOR_CREATE;
            q.id = id;
            q.requestInfo.fromObject = building;
            Updater.Instance.AddQ(q);

            Context.Instance.SetMode(Context.Mode.NONE);
            HideLayers();
        }
    }
    
    void OnClickForCreatingActor(BuildingObject building)
    {
        if(actorLayer.activeSelf == true)
            return;

        actor_scrollview = LoaderPerspective.Instance.CreateByPrefab("scrollview_default", 
                                                                        actorLayer.transform, 
                                                                        actorLayer.GetComponent<RectTransform>().sizeDelta,
                                                                        actorLayer.transform.position
                                                                        );
        List<GameObject> list = GetActorScrollItems(building.id, building.level);
        LoaderPerspective.Instance.CreateScrollViewItems(list
                                                        , new Vector2(15, 15)
                                                        , new Vector2(10, 10)
                                                        , OnClickButton
                                                        , actor_scrollview
                                                        , list.Count
                                                        );
                
        actorLayer.SetActive(true);
        Context.Instance.SetMode(Context.Mode.UI_ACTOR);
        ((ContextCreatingActor)Context.Instance.contexts[Context.Mode.UI_ACTOR]).SetSelectedBuilding(building);
    }
    void OnClickForUpgradingActor(Object actor)
    {
        //저장
        GameStatusManager.Instance.Save(GameStatusManager.Instance.savedFilePath);

        ((ContextActor)Context.Instance.contexts[Context.Mode.ACTOR]).Clear();
        Context.Instance.SetMode(Context.Mode.NONE);
        //강화
        GachaManager.Instance.SetGachaTarget(actor, actor.tag);
        SceneManager.LoadScene("LevelUp");
    }
    

    // UI canceling
    void Update()
    {
        callbacks.CheckMessageAvailable(Time.deltaTime);
        
        if(Input.GetMouseButtonUp(0))
        {
            //------------------------------
            switch(Context.Instance.mode)
            {
                case Context.Mode.UI_ACTOR:
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
    //-------------------------------------------------
    private void InitPopup()
    {
        foreach(KeyValuePair<POPUPID, IUIInterface> kv in popupUI)
        {
            kv.Value.Init();
            kv.Value.Close();
        }
    }
    private void ShowPopup(POPUPID key)
    {
        Context.Instance.SetMode(Context.Mode.UI_POPUP);
        HidePopup();
        popupUI[key].Show();
        popupUI[key].UpdateUI();
    }

    private void HidePopup()
    {
        foreach(KeyValuePair<POPUPID, IUIInterface> kv in popupUI)
        {
            kv.Value.Close();
        }
    }
}
