using UnityEngine;
using UnityEngine.UI;
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
    public GameObject progress; 
    //fn
    public abstract bool Create(int mapId, int id);
    public abstract void Update();
    
    protected GameObject Instantiate(int mapId, int id, string prefab, TAG tag, bool flying)
    {
        Vector3 position = MapManager.Instance.GetVector3FromMapId(mapId);
        GameObject obj = Resources.Load<GameObject>(prefab);

        Meta.Actor actor = MetaManager.Instance.actorInfo[id];

        obj = GameObject.Instantiate(obj, Util.AdjustY(position, flying), Quaternion.identity);
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
    protected void SetProgress(float v, float max, bool displayRemainTime)
    {
        if(progress)
        {
            float value = v / max;
            progress.GetComponent<Slider>().value = value;

            Text txt = progress.GetComponentInChildren<Text>();
            if(txt != null)
            {
                if(displayRemainTime)
                {
                    float t = max - v;
                    int hour = (int)(t / (60 * 60));
                    int min = (int)(t / 60);
                    int sec = (int)(t % 60);
                    
                    if(hour == 0)
                        txt.text = string.Format("{0:D2}:{1:D2}", min, sec);
                    else
                        txt.text = string.Format("{0}:{1:D2}:{2:D2}", hour, min, sec);
                }
                else
                {
                    txt.text = "";
                }
            }
        }
        
    }
    protected Vector3 GetProgressPosition()
    {
        Vector3 pos = gameObject.transform.position;
        return Camera.main.WorldToScreenPoint(new Vector3(pos.x, pos.y + 1, pos.z));
    }
    /*
    private Vector3 GetColliderSize()
    {
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        if(boxCollider)
        {
            return boxCollider.size;
        }
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        if(meshCollider)
        {
            return meshCollider.bounds.max;
        }
        return Vector3.zero;
    }
    */
    public void UpdateUIPosition()
    {
        if(progress != null) //progress
        {
            progress.transform.position = GetProgressPosition(); //position
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
    public List<QNode> routine;
    protected bool isMovingStarted;
    private int routineIdx = 0;
    public void SetRoutine(List<QNode> routine)
    {
        this.routine = routine;
        routineIdx = 0;
    }
    protected void ApplyRoutine()
    {
        if(actions.Count > 0 || routine == null)
            return;
        if(AddAction(routine[routineIdx]))
        {
            routineIdx++;
            if(routineIdx >= routine.Count)
                routineIdx = 0;
        }
        
    }
    public abstract bool AddAction(QNode node);
    
    protected Action GetFlyingAction(int targetMapId, Meta.Ability ability, ActionType type)
    {
        int start = this.mapId;
        //중도 변경을 처리하기 위해 현재 위치의 mapid를 찾아낸다.
        RaycastHit hit;
        Physics.Raycast(this.gameObject.transform.position, Vector3.down, out hit);
        if (hit.collider != null) 
        {
            start = Util.GetIntFromGameObjectName(hit.collider.gameObject.name);
            //Debug.Log("Current MapId: " + start.ToString());
        }
        
        List<int> route = new List<int>();
        route.Add(start);
        route.Add(targetMapId);

        Vector2Int from = MapManager.Instance.GetMapPosition(start);
        Vector2Int to = MapManager.Instance.GetMapPosition(targetMapId);

        Vector2Int diff = from - to;
        return new Action(type, (Mathf.Abs(diff.x) + Mathf.Abs(diff.y)) / ability.moving, route);
    }
    protected Action GetMovingAction(int targetMapId, Meta.Ability ability, ActionType type)
    {
        //Astar
        List<int> route = new List<int>();
        Astar astar = new Astar(MapManager.Instance.map);
        Vector2Int from = MapManager.Instance.GetMapPosition(GetCurrentPositionMapId());
        Vector2Int to = MapManager.Instance.GetMapPosition(targetMapId);
        Stack<Astar.Pos> stack = astar.Search(new Astar.Pos(from.x, from.y), new Astar.Pos(to.x, to.y));
        if(stack == null)
        {
            Action a = new Action();
            a.type = ActionType.MAX;
            return a;
        }

        while(stack.Count > 0)
        {
            int id = MapManager.Instance.GetMapId(new Vector2Int(stack.Peek().x, stack.Peek().y));
            route.Add(id);
            stack.Pop();
        }

        return new Action(type, route.Count / ability.moving, route);
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

        float progression = (action.currentTime / action.totalTime) * route.Count; 
        int idx = (int)progression; 
        float ratio = progression % 1.0f;
        /*
        int idx = (int)action.currentTime;
        float ratio = action.currentTime % 1.0f;
        */
        bool flying = false;

        if(!isMovingStarted && ratio > 0.5f) 
        {
            SetAnimation(ActionType.ACTOR_MOVING);
            isMovingStarted = true;
        }

        if(idx >= route.Count -1 && ratio > 0.5f)
        {
            SetAnimation(ActionType.MAX);
            Vector3 end = Util.AdjustY(MapManager.Instance.GetVector3FromMapId(route[route.Count-1]), flying);
            actor.transform.position = end;
        }

        if(idx == 0)
            return true;

        if(idx >= route.Count)
        {
            return false;
        }
        
        Vector3 pos = Util.AdjustY(MapManager.Instance.GetVector3FromMapId(route[idx - 1]), flying);
        Vector3 posNext = Util.AdjustY(MapManager.Instance.GetVector3FromMapId(route[idx + 0]), flying);

        actor.transform.position = Vector3.Lerp(pos, posNext, ratio);

        Vector3 target = posNext;
        
        Vector3 dir = target - actor.transform.position;
        actor.transform.rotation = Quaternion.Lerp(actor.transform.rotation, Quaternion.LookRotation(dir), ratio);
        
        return true;
    }
    protected bool Flying(Action action, float height)
    {
        List<int> route = action.values;
        GameObject actor = this.gameObject;

        Vector3 from = Util.GetFlyingPosition(MapManager.Instance.GetVector3FromMapId(route[0]), height);
        Vector3 to = Util.GetFlyingPosition(MapManager.Instance.GetVector3FromMapId(route[1]), height);

        float ratio = action.currentTime / action.totalTime;

        actor.transform.position = Vector3.Lerp(from, to, ratio);

        float distance = Vector3.Distance(actor.transform.position, to);
        if(distance < 0.01f)
        {
            actor.transform.position = to;
            return false;
        }

        Vector3 dir = to - actor.transform.position;
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