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
    public delegate void OnSelectionEvent(TAG tag, int mapId, int id, GameObject gameObject);
    public OnSelectionEvent onSelectEvent;
    public delegate void OnActorAction(Actor actor, TAG targetTag, int targetMapId);
    public OnActorAction onAction;
    public delegate void OnCreationEvent(ActionType type, TAG tag, int mapId, int id);
    public OnCreationEvent onCreationEvent;

    public Dictionary<Context.Mode, IContext> contexts;
    private static readonly Lazy<Context> hInstance = new Lazy<Context>(() => new Context());
    public Mode mode = Mode.NONE;
    public bool isInitialized = false;
    public Transform canvas;
    public GameObject greenPrefab, redPrefab, progressPrefab, titlePrefab;
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
                    OnCreationEvent onCreationEvent,
                    OnSelectionEvent onSelectEvent,
                    OnActorAction onAction,
                    ref Transform canvas, 
                    string progressPrefab, 
                    string titlePrefab, 
                    string greenCube, 
                    string redCube
                    )
    {
        this.onCreationEvent = onCreationEvent;
        this.onSelectEvent = onSelectEvent;
        this.onAction = onAction;

        this.canvas = canvas;
        this.progressPrefab = Resources.Load<GameObject>(progressPrefab);
        this.titlePrefab = Resources.Load<GameObject>(titlePrefab);
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