using UnityEngine;
using System.Collections.Generic;
public interface IContext
{
    void Init();
    void Reset();
    void OnMove();
    void OnTouch();
    void OnTouchRelease();
    void OnDrag();
}

public abstract class IAnimation : MonoBehaviour
{
    public abstract void SetIdle();
    public abstract void SetMoving();
}

//기본 struct
public abstract class Object
{
    public GameObject gameObject;
    public int mapId;     //이동시 업데이트 필요
    public int id;
    public int level;
    public List<Action> actions = new List<Action>(); //현재 겪고 있는 액션 리스트.
    public GameObject progress, ui;
    protected bool isMovingStarted;
    //fn
    public abstract bool Create(int mapId, int id);
    public abstract void Update();
    public void EnableUI(GameObject obj)
    {
        ui = obj;
    }
    public void DisableUI()
    {
        ui = null;
    }
    
    
    protected GameObject Instantiate(int mapId, int id, string prefab, MetaManager.TAG tag)
    {
        Vector3 position = MapManager.Instance.GetVector3FromMapId(mapId);
        GameObject obj = Resources.Load<GameObject>(prefab);
        obj = GameObject.Instantiate(obj, new Vector3(position.x, position.y + 0.1f, position.z), Quaternion.identity);
        obj.tag = MetaManager.Instance.GetTag(tag);
        obj.name = mapId.ToString();
        GameObject parent = MapManager.Instance.defaultGameObjects[mapId];
        obj.transform.SetParent(parent.transform);

        obj.transform.rotation = Quaternion.Euler(0, 180, 0);

        this.id = id;
        this.mapId = mapId;
        this.gameObject = obj;

        return gameObject;
    }
    protected Vector3 GetProgressPosition()
    {
        //객체의 크기와 줌 크기에 맞춰 조절
        return Camera.main.WorldToScreenPoint(gameObject.transform.position + new Vector3(0, 1.0f, 0));
    }
    public void UpdateUIPosition()
    {
        if(ui != null)
        {
            if(ui.activeSelf)
            {
                //title과 버튼의 위치를 객체의 크기에 맞춰서 싱크해 줘야 함
                ui.transform.position = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            }
            else
                ui = null;
        } 
    }
    protected void RemoveActions(List<int> removeActionIds)
    {
        for(int n = 0; n < removeActionIds.Count; n++)
        {   
            actions.RemoveAt(removeActionIds[n]);
        }
    }
    protected void RemoveActionType(ActionType type)
    {
        List<int> removeList = new List<int>();
        for(int n = 0; n < actions.Count; n++)
        {
            if(actions[n].type == type)
            {
                removeList.Add(n);
            }
        }
        RemoveActions(removeList);
    }
}

public abstract class ActingObject : Object
{
    public void SetMoving(int targetMapId)
    {
        if(targetMapId == -1)
            return;

        //mapmanager 변경. 
        MapManager.Instance.Move(mapId, targetMapId);

        Action action = new Action();
        action.type = ActionType.ACTOR_MOVING;
        action.currentTime = 0;

        //Astar
        List<int> route = new List<int>();
        Astar astar = new Astar(MapManager.Instance.map);
        Vector2Int from = MapManager.Instance.GetMapPosition(GetCurrentPositionMapId());
        Vector2Int to = MapManager.Instance.GetMapPosition(targetMapId);
        Stack<Astar.Pos> stack = astar.Search(new Astar.Pos(from.x, from.y), new Astar.Pos(to.x, to.y));
        if(stack == null)
            return;
        
        while(stack.Count > 0)
        {
            int id = MapManager.Instance.GetMapId(new Vector2Int(stack.Peek().x, stack.Peek().y));
            route.Add(id);
            stack.Pop();
        }
        action.totalTime = route.Count;
        action.values = route;

        //이전 이동 액션을 제거
        RemoveActionType(ActionType.ACTOR_MOVING);
        //새로운 액션을 추가
        actions.Add(action);

        //actor map id변경
        this.mapId = targetMapId;
        GameObject parent = MapManager.Instance.defaultGameObjects[targetMapId];
        this.gameObject.name = mapId.ToString();
        this.gameObject.transform.SetParent(parent.transform);

        isMovingStarted = false;
    }
    protected int GetCurrentPositionMapId()
    {
        for(int n = 0; n < actions.Count; n++)
        {
            if(actions[n].type == ActionType.ACTOR_MOVING)
            {
                int idx = (int)actions[n].currentTime;
                return actions[n].values[idx];
            }
        }
        return this.mapId;
    }
    protected bool Moving(Action action)
    {
        List<int> route = action.values;

        GameObject actor = this.gameObject;
        int idx = (int)action.currentTime;
        float ratio = action.currentTime % 1.0f;

        if(!isMovingStarted && ratio > 0.5f) 
        {
            SetAnimation(ActionType.ACTOR_MOVING);
            isMovingStarted = true;
        }

        if(idx >= route.Count -1 && ratio > 0.5f)
        {
            SetAnimation(ActionType.MAX);
        }

        if(idx == 0)
            return true;

        if(idx >= route.Count)
        {
            return false;
        }

        
        Vector3 pos = MapManager.Instance.GetVector3FromMapId(route[idx - 1]) + new Vector3(0, 0.1f, 0);
        Vector3 posNext = MapManager.Instance.GetVector3FromMapId(route[idx + 0]) + new Vector3(0, 0.1f, 0);

        actor.transform.position = Vector3.Lerp(pos, posNext, ratio);

        Vector3 target = posNext;// + new Vector3(0, 0.1f, 0);
        
        Vector3 dir = target - actor.transform.position;
        actor.transform.rotation = Quaternion.Lerp(actor.transform.rotation, Quaternion.LookRotation(dir), ratio);
        
        return true;
    }
    public void SetAnimation(ActionType type)
    {
        //set animation
        IAnimation p = gameObject.GetComponent<IAnimation>();
        if(p != null)
        {
            switch(type)
            {
                case ActionType.ACTOR_MOVING:
                    p.SetMoving();
                    break;
                case ActionType.MAX:
                    p.SetIdle();
                    break;
            }
            
        }
    }
}