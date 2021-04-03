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
    }
}


public class ContextDummy : IContext
{
    public void Init()
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