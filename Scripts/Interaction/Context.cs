using System;
using System.Collections.Generic;
using UnityEngine;

public class Context 
{
    public enum Mode
    {
        NONE,
        UI_BUILD,
        BUILD,
        UI_ACTOR,
        ACTOR,
        MAX
    }

    public struct CallbackFunctions
    {
        public OnCreationEvent onCreationEvent;
        public OnCreationFinish onCreationFinish;
        public OnSelectionEvent onSelectEvent;
        public OnActorAction onAction;
        public OnAttack onAttack;
        public OnLoadResource onLoadResource;
        public OnDelivery onDelivery;
        public CheckDefenseAttack checkDefenseAttack;
        public OnEarning onEarning;

        public CallbackFunctions(OnCreationEvent onCreationEvent,
                                    OnCreationFinish onCreationFinish,
                                    OnSelectionEvent onSelectEvent,
                                    OnActorAction onAction,
                                    OnAttack onAttack,
                                    OnLoadResource onLoadResource,
                                    OnDelivery onDelivery, 
                                    CheckDefenseAttack checkDefenseAttack,
                                    OnEarning onEarning)
        {
            this.onCreationEvent = onCreationEvent;
            this.onCreationFinish = onCreationFinish;
            this.onSelectEvent = onSelectEvent;
            this.onAction = onAction;
            this.onAttack = onAttack;
            this.onLoadResource = onLoadResource;
            this.onDelivery = onDelivery;
            this.checkDefenseAttack = checkDefenseAttack;
            this.onEarning = onEarning;
        }
    }
    public delegate void OnSelectionEvent(TAG tag, int mapId, int id, GameObject gameObject);
    public OnSelectionEvent onSelectEvent;
    public delegate void OnActorAction(Actor actor, TAG targetTag, int targetMapId);
    public OnActorAction onAction;
    public delegate bool OnCreationEvent(QNode q);
    public OnCreationEvent onCreationEvent;
    public delegate void OnAttack(Object from, Object to, int amount);
    public OnAttack onAttack;
    public delegate void OnLoadResource(Actor actor, int targetBuildingMapId);
    public OnLoadResource onLoadResource;
    public delegate void OnDelivery(Actor actor, int targetBuildingMapId, TAG targetBuildingTag);
    public OnDelivery onDelivery;
    public delegate bool CheckDefenseAttack(Object target, Object from);
    public CheckDefenseAttack checkDefenseAttack;
    public delegate void OnCreationFinish(ActionType type, Object obj);
    public OnCreationFinish onCreationFinish;
    public delegate void OnEarning(Object obj, bool success);
    public OnEarning onEarning;

    //----------------------------------------------------------------------------------------
    public Dictionary<Context.Mode, IContext> contexts;
    private static readonly Lazy<Context> hInstance = new Lazy<Context>(() => new Context());
    public Mode mode = Mode.NONE;
    public bool isInitialized = false;
    public Transform canvas;
    public GameObject greenPrefab, redPrefab, progressPrefab;
    public static Context Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected Context()
    {
        contexts = new Dictionary<Context.Mode, IContext>()
        {
            { Mode.NONE,        new ContextNone()   },
            { Mode.UI_BUILD,    new ContextDummy()  },
            { Mode.BUILD,       new ContextBuild()  },
            { Mode.UI_ACTOR,    new ContextCreatingActor()  },
            { Mode.ACTOR,       new ContextActor()  }
        };
    }
    public void Update()
    {
        //event별 context 호출
        if(Input.GetMouseButtonDown(0))
        {
            contexts[mode].OnTouch();
        }
        else if(Input.GetMouseButtonUp(0))
        {
            contexts[mode].OnTouchRelease();
        }
        else if(Input.GetMouseButton(0))
        {
            contexts[mode].OnDrag();
        }
        
        contexts[mode].OnMove();
    }
    public void Init(
                    CallbackFunctions functions,
                    ref Transform canvas,
                    string progressPrefab, 
                    string greenCube, 
                    string redCube
                    )
    {
        this.onCreationEvent = functions.onCreationEvent;
        this.onCreationFinish = functions.onCreationFinish;
        this.onSelectEvent = functions.onSelectEvent;
        this.onAction = functions.onAction;
        this.onAttack = functions.onAttack;
        this.onLoadResource = functions.onLoadResource;
        this.onDelivery = functions.onDelivery;
        this.checkDefenseAttack = functions.checkDefenseAttack;
        this.onEarning = functions.onEarning;

        this.canvas = canvas;
        this.progressPrefab = Resources.Load<GameObject>(progressPrefab);
        this.greenPrefab = Resources.Load<GameObject>(greenCube);
        this.redPrefab = Resources.Load<GameObject>(redCube);

        //init
        foreach (KeyValuePair<Context.Mode, IContext> kv in contexts)
        {
            kv.Value.Init();
        }

    }
    public void SetMode(Mode _mode)
    {
        mode = _mode;
        contexts[mode].Reset();
    }
}


public class ContextDummy : IContext
{
    public void Init()
    {
    }
    public void Reset()
    {
    }
    public void OnMove()
    {
    }
    public void OnTouch()
    {
    }
    public void OnTouchRelease()
    {
    }
    public void OnDrag()
    {
    }
}