using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class GamePlay : MonoBehaviour
{
    Callbacks callbacks = new Callbacks();
    GameObject cmObj;
    float cmRatio = 0;
    //Vector3 cmDefault;
    //-------------------
    public Transform canvas;
    private GameObject buildingLayer, actorLayer;

    //
    int myTribeId = 0; 

    //a*
    List<int> route = new List<int>();
    //float time = 0;
    GameObject actor_scrollview;
    // Start is called before the first frame update
    void Start()
    {
        LoaderPerspective.Instance.SetUI(Camera.main, ref canvas, OnClickButton);
        if(!LoaderPerspective.Instance.LoadJsonFile("ui"))
        {
            Debug.LogError("ui.json loading failure");
            return;
        } 

        LoaderPerspective.Instance.AddComponents(OnCreate, OnCreatePost);
        buildingLayer = GameObject.Find("buildings");
        actorLayer = GameObject.Find("actors");

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
                LoaderPerspective.Instance.CreateScrollViewItems(GetBuildingScrollItems(), 15, OnClickButton, obj);
                break;
            case "scrollview_actor":
                actor_scrollview = obj;
                break;
            /*
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
            */
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
                break;
            case "zoomout":
                if(Camera.main.fieldOfView < 25)
                    Camera.main.fieldOfView += 5;
                break;
            case "tribe0":
                myTribeId = 0;
                break;
            case "tribe1":
                myTribeId = 1;
                break;
            case "tribe2":
                myTribeId = 2;
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
                    int mapId = SelectionUI.Instance.GetSelectedMapId();
                    Object building = BuildingManager.Instance.objects[mapId];
                    
                    if(BuildingManager.Instance.objects[mapId].actions.Count == 0)
                        Updater.Instance.AddQ(ActionType.BUILDING_DESTROY, building.tribeId, mapId, -1, null, true);
                    else
                        Debug.Log("The Building has actions");
                }
                SelectionUI.Instance.Hide();
                break;
            case "buttonR":
                if(SelectionUI.Instance.selectedObject != null)
                {
                    Vector3 angles = SelectionUI.Instance.selectedObject.transform.localEulerAngles;
                    SelectionUI.Instance.selectedObject.transform.localEulerAngles = new Vector3(angles.x, angles.y + 90, angles.z);
                }
                SelectionUI.Instance.Hide();
                break;
            case "buttonB":
                {
                    int mapId = SelectionUI.Instance.GetSelectedMapId();
                    if(mapId != -1)
                    {
                        OnClickForCreatingActor(BuildingManager.Instance.objects[mapId]);
                    }
                    SelectionUI.Instance.Hide();
                }
                break;
            case "buttonI":
                {
                    TAG tag = MetaManager.Instance.GetTag(SelectionUI.Instance.selectedObject.tag);
                    int mapId = SelectionUI.Instance.GetSelectedMapId();
                    int id = Util.GetIdInGame(tag, mapId);
                    Debug.Log(string.Format("buttonI {0} {1} {2}", tag, mapId, id));
                    SelectionUI.Instance.Hide();
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
            ((ContextBuild)Context.Instance.contexts[Context.Mode.BUILD]).SetBuildingId(myTribeId, id);
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
            //building정보에서 tribe정보를 actor에 반영하기 때문에 tribeId는 몰라도 된다. 
            Updater.Instance.AddQ(ActionType.ACTOR_CREATE, -1, mapId, id, null, false);
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
        LoaderPerspective.Instance.CreateScrollViewItems(GetActorScrollItems(building.id, building.level), 15, OnClickButton, actor_scrollview);
                
        actorLayer.SetActive(true);
        Context.Instance.SetMode(Context.Mode.UI_ACTOR);
        ((ContextCreatingActor)Context.Instance.contexts[Context.Mode.UI_ACTOR]).SetSelectedBuilding(building.mapId, building.id);
    }
    void OnClickForUpgradingActor(int mapId, int actorId)
    {
        
        ((ContextActor)Context.Instance.contexts[Context.Mode.ACTOR]).Clear();
        Context.Instance.SetMode(Context.Mode.NONE);
        //강화
        GachaManager.Instance.SetGachaTarget(ActorManager.Instance.actors[mapId], TAG.ACTOR);
        SceneManager.LoadScene("LevelUp");
        /*
        //camera moving
        cmObj = ActorManager.Instance.actors[mapId].gameObject;
        cmRatio = 0;
        cmDefault = Camera.main.transform.position;
        */
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

        if(cmObj != null)
        {
            cmRatio += Time.deltaTime;
            if(cmRatio < 1 )
            {
                float s = Mathf.Sin( 90 * cmRatio * Mathf.PI / 180 );
                //Debug.Log(string.Format("sin{0} {1}", cmRatio, s));
                //Camera.main.transform.position = Vector3.Lerp(cmDefault, cmObj.transform.position, s);
                Camera.main.fieldOfView = 10 - (5 * cmRatio);
                Camera.main.transform.RotateAround(cmObj.transform.position, cmObj.transform.rotation.eulerAngles, 10*s);

            }
                
        }
    }
    
    GameObject OnCreate(string layerName,string name, string tag, Vector2 position, Vector2 size)
    {
        return null;
    } 
}
