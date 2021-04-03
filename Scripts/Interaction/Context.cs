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

        //init
        foreach (KeyValuePair<Context.Mode, IContext> kv in contexts)
        {
            kv.Value.Init();
        }
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
        contexts[mode].OnMove();
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
}