using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MapMaker : MonoBehaviour
{
    public Transform canvas;
    private GameObject buildingLayer, actorLayer, environmentLayer, neutralLayer, mobLayer;
    private GameObject actor_scrollview;

    int myTribeId = 0; 

    // Start is called before the first frame update
    void Start()
    {
        InitManager.Instance.Initialize();
        InitManager.Instance.Instantiate();

        LoaderPerspective.Instance.SetUI(Camera.main, ref canvas, OnClickButton);
        if(!LoaderPerspective.Instance.LoadJsonFile("ui_map_maker"))
        {
            Debug.LogError("ui.json loading failure");
            return;
        } 

        LoaderPerspective.Instance.AddComponents(null, OnCreatePost);
        buildingLayer = GameObject.Find("buildings");
        actorLayer = GameObject.Find("actors");
        environmentLayer = GameObject.Find("environments");
        neutralLayer = GameObject.Find("neutrals");
        mobLayer = GameObject.Find("mobs");

        HideLayers();
        InitSelectionUI();

        Context.CallbackFunctions functions = new Context.CallbackFunctions(
            OnCreationEvent,
            OnCreationFinish,
            OnSelected,
            OnActorAction,
            OnAttack,
            OnLoadResource,
            OnDelivery,
            CheckDefenseAttack,
            OnEarning,
            OnDie
        );
    
        //Context
        Context.Instance.Init(  functions,
                                ref canvas, 
                                "progress_default", 
                                "CubeGreen", 
                                "CubeRed"
                                );
        //update 중지
        Updater.Instance.onlyBasicUpdate = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            //------------------------------
            switch(Context.Instance.mode)
            {
                case Context.Mode.UI_ACTOR:
                case Context.Mode.UI_BUILD:
                case Context.Mode.UI_ENVIRONMENT:
                case Context.Mode.UI_NEUTRAL:
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
    void InitSelectionUI()
    {
        //UI
        string[] arr = new string[2] {"text_title", "select_ui_building"};
        GameObject[] uiObjs = new GameObject[2];
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
        /*
        //for actor
        Button btn = uiObjs[2].GetComponentInChildren<Button>();
        btn.onClick.AddListener(()=>{ OnClickButton(btn.gameObject);});

        //for neutral
        Button btnNeutral = uiObjs[3].GetComponentInChildren<Button>();
        btnNeutral.onClick.AddListener(()=>{ OnClickButton(btnNeutral.gameObject);});
        */

        SelectionUI.Instance.Init(
            new List<SelectionUI.UI>(){
                new SelectionUI.UI(TAG.BUILDING, uiObjs[0], uiObjs[1]),
                new SelectionUI.UI(TAG.ACTOR, uiObjs[0], uiObjs[1]),
                new SelectionUI.UI(TAG.MOB, uiObjs[0], uiObjs[1]),
                new SelectionUI.UI(TAG.NEUTRAL, uiObjs[0], uiObjs[1]),
                new SelectionUI.UI(TAG.ENVIRONMENT, uiObjs[0], uiObjs[1]),
            }
        );
    }

    void HideLayers()
    {
        buildingLayer.SetActive(false);
        actorLayer.SetActive(false);
        environmentLayer.SetActive(false);
        neutralLayer.SetActive(false);
        mobLayer.SetActive(false);

        GameObject.DestroyImmediate(actor_scrollview);
    }

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
            case "btn_save":
                Save();
                return;
            case "btn_load_scene":
                SceneManager.LoadScene("LoadMap");
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
            case Context.Mode.UI_ENVIRONMENT:
                OnClick_UI_ENVIRONMENT(obj, name);
                break;
            case Context.Mode.UI_NEUTRAL:
                OnClick_UI_NEUTRAL(obj, name);
                break;
            case Context.Mode.UI_MOB:
                OnClick_UI_MOB(obj, name);
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
            case "btn_environment":
                environmentLayer.SetActive(true);
                Context.Instance.SetMode(Context.Mode.UI_ENVIRONMENT);
                break;
            case "btn_neutral":
                neutralLayer.SetActive(true);
                Context.Instance.SetMode(Context.Mode.UI_NEUTRAL);
                break;
            case "btn_mob":
                mobLayer.SetActive(true);
                Context.Instance.SetMode(Context.Mode.UI_MOB);
                break;
            case "buttonX":
                if(SelectionUI.Instance.selectedObject != null)
                {
                    int id = SelectionUI.Instance.GetSelectedObjectId();
                    //environment 찾기
                    if(EnvironmentManager.Instance.environments.ContainsKey(id))
                    {
                        EnvironmentManager.Instance.DestroyEnvironment(id);
                    } 
                    else
                    {
                        Object p = ObjectManager.Instance.Get(id);
                        switch(p.tag)
                        {
                            case TAG.MOB:
                                ((Mob)p).Destroy();
                            break;
                            case TAG.ACTOR:
                                ((Actor)p).Destroy();
                            break;
                            case TAG.BUILDING:
                                Updater.Instance.AddQ(new QNode(ActionType.BUILDING_DESTROY, p.seq));
                            break;
                            case TAG.NEUTRAL:
                                ((NeutralBuilding)p).Destroy();
                            break;
                        }
                    }
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
                    int seq = SelectionUI.Instance.GetSelectedObjectId();
                    if(seq != -1)
                    {
                        OnClickForCreatingActor((BuildingObject)ObjectManager.Instance.Get(seq));
                    }
                    SelectionUI.Instance.Hide();
                }
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
            ((ContextBuild)Context.Instance.contexts[Context.Mode.BUILD]).SetBuildingId(myTribeId, id, true);
            HideLayers();
        }
    }

    void OnClick_UI_ENVIRONMENT(GameObject obj, string name)
    {
        string[] arr = name.Split('-');
        if(arr.Length >= 2 && arr[0] == "environment")
        {
            int id = int.Parse(arr[1]);
            Context.Instance.SetMode(Context.Mode.ENVIRONMENT);
            ((ContextEnvironment)Context.Instance.contexts[Context.Mode.ENVIRONMENT]).SetEnvironmentId(id);
            HideLayers();
        }
    }

    void OnClick_UI_NEUTRAL(GameObject obj, string name)
    {
        string[] arr = name.Split('-');
        if(arr.Length >= 2 && arr[0] == "neutral")
        {
            int id = int.Parse(arr[1]);
            Context.Instance.SetMode(Context.Mode.NEUTRAL);
            ((ContextNeutral)Context.Instance.contexts[Context.Mode.NEUTRAL]).SetNeutralId(id);
            HideLayers();
        }
    }
    void OnClick_UI_MOB(GameObject obj, string name)
    {
        string[] arr = name.Split('-');
        if(arr.Length >= 2 && arr[0] == "mob")
        {
            int id = int.Parse(arr[1]);
            Context.Instance.SetMode(Context.Mode.MOB);
            ((ContextMob)Context.Instance.contexts[Context.Mode.MOB]).SetMobId(id);
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
            //Updater.Instance.AddQ(ActionType.ACTOR_CREATE, -1, mapId, id, null, true);
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
            case "scrollview_environment":
                List<GameObject> environmentScrollItems = GetEnvironmentScrollItems();
                LoaderPerspective.Instance.CreateScrollViewItems(environmentScrollItems
                                                                , new Vector2(15, 15)
                                                                , new Vector2(10, 10)
                                                                , OnClickButton
                                                                , obj
                                                                , environmentScrollItems.Count);
                break;
            case "scrollview_neutral":
                List<GameObject> neutralScrollItems = GetNeutralScrollItems();
                LoaderPerspective.Instance.CreateScrollViewItems(neutralScrollItems
                                                                , new Vector2(15, 15)
                                                                , new Vector2(10, 10)
                                                                , OnClickButton
                                                                , obj
                                                                , neutralScrollItems.Count);
                break;
            case "scrollview_mob":
                List<GameObject> mobScrollItems = GetMobScrollItems();
                LoaderPerspective.Instance.CreateScrollViewItems(mobScrollItems
                                                                , new Vector2(15, 15)
                                                                , new Vector2(10, 10)
                                                                , OnClickButton
                                                                , obj
                                                                , mobScrollItems.Count);
                break;
            case "scrollview_actor":
                actor_scrollview = obj;
                break;
            case "resource0":
                obj.GetComponent<InputField>().text = GameStatusManager.Instance.resourceInfo[0][0].ToString();
                break;
            case "resource1":
                obj.GetComponent<InputField>().text = GameStatusManager.Instance.resourceInfo[0][1].ToString();
                break;
            case "resource2":
                obj.GetComponent<InputField>().text = GameStatusManager.Instance.resourceInfo[0][2].ToString();
                break;
            case "filepath":
                obj.GetComponent<InputField>().text = InitManager.Instance.savedFilePath;
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
    List<GameObject> GetEnvironmentScrollItems()
    {
        List<GameObject> list = new List<GameObject>();
        for(int n = 0; n < MetaManager.Instance.meta.environments.Count; n++)
        {
            GameObject obj = Resources.Load<GameObject>("button_default");
            Meta.Environment prefab = MetaManager.Instance.meta.environments[n];
            obj.GetComponentInChildren<Text>().text = prefab.name;
            obj.name = string.Format("environment-{0}", prefab.id);
            list.Add(Instantiate(obj));
        }

        return list;
    }

    List<GameObject> GetNeutralScrollItems()
    {
        List<GameObject> list = new List<GameObject>();
        for(int n = 0; n < MetaManager.Instance.meta.neutrals.Count; n++)
        {
            GameObject obj = Resources.Load<GameObject>("button_default");
            Meta.Neutral prefab = MetaManager.Instance.meta.neutrals[n];
            obj.GetComponentInChildren<Text>().text = prefab.name;
            obj.name = string.Format("neutral-{0}", prefab.id);
            list.Add(Instantiate(obj));
        }

        return list;
    }

    List<GameObject> GetMobScrollItems()
    {
        List<GameObject> list = new List<GameObject>();
        for(int n = 0; n < MetaManager.Instance.meta.mobs.Count; n++)
        {
            GameObject obj = Resources.Load<GameObject>("button_default");
            Meta.Mob prefab = MetaManager.Instance.meta.mobs[n];
            obj.GetComponentInChildren<Text>().text = prefab.name;
            obj.name = string.Format("mob-{0}", prefab.id);
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
    void Save()
    {
        GameStatus save = new GameStatus();

        GameStatus.ResourceIdAmount r0 = new GameStatus.ResourceIdAmount();
        r0.resourceId = 0;
        r0.amount = int.Parse(GameObject.Find("resource0").GetComponent<InputField>().text);

        GameStatus.ResourceIdAmount r1 = new GameStatus.ResourceIdAmount();
        r1.resourceId = 1;
        r1.amount = int.Parse(GameObject.Find("resource1").GetComponent<InputField>().text);

        GameStatus.ResourceIdAmount r2 = new GameStatus.ResourceIdAmount();
        r2.resourceId = 2;
        r2.amount = int.Parse(GameObject.Find("resource2").GetComponent<InputField>().text);
        
        SortedDictionary<int, List<BuildingObject>> temp = new SortedDictionary<int, List<BuildingObject>>();
        /*
        foreach(KeyValuePair<int, BuildingObject> kv in BuildingManager.Instance.objects)
        {
            int tribeId = kv.Value.tribeId;
            if(!temp.ContainsKey(tribeId))
            {
                temp[tribeId] = new List<BuildingObject>();
            }

            temp[tribeId].Add(kv.Value);
        }
        */
        List<int> list = ObjectManager.Instance.GetObjectSeqs(TAG.BUILDING);
        for(int n = 0; n < list.Count; n++)
        {
            int seq = list[n];
            Object obj = ObjectManager.Instance.Get(seq);

            int tribeId = obj.tribeId;
            if(!temp.ContainsKey(tribeId))
            {
                temp[tribeId] = new List<BuildingObject>();
            }

            temp[tribeId].Add((BuildingObject)obj);
        }

        //tribes
        save.tribes = new List<GameStatus.Tribe>();

        foreach(KeyValuePair<int, List<BuildingObject>> kv in temp)
        {
            GameStatus.Tribe tribe = new GameStatus.Tribe();
            //resource
            tribe.resources = new List<GameStatus.ResourceIdAmount>(new GameStatus.ResourceIdAmount[3] {r0, r1, r2});
            //building
            tribe.buildings = new List<GameStatus.Building>();
            for(int n = 0; n < kv.Value.Count; n++)
            {
                BuildingObject obj = kv.Value[n];

                GameStatus.Building building = new GameStatus.Building();
                building.mapId = obj.mapId;
                building.buildingId = obj.id;
                building.rotation = obj.gameObject.transform.rotation.eulerAngles.y;

                //actor
                building.actors = new List<GameStatus.MapIdActorIdHP>();
                for(int i = 0; i < kv.Value[n].actors.Count; i++)
                {
                    Actor actor = kv.Value[n].actors[i];
                    GameStatus.MapIdActorIdHP p = new GameStatus.MapIdActorIdHP();
                    p.mapId = actor.mapId;
                    p.actorId = actor.id;
                    p.HP = actor.currentHP;
                    p.rotation = actor.gameObject.transform.rotation.eulerAngles.y;
                    building.actors.Add(p);
                }

                tribe.buildings.Add(building);
            }

            save.tribes.Add(tribe);
        }
        
        //mob
        save.mobs = new List<GameStatus.Mob>();
        List<int> mobs = ObjectManager.Instance.GetObjectSeqs(TAG.MOB);
        for(int n = 0; n < mobs.Count; n++)
        {
            int seq = mobs[n];
            Mob obj = (Mob)ObjectManager.Instance.Get(seq);
            GameStatus.Mob mob = new GameStatus.Mob();
            mob.mapId = obj.attachedId;
            mob.mobId = obj.id;
            mob.amount = 1;
            save.mobs.Add(mob);
        }
        /*
        foreach(KeyValuePair<int, Mob> kv in MobManager.Instance.mobs)
        {
            GameStatus.Mob mob = new GameStatus.Mob();
            mob.mapId = kv.Value.attachedId;
            mob.mobId = kv.Value.id;
            mob.amount = 1;
            save.mobs.Add(mob);
        }
        */

        //environment
        save.environments = new List<GameStatus.Environment>();
        foreach(KeyValuePair<int, EnvironmentManager.Environment> kv in EnvironmentManager.Instance.environments)
        {
            GameStatus.Environment env = new GameStatus.Environment();
            env.mapId = kv.Key;
            env.environmentId = kv.Value.id;
            env.rotation = kv.Value.gameObject.transform.rotation.eulerAngles.y;

            save.environments.Add(env);
        }

        //neutral
        save.neutrals = new List<GameStatus.Neutral>();
        List<int> neutrals = ObjectManager.Instance.GetObjectSeqs(TAG.NEUTRAL);
        for(int n = 0; n < neutrals.Count; n++)
        {
            int seq = neutrals[n];
            Object p = ObjectManager.Instance.Get(seq);

            GameStatus.Neutral neutral = new GameStatus.Neutral();
            neutral.mapId = p.mapId;
            neutral.neutralId = p.id;
            neutral.rotation = p.gameObject.transform.rotation.eulerAngles.y;

            save.neutrals.Add(neutral);
        }
        /*
        foreach(KeyValuePair<int, NeutralBuilding> kv in NeutralManager.Instance.objects)
        {
            GameStatus.Neutral neutral = new GameStatus.Neutral();
            neutral.mapId = kv.Key;
            neutral.neutralId = kv.Value.id;
            neutral.rotation = kv.Value.gameObject.transform.rotation.eulerAngles.y;

            save.neutrals.Add(neutral);
        }
        */
        string path = GameObject.Find("filepath").GetComponent<InputField>().text;
        path = Json.SaveJsonFile(path, save);

        Debug.Log(string.Format("Save {0}", path));
    }

    //-----------------------------------------------
    public bool OnCreationEvent(QNode q)
    {
        return true;
    }
    public void OnCreationFinish(ActionType type, Object obj)
    {
    }
    public void OnDie(ActionType type, Object obj, Object from)
    {    
    }
    public void SetDelivery(Actor actor, int targetMapId, TAG targetBuildingTag)
    {
    }
    public void OnSelected(TAG tag, int mapId, int id, GameObject gameObject)
    {
        if(tag == TAG.BOTTOM)
            return;

        //Debug.Log(string.Format("OnSelected {0} {1} {2}", tag, mapId, id));
        int seq = Util.GetIntFromGameObjectName(gameObject.name);
        Object obj = ObjectManager.Instance.Get(seq);
        SelectionUI.Instance.Activate(tag, gameObject, new string[1] { 
            string.Format("{0} {1} {2}", Util.GetNameInGame(tag, id),  obj.tribeId, mapId.ToString() )
            });
    }
    //Actor 모든 행동 이벤트
    public void OnActorAction(Actor actor, GameObject target)
    {
    }
    public void OnAttack(Object from, Object to, int amount)
    {
    }
    
    public void OnLoadResource(Actor actor)
    {
    }
    public void OnDelivery(Actor actor, Object target)
    {
    }
    public bool CheckDefenseAttack(Object target, Object from)
    {
        return true;
    }

    public void OnEarning(Object obj, bool success)
    {
    }
}
