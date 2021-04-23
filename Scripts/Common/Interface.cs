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
public abstract class IBuildingAttack : MonoBehaviour
{
    public abstract void Attack(List<Vector3> targets, float ratio);
    public abstract void AttackEnd();
    public abstract void Rotation(List<Transform> rots);
}

public abstract class IAnimation : MonoBehaviour
{
    public abstract void SetIdle();
    public abstract void SetMoving();
    public abstract void SetDie();
}
public abstract class IAttacking : IAnimation
{
    public abstract void SetAttack();
}

//기본 struct
public abstract class Object
{
    public GameObject gameObject;
    public int tribeId = -1;
    public int mapId;
    public int currentMapId = -1;
    public int id;
    public int level;
    public float currentHP;
    public Queue<UnderAttack> underAttackQ = new Queue<UnderAttack>();
    public List<Action> actions = new List<Action>(); //현재 겪고 있는 액션 리스트.
    public GameObject progress; 
    
    //fn
    public abstract bool AddAction(QNode node);
    public abstract bool Create(int tribeId, int mapId, int id);
    public abstract void Update();
    public abstract void UpdateDefence();
    public abstract void UpdateUnderAttack();
    
    protected GameObject Instantiate(int tribeId, int mapId, int id, string prefab, TAG tag, bool flying)
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

        this.tribeId = tribeId;
        this.id = id;
        this.mapId = mapId;
        this.gameObject = obj;

        return gameObject;
    }
    public void ShowHP(int totalHP)
    {
        progress.SetActive(true);
        progress.transform.position = GetProgressPosition();

        float value = (float)this.currentHP / (float)totalHP;
        progress.GetComponent<Slider>().value = value;
        Text txt = progress.GetComponentInChildren<Text>();
        if(txt != null)
        {
            txt.text = "";
        }

    }
    protected void HideProgress()
    {
        if(progress != null)
            progress.SetActive(false);
    }
    protected void ShowProgress(float v, float max, bool displayRemainTime)
    {
        progress.SetActive(true);
        progress.transform.position = GetProgressPosition();

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
    protected Vector3 GetProgressPosition()
    {
        Vector3 pos = gameObject.transform.position;
        return Camera.main.WorldToScreenPoint(new Vector3(pos.x, pos.y + 1, pos.z));
    }
    public void DestroyProgress()
    {
        if(progress != null)
            GameObject.DestroyImmediate(progress);
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
    public void RemoveActionType(ActionType type)
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
    public bool HasActionType(ActionType type)
    {
        for(int n = 0; n < actions.Count; n++)
        {
            if(actions[n].type == type)
            {
                return true;
            }
        }
        return false;
    }
    private bool CheckMovableObject(TAG tag)
    {
        switch(tag)
        {
            case TAG.ACTOR:
            case TAG.MOB:
                return true;
            default:
                return false;
        }

    }
    protected void SetCurrentMapId()
    {
        int beforeMapId = this.GetCurrentMapId();
        RaycastHit hit;

        if(actions.Count > 0)
        {
            Action action = actions[0];
            switch(action.type)
            {
                case ActionType.ACTOR_FLYING:
                case ActionType.MOB_FLYING:
                {
                    Physics.Raycast(this.gameObject.transform.position, Vector3.down, out hit);
                    if (hit.collider != null && !CheckMovableObject(MetaManager.Instance.GetTag(hit.collider.gameObject.tag)))
                    {
                        this.currentMapId = Util.GetIntFromGameObjectName(hit.collider.gameObject.name);
                    }
                    break;
                }
                case ActionType.ACTOR_MOVING:
                case ActionType.MOB_MOVING:
                {
                    float progression = action.GetProgression(); 
                    int idx = (int)progression;
                    float ratio = progression % 1.0f;
                    if(ratio < 0.5f && idx > 0)
                        idx --;
                    this.currentMapId = action.values[idx];
                    break;
                }
                default:
                    this.currentMapId = this.mapId;
                    break;
            }
        }
        else
            this.currentMapId = this.mapId;

        if(beforeMapId != this.currentMapId)
            MapManager.Instance.MoveCurrentMap(beforeMapId, this, MetaManager.Instance.GetTag(this.gameObject.tag));
    }
    public int GetCurrentMapId()
    {
        if(this.currentMapId == -1)
        {
            Debug.Log("Current MapId is -1");
            return this.mapId;
        }
            
        return this.currentMapId;
    }
}

public abstract class ActingObject : Object
{
    public List<QNode> routine;
    public Object followObject; // 쫒을 actingobject
    protected bool isMovingStarted;
    private int routineIdx = 0;
    public void SetRoutine(List<QNode> routine)
    {
        this.routine = routine;
        routineIdx = 0;
    }
    public bool SetFollowObject(int mapId, TAG tag)
    {
        followObject = Util.GetObject(mapId, tag);
        if(followObject == null)
            return false;
        return true;
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
    protected bool CheckAttacking(Meta.Ability ability)
    {
        if(this.followObject == null)
            return false;
        float distance = MapManager.Instance.GetDistance(GetCurrentMapId(), this.followObject.GetCurrentMapId());
        if(ability.attackDistance >= distance)
            return true;
        return false;
    }
    
    protected Action GetFlyingAction(int targetMapId, Meta.Ability ability, ActionType type)
    {
        int start = GetCurrentMapId();
        List<int> route = new List<int>();
        route.Add(start);
        route.Add(targetMapId);

        Vector2Int from = MapManager.Instance.GetMapPosition(start);
        Vector2Int to = MapManager.Instance.GetMapPosition(targetMapId);

        Vector2Int diff = from - to;
        return new Action(type, (Mathf.Abs(diff.x) + Mathf.Abs(diff.y)) / ability.moving, route);
    }
    /*
    step: 몇 스텝만 이동할지, -1은 전체 
    */
    protected Action GetMovingAction(int targetMapId, Meta.Ability ability, ActionType type, int step = -1)
    {
        isMovingStarted = false;
        //Astar
        List<int> route = new List<int>();
        Astar astar = new Astar(MapManager.Instance.map);
        Vector2Int from = MapManager.Instance.GetMapPosition(GetCurrentMapId());
        Vector2Int to = MapManager.Instance.GetMapPosition(targetMapId);
        Stack<Astar.Pos> stack = astar.Search(new Astar.Pos(from.x, from.y), new Astar.Pos(to.x, to.y));
        if(stack == null)
        {
            Action a = new Action();
            a.type = ActionType.MAX;
            return a;
        }

        int count = stack.Count;
        if(step > 0 && step <= count)
            count = step;

        for(int n = 0; n < count; n++)
        {
            if(stack.Count > 1)
                route.Add(MapManager.Instance.GetMapId(new Vector2Int(stack.Peek().x, stack.Peek().y)));
            else
                route.Add(targetMapId);
            
            stack.Pop();
        }

        return new Action(type, count / ability.moving, route);
    }
    protected bool Moving(Action action)
    {
        List<int> route = action.values;

        GameObject actor = this.gameObject;

        float progression = action.GetProgression(); 
        int idx = (int)progression; 
        float ratio = progression % 1.0f;
        /*
        int idx = (int)action.currentTime;
        float ratio = action.currentTime % 1.0f;
        */
        bool flying = false;

        if(!isMovingStarted) 
        {
            SetAnimation(ActionType.ACTOR_MOVING);
            isMovingStarted = true;
        }

        if(idx == 0)
            return true;

        if(idx >= route.Count)
        {
            SetAnimation(ActionType.MAX);
            actor.transform.position = Util.AdjustY(MapManager.Instance.GetVector3FromMapId(route[route.Count - 1]), flying);
            return false;
        }
        
        Vector3 pos = Util.AdjustY(MapManager.Instance.GetVector3FromMapId(route[idx - 1]), flying);
        Vector3 posNext = Util.AdjustY(MapManager.Instance.GetVector3FromMapId(route[idx + 0]), flying);
        Vector3 position = Vector3.Lerp(pos, posNext, ratio);
        float distance = Vector3.Distance(actor.transform.position, position);
        if(distance > 3)
        {
            Debug.LogError(string.Format("too much distance. {0} mapid {1} -> {2}", distance, route[idx - 1], route[idx]));
        }
        actor.transform.position = position;

        Vector3 dir = posNext - actor.transform.position;
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
    protected bool Attacking()
    {
        if(followObject == null)
            return false;

        this.gameObject.transform.LookAt(followObject.gameObject.transform.position);
        SetAnimation(ActionType.ACTOR_ATTACK);
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
                case ActionType.ACTOR_MAX:
                case ActionType.MAX:
                    p.SetIdle();
                    break;
                case ActionType.ACTOR_ATTACK:
                    {
                        IAttacking a = gameObject.GetComponent<IAttacking>();
                        if(a != null)
                            a.SetAttack();
                    }
                    break;    
                case ActionType.ACTOR_DIE:
                    p.SetDie();
                    break;
            }
            
        }
    }
    public void Clear(bool clearActions = true, bool clearFollowObject = true, bool clearRoutine = true)
    {
        if(clearActions)
            actions.Clear();
        if(clearFollowObject)
            followObject = null;
        if(clearRoutine && routine != null)
            routine.Clear();
    }
    
}