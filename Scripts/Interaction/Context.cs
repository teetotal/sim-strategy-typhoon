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
        MAX
    }
    public Dictionary<Context.Mode, IContext> contexts;
    private static readonly Lazy<Context> hInstance = new Lazy<Context>(() => new Context());
    public Mode mode = Mode.NONE;
    public bool isInitialized = false;
    public Transform canvas;
    public GameObject greenPrefab, redPrefab, progressPrefab, selectUIPrefab;
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
            { Context.Mode.NONE,        new ContextNone()   },
            { Context.Mode.UI_BUILD,    new ContextDummy()  },
            { Context.Mode.BUILD,       new ContextBuild()  }
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
    public void Init(ref Transform canvas, string progressPrefab, string greenCube, string redCube, string selectUIPrefab)
    {
        this.canvas = canvas;
        this.progressPrefab = Resources.Load<GameObject>(progressPrefab);
        this.greenPrefab = Resources.Load<GameObject>(greenCube);
        this.redPrefab = Resources.Load<GameObject>(redCube);
        this.selectUIPrefab = Resources.Load<GameObject>(selectUIPrefab);

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
    }public void Reset()
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