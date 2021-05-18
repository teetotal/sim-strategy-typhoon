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
public abstract class IBuilding: MonoBehaviour
{
    public abstract void Earning(bool success);
}
public abstract class IBuildingDefensing : IBuilding
{
    public abstract void Attack(List<Vector3> targets, float ratio);
    public abstract void AttackEnd();
    public abstract void Rotation(List<Transform> rots);
}

public abstract class IActor : MonoBehaviour
{
    public abstract void SetIdle();
    public abstract void SetMoving();
    public abstract void SetDie();
    public abstract void Earning(bool success);
}
public abstract class IActorAttacking : IActor
{
    public abstract void SetAttack();
}

//기본 struct. TAG추가. 장착 아이템 추가. 순간 발동 아이템 추가.
public abstract class Object
{
    //추가
    public struct ForceItem // 발동형 아이템
    {
        public float elapse;
        public float totalTime;
        public Meta.Ability forceItem; 
    }
    public int seq;
    public Meta.Ability[] installationItems = new Meta.Ability[(int)ItemInstallationType.MAX]; //장착 아이템
    public ForceItem forceItem;
    public TAG tag;
    //-----

    public GameObject gameObject;
    public float rotation;
    public int tribeId = -1;
    public int mapId;
    public int currentMapId = -1;
    public int id;
    public int level;
    public float currentHP;
    public float earningElapse = 0;
    public Queue<UnderAttack> underAttackQ = new Queue<UnderAttack>();
    public List<Action> actions = new List<Action>(); //현재 겪고 있는 액션 리스트.
    public GameObject progress; 
    
    //fn
    public abstract bool AddAction(QNode node);
    public abstract bool Create(int tribeId, int mapId, int id, bool isInstantiate, float rotation);
    public abstract void Instantiate();
    public abstract void Update();
    public abstract void UpdateDefence();
    public abstract void UpdateUnderAttack();
    public abstract void UpdateEarning();

    protected void Init(int tribeId, int id, int mapId, TAG tag, float HP, int level, float rotation)
    {
        this.tribeId = tribeId;
        this.id = id;
        this.mapId = mapId;
        this.tag = tag;
        this.currentHP = HP;
        this.level = level;
        this.rotation = rotation;

        ObjectManager.Instance.Add(this);
    }
    
    protected GameObject Instantiate(string prefab, bool flying)
    {
        Vector3 position = MapManager.Instance.GetVector3FromMapId(mapId);
        GameObject obj = GameObjectPooling.Instance.Get(prefab, Util.AdjustY(position, flying), Quaternion.Euler(0, rotation, 0));

        obj.tag = MetaManager.Instance.GetTag(tag);
        obj.name = this.seq.ToString(); //mapId.ToString();
        //GameObject parent = MapManager.Instance.defaultGameObjects[mapId];
        //obj.transform.SetParent(parent.transform);

        this.gameObject = obj;

        return gameObject;
    }
    protected void Release()
    {
        //remove map manager
        MapManager.Instance.Remove(this.mapId, this.tag);

        //prefab release
        string prefab = "";
        this.gameObject.transform.parent = null;
        
        switch(this.tag)
        {
            case TAG.ACTOR:
            prefab = MetaManager.Instance.actorInfo[this.id].level[this.level].prefab;
            break;
            case TAG.BUILDING:
            prefab = MetaManager.Instance.buildingInfo[this.id].level[this.level].prefab;
            break;
            case TAG.MOB:
            prefab = MetaManager.Instance.mobInfo[this.id].prefab;
            break;
        }
        GameObjectPooling.Instance.Release(prefab, this.gameObject);

        //remove Object pooling
        ObjectManager.Instance.Remove(this.seq);
         
    }
    public void ShowHP(int totalHP)
    {
        if(progress == null)
            CreateProgress();

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
    public bool Levelup()
    {
        float HP = 0;
        switch(MetaManager.Instance.GetTag(this.gameObject.tag))
        {
            case TAG.ACTOR:
            {
                if(this.level + 1 >= MetaManager.Instance.actorInfo[this.id].level.Count)
                {
                    return false;
                }

                HP = MetaManager.Instance.actorInfo[this.id].level[this.level +1].ability.HP;
                break;
            }
            case TAG.BUILDING:
            {
                if(this.level + 1 >= MetaManager.Instance.buildingInfo[this.id].level.Count)
                {
                    return false;
                }
                HP = MetaManager.Instance.buildingInfo[this.id].level[this.level+1].HP;
                break;
            }
            default:
                break;
        }
        this.level++;
        this.currentHP = HP;

        return true;
    }
    public bool IsCreating()
    {
        if(actions.Count > 0)
        {
            switch(actions[0].type)
            {
                case ActionType.BUILDING_CREATE:
                case ActionType.ACTOR_CREATE:
                    return true;
                default:
                    break;
            }
        } 
        return false;
    }
    protected void CreateProgress()
    {
        //progress
        Vector3 pos = GetProgressPosition();
        progress = GameObject.Instantiate(Context.Instance.progressPrefab, pos, Quaternion.identity);
        progress.name = string.Format("progress-{0}-{1}", mapId, this.id);
        progress.transform.SetParent(Context.Instance.canvas);
        progress.SetActive(false);
    }
    protected void HideProgress()
    {
        if(progress != null)
            progress.SetActive(false);
    }
    protected void ShowProgress(float v, float max, bool displayRemainTime)
    {
        if(progress == null)
            CreateProgress();

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
    /*
    public void DestroyProgress()
    {
        if(progress != null)
            GameObject.DestroyImmediate(progress);
    }
    */
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
                    //여기 손봐야함. !!!!
                    float progression = action.GetProgression(); 
                    int idx = (int)progression;
                    float ratio = progression % 1.0f;
                    //if(ratio < 0.5f && idx > 0) idx --;
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
    public bool SetFollowObject(Object obj)
    {
        followObject = obj;
        if(followObject == null)
            return false;
        return true;
    }
    protected void ApplyRoutine()
    {
        if(actions.Count > 0 || routine == null || routine.Count == 0)
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
        Astar astar = new Astar(MapManager.Instance.map, MapManager.Instance.mapMeta.defaultVal.AstarNodeCost);
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

        Action action = new Action(type, count / ability.moving, route);
        action.SetMovingRoute(this.gameObject.transform.position);

        return action;
    }
    protected bool Moving(Action action)
    {
        List<int> route = action.values;

        GameObject actor = this.gameObject;

        Vector3 now = Vector3.zero;
        Vector3 next = Vector3.zero;
        float r = 0;

        bool ret = action.GetMovingProgression(ref now, ref next, ref r);

        if(!isMovingStarted) 
        {
            SetAnimation(ActionType.ACTOR_MOVING); //mob도 걍 ACTOR_MOVING로 쓸까?
            isMovingStarted = true;
        }

        actor.transform.rotation = Quaternion.Lerp(actor.transform.rotation, Quaternion.LookRotation(next - actor.transform.position), r);
        actor.transform.position = Vector3.Lerp(now, next, r);

        if(!ret)
        {
            SetAnimation(ActionType.MAX);
        }
        
        return ret;
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
    protected void Attack(Meta.Ability ability, bool isFlying)
    {
        Action action = this.actions[0];
        ActionType attackAction = ActionType.MAX;
        ActionType movingAction = ActionType.MAX;

        switch(tag)
        {
            case TAG.ACTOR:
                attackAction = ActionType.ACTOR_ATTACK;
                movingAction = isFlying ? ActionType.ACTOR_FLYING : ActionType.ACTOR_MOVING_1_STEP;
            break;
            case TAG.MOB:
                attackAction = ActionType.MOB_ATTACK;
                movingAction = isFlying ? ActionType.MOB_FLYING : ActionType.MOB_MOVING;
            break;
        }
        //공격 거리
        if(CheckAttacking(ability))
        {
            ShowHP(ability.HP);
            //attack
            //죽었는지 확인
            if(followObject.currentHP > 0 && Attacking())
            {
                //아래 finish코드에 추가 안하고 이렇게 한 이유가 다 있음
                if(action.currentTime >= action.totalTime)
                {
                    //상대방 공격 당함
                    ActionType at = ActionType.MAX;
                    if(followObject.tag == TAG.ACTOR)
                        at = ActionType.ACTOR_UNDER_ATTACK;
                    else if(followObject.tag == TAG.BUILDING)
                        at = ActionType.BUILDING_UNDER_ATTACK;
                    else if(followObject.tag == TAG.MOB)
                        at = ActionType.MOB_UNDER_ATTACK;

                    //임시로 꼬일까봐 둘다 셋팅
                    QNode q = new QNode(at,
                        followObject.tribeId,
                        followObject.mapId, 
                        this.mapId,
                        new List<int>() { (int)tag, ability.attack },
                        true, -1);

                    q.type = at;
                    q.requestInfo.mySeq = followObject.seq;
                    q.requestInfo.fromObject = this;
                    q.requestInfo.amount = ability.attack;
                    
                    Updater.Instance.AddQ(q);


                    action.currentTime = 0;
                    actions[0] = action;
                }
            }
            else
            {
                Clear(true, true, false);
                SetAnimation(ActionType.MAX);
                this.HideProgress();
            }
            return;
        } 
        else 
        {
            //SetAnimation(ActionType.ACTOR_MAX);
            this.Clear(true, false, false);
            /*
            List<QNode> list = new List<QNode>()
            {
                //공격하기
                new QNode(attackAction,
                    this.tribeId,
                    this.mapId, 
                    -1,
                    null,
                    false,
                    -1
                    ),
                //따라가기. 이동을 먼저 넣으면 mapid정보가 바뀌니까 뒤에 넣고 actions 순서를 바꾼다.
                new QNode(movingAction,
                    this.tribeId,
                    this.mapId, 
                    MapManager.Instance.GetRandomNearEmptyMapId(followObject.GetCurrentMapId(), (int)ability.attackDistance),
                    null,
                    false,
                    0
                    ),
            };
            */
            List<QNode> list = new List<QNode>();
            // 1. 따라가기
            {
                QNode q = new QNode(movingAction, this.seq);
                q.requestInfo.targetMapId = MapManager.Instance.GetRandomNearEmptyMapId(followObject.GetCurrentMapId(), (int)ability.attackDistance);
                list.Add(q);
            }
            // 2. 공격하기
            {
                QNode q = new QNode(attackAction, this.seq);
                list.Add(q);
            }
            
            Updater.Instance.AddQs(list);
        }
        return;
    }
    protected bool Attacking()
    {
        if(followObject == null)
            return false;

        this.gameObject.transform.LookAt(followObject.gameObject.transform.position);
        SetAnimation(ActionType.ACTOR_ATTACK); //mob, actor 모두 공통으로 처리
        return true;
    }
    public void SetAnimation(ActionType type)
    {
        //set animation
        IActor p = gameObject.GetComponent<IActor>();
        if(p != null)
        {
            switch(type)
            {
                case ActionType.MOB_MOVING:
                case ActionType.ACTOR_MOVING:
                    p.SetMoving();
                    break;
                case ActionType.ACTOR_ATTACK:
                    {
                        IActorAttacking a = gameObject.GetComponent<IActorAttacking>();
                        if(a != null)
                            a.SetAttack();
                    }
                    break;    
                case ActionType.MOB_DIE:
                case ActionType.ACTOR_DIE:
                    p.SetDie();
                    break;
                default:
                    p.SetIdle();
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