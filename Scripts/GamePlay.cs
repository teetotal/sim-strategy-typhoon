using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class GamePlay : MonoBehaviour
{
    private float attackRatio = 0;
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
        LoadSavedPlay();
        UpdateResourceUI();
    
        //Context
        Context.Instance.Init(  OnCreationEvent,
                                OnSelected,
                                OnActorAction,
                                OnAttack,
                                OnLoadResource,
                                OnDelivery,
                                ref canvas, 
                                "progress_default", 
                                "text_default",
                                "CubeGreen", 
                                "CubeRed"
                                );
    }
    void LoadSavedPlay()
    {
        //Load save game status
        foreach(KeyValuePair<int, GameStatus.Building> kv in GameSystem.Instance.gameStatus.buildingInfo)
        {
            Updater.Instance.AddQ(ActionType.BUILDING_CREATE, kv.Key, kv.Value.buildingId, new List<int>() {  (int)kv.Value.rotation }, true);
            for(int n = 0; n < kv.Value.actors.Count; n++)
            {
                GameStatus.MapIdActorIdHP p = kv.Value.actors[n];
                Updater.Instance.AddQ(ActionType.ACTOR_CREATE, kv.Key, p.actorId, new List<int>() { p.HP}, true);
            }
        }
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
    */
    void UpdateResourceUI()
    {
        for(int n = 0; n < 3; n++)
        {
            GameObject.Find("resource" + (n+1).ToString()).GetComponentInChildren<Text>().text = string.Format("{0} {1}", 
                    MetaManager.Instance.resourceInfo[n], 
                    GameSystem.Instance.gameStatus.GetResource(0, n));
        }
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
    }
    /*
    Context에 등록되는 On~ 함수들
    */
    void SetDelivery(Actor actor, int targetMapId, TAG targetBuildingTag)
    {
        Meta.Actor meta = MetaManager.Instance.actorInfo[actor.id];
        ActionType type = meta.flying ? ActionType.ACTOR_FLYING: ActionType.ACTOR_MOVING;
        //routine 추가
        actor.SetRoutine(new List<QNode>()
        {
            new QNode(type, actor.mapId, actor.attachedBuilding.mapId, null, false, -1), //home으로 가서
            new QNode(ActionType.ACTOR_LOAD_RESOURCE, actor.mapId, actor.attachedBuilding.mapId, null, false, -1), //적재
            new QNode(type, actor.mapId, targetMapId, null, false, -1), // 시장으로 이동
            new QNode(ActionType.ACTOR_DELIVERY, actor.mapId, targetMapId, new List<int>() { (int)targetBuildingTag }, false, -1) //판매
        });
    }

    // 모든 선택 이벤트 통합.
    void OnSelected(TAG tag, int mapId, int id, GameObject gameObject)
    {
        //Debug.Log(string.Format("OnSelected {0} {1} {2}", tag, mapId, id));
        SelectionUI.Instance.Activate(tag, gameObject, new string[1] { Util.GetNameInGame(tag, id) });
    }
    //Actor 모든 행동 이벤트
    void OnActorAction(Actor actor, TAG tag, int targetMapId)
    {
        //Debug.Log(string.Format("OnAction {0}, {1}, {2}, {3}", mapId, id, tag, targetMapId));
        Meta.Actor meta = MetaManager.Instance.actorInfo[actor.id];
        switch(tag)
        {
            case TAG.BUILDING:
                if(actor.SetFollowObject(targetMapId, TAG.BUILDING))
                    Updater.Instance.AddQ(ActionType.ACTOR_ATTACK, actor.mapId, -1, null, false);
                //BuildingObject targetBuilding = BuildingManager.Instance.objects[targetMapId];
                //BuildingObject home = actor.attachedBuilding;

                //Debug.Log(string.Format("{0} -> {1}", home.mapId, targetBuilding.mapId));
                break;
            case TAG.NEUTRAL:
                NeutralBuilding targetBuilding = NeutralManager.Instance.objects[targetMapId];
                Meta.Neutral targetMeta = MetaManager.Instance.neutralInfo[targetBuilding.id]; 
                if(targetMeta.type == (int)BuildingType.MARKET)
                    SetDelivery(actor, targetMapId, TAG.NEUTRAL);
                break;
            case TAG.ACTOR:
                if(actor.mapId != targetMapId)
                {
                    if(actor.SetFollowObject(targetMapId, TAG.ACTOR))
                        Updater.Instance.AddQ(ActionType.ACTOR_ATTACK, actor.mapId, -1, null, false);
                }
                break;
            case TAG.MOB:
                actor.followObject = MobManager.Instance.mobs[targetMapId];
                Updater.Instance.AddQ(ActionType.ACTOR_ATTACK, actor.mapId, -1, null, false);
                break;
            case TAG.BOTTOM:
                if(actor.mapId != targetMapId && MapManager.Instance.IsEmptyMapId(targetMapId))
                {
                    //이전 모든 행위 취소
                    actor.Clear();
                    Updater.Instance.AddQ(
                        meta.flying ? ActionType.ACTOR_FLYING : ActionType.ACTOR_MOVING, 
                        actor.mapId, 
                        targetMapId, 
                        null,
                        false);
                }
                break;
        }
        SelectionUI.Instance.Hide();
    }
    void OnAttack(Object from, Object to, int amount)
    {
        Debug.Log(string.Format("OnAttack {0} -> {1} attack: {2}, HP {3}", from.mapId, to.mapId, amount, to.currentHP));
        if(to.currentHP <= 0)
            Debug.Log("OnAttack Die");
            
    }
    //생성, 소멸등의 이벤트
    void OnCreationEvent(ActionType type, TAG tag, int mapId, int id)
    {
        //Debug.Log(string.Format("OnCreationEvent {0}, {1}, {2}, {3}", type, tag, mapId, id));
        //game system과 연결 시켜 줘야함
    }
    void OnLoadResource(Actor actor, int targetBuildingMapId)
    {
        //짐 싣기
        GameObject selectedObj = actor.gameObject;
        //Debug.Log(string.Format("OnClickForUpgradingActor {0}-{1}", mapId, actorId));
        GameObject o = Resources.Load<GameObject>("load");
        o = GameObject.Instantiate(o);
        o.name = "load";
        o.transform.SetParent(selectedObj.transform);
        Vector3 size = selectedObj.GetComponent<BoxCollider>().size;
        o.transform.localPosition = new Vector3(0, size.y, 0);
    }
    void OnDelivery(Actor actor, int targetBuildingMapId, TAG targetBuildingTag)
    {
        Transform load = actor.gameObject.transform.Find("load");
        if(load == null)
        {
            Debug.LogError("Finding the load failure");
        }
        else
        {
            GameObject.DestroyImmediate(load.gameObject);
        }
        //resource 차감
        Meta.Building metaBuilding = MetaManager.Instance.buildingInfo[actor.attachedBuilding.id];
        Meta.Actor metaActor = MetaManager.Instance.actorInfo[actor.id];

        for(int n = 0; n < metaBuilding.level[actor.attachedBuilding.level].output.Count; n++)
        {
            int resourceId = metaBuilding.level[actor.attachedBuilding.level].output[n].resourceId;
            int amount = metaActor.level[actor.level].ability.carring;
            //tribe
            GameSystem.Instance.gameStatus.resourceInfo[0][resourceId] -= amount;
            GameSystem.Instance.gameStatus.resourceInfo[0][GameSystem.Instance.marketStatus.standardResourceId] += 
                amount * GameSystem.Instance.marketStatus.exchangeInfo[targetBuildingMapId][resourceId];
        }
        UpdateResourceUI();
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
        else if(Input.GetMouseButtonUp(1))
        {
            //GameObject obj = GameObject.Find("Turret");
            
            Vector3 target = MapManager.Instance.GetVector3FromMapId(70);
            
            IBuildingAttack p = GameObject.Find("TurretTower").GetComponent<IBuildingAttack>();
            p.Rotation(Quaternion.LookRotation(target));
            //obj.transform.LookAt(target);
            attackRatio += 0.1f;
            p.Attack(target, attackRatio);

            
            if(attackRatio > 1)
            {
                p.AttackEnd();
                attackRatio = 0;
            }
                
            //obj.transform
            //GameObject p = GameObject.Instantiate(particle, obj.transform.position + new Vector3(0, 0, 1), obj.transform.rotation);
            //p.transform.SetParent(obj.transform);
        }
    }
    
     GameObject OnCreate(string layerName,string name, string tag, Vector2 position, Vector2 size)
    {
        return null;
    } 
}
