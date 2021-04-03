using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*

    여기사 시작점이라 모든 event에 필요한 context를 여기서 호출해버리면 된당

*/
public class UserAction : MonoBehaviour
{
    Dictionary<Context.Mode, IContext> contexts;
    
    void Awake()
    {
        MetaManager.Instance.Load();
        MapManager.Instance.Load();
        //assign
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
        
        Vector3 pos = MapManager.Instance.GetVector3FromMapId(MapManager.Instance.GetMapId(new Vector2Int(MapManager.Instance.mapMeta.dimension.x / 2, MapManager.Instance.mapMeta.dimension.y / 2)));
        //Camera.main.transform.position = new Vector3(pos.x, 8, -5);
        Context.Instance.isInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        //event별 context 호출
        if(Input.GetMouseButtonDown(0))
        {
            contexts[Context.Instance.mode].OnTouch();
        }
        else if(Input.GetMouseButtonUp(0))
        {
            contexts[Context.Instance.mode].OnTouchRelease();
        }
        contexts[Context.Instance.mode].OnMove();
    }
}
