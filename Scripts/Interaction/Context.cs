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
    public delegate void OnClickForObject(int mapId, int id); //건물, 액터 선택시 이벤트용
    public OnClickForObject onClickForCreatingActor, onClickForUpgradingActor;
    public Dictionary<Context.Mode, IContext> contexts;
    private static readonly Lazy<Context> hInstance = new Lazy<Context>(() => new Context());
    public Mode mode = Mode.NONE;
    public bool isInitialized = false;
    public Transform canvas;
    public GameObject greenPrefab, redPrefab, progressPrefab, selectUIPrefab, selectUIActorPrefab;
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
    public void Init(ref Transform canvas, 
                    string progressPrefab, 
                    string greenCube, 
                    string redCube, 
                    string selectUIPrefab,
                    string selectUIActorPrefab,
                    OnClickForObject onClickForCreatingActor,
                    OnClickForObject onClickForUpgradingActor
                    )
    {
        this.canvas = canvas;
        this.progressPrefab = Resources.Load<GameObject>(progressPrefab);
        this.greenPrefab = Resources.Load<GameObject>(greenCube);
        this.redPrefab = Resources.Load<GameObject>(redCube);
        this.selectUIPrefab = Resources.Load<GameObject>(selectUIPrefab);
        this.selectUIActorPrefab = Resources.Load<GameObject>(selectUIActorPrefab);
        this.onClickForCreatingActor = onClickForCreatingActor;
        this.onClickForUpgradingActor = onClickForUpgradingActor;

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