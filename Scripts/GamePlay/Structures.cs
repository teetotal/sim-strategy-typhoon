using System.Collections.Generic;
using UnityEngine;

public struct UnderAttack
{
    public Object from;
    public int amount;

    public UnderAttack(Object from, int amount)
    {
        this.from = from;
        this.amount = amount;
    }
}

//각종 행위. 공격, 이동
public struct Action
{
    public ActionType type;
    public float totalTime;       //Action이 적용되는 총 시간
    public float currentTime;     //현재까지 진행된 시간
    public List<int> values;      //기타 추가 정보. 이동시 A* route같은거 담는 용도

    public Action(ActionType type, float totalTime = 0, List<int> values = null)
    {
        this.type = type;
        this.currentTime = 0;
        this.totalTime = totalTime;
        this.values = values;
    }

    public float GetProgression()
    {
        if(this.totalTime == 0)
            return 0;
        if(this.currentTime > this.totalTime)
            return this.values[this.values.Count - 1];

        return (this.currentTime / this.totalTime) * this.values.Count;
    }
}
/* --------------------------- */
//중립 지역 건물 정보
public class NeutralBuilding: Object
{
    public override bool AddAction(QNode node)
    {
        return true;
    }
    public override bool Create(int mapId, int id)
    {
        this.mapId = mapId;
        this.id = id;

        //progress
        Vector3 pos = GetProgressPosition();
        progress = GameObject.Instantiate(Context.Instance.progressPrefab, pos, Quaternion.identity);
        progress.name = string.Format("progress-{0}-{1}", mapId, this.id);
        progress.transform.SetParent(Context.Instance.canvas);
        progress.SetActive(false);

        return true;
    }

    public override void Update()
    {
    }
    public override void UpdateUnderAttack()
    {
    }
}
//건물 정보
public class BuildingObject : Object
{   
    public List<Actor> actors = new List<Actor>();
    public override bool AddAction(QNode node)
    {
        Meta.Building meta =  MetaManager.Instance.buildingInfo[this.id];
        Action action = new Action(ActionType.MAX);

        switch(node.type)
        {
            case ActionType.BUILDING_UNDER_ATTACK:
                Object obj = Util.GetObject(node.id, (TAG)node.values[0]);
                //일부러 null 체크 안함
                underAttackQ.Enqueue(new UnderAttack(obj, node.values[1]));
                return true;
            case ActionType.BUILDING_DESTROY:
                action = new Action(node.type, 2);
                break;
        }
        actions.Add(action);
        return true;
    }
    
    public override bool Create(int mapId, int id)
    {
        this.mapId = mapId;
        this.id = id;
        this.currentMapId = mapId;
        this.currentHP = MetaManager.Instance.buildingInfo[id].level[this.level].HP;
        this.level = 0;

        //progress
        Vector3 pos = GetProgressPosition();
        progress = GameObject.Instantiate(Context.Instance.progressPrefab, GetProgressPosition(), Quaternion.identity);
        progress.name = string.Format("progress-{0}-{1}", mapId, this.id);
        progress.transform.SetParent(Context.Instance.canvas);

        return true;
    }

    public override void Update()
    {
        List<int> removeActionIds = new List<int>();
        if(actions.Count > 0)
        {
            Action action = actions[0];
            //progress
            if(progress != null)
                progress.transform.position = GetProgressPosition(); //position

            action.currentTime += Time.deltaTime;
            actions[0] = action;

            switch(action.type)
            {
                case ActionType.BUILDING_CREATE:
                    ShowProgress(action.currentTime, action.totalTime, true);
                    break;
                case ActionType.BUILDING_DESTROY:
                    if(action.currentTime >= action.totalTime)
                    {
                        //딸린 actor들 삭제
                        for(int n = 0; n < this.actors.Count; n++)
                        {
                            Actor actor = actors[n];
                            Updater.Instance.AddQ(ActionType.ACTOR_DIE, actor.mapId, actor.id, null, false, 0);
                        }
                        actions.Clear();
                        DestroyProgress();
                        BuildingManager.Instance.objects.Remove(mapId);
                        MapManager.Instance.DestroyBuilding(mapId);
                        return;
                    }
                    break;
            }

            //finish
            if(action.currentTime >= action.totalTime)
            {
                //removeActionIds.Add(n);
                switch(action.type)
                {
                    case ActionType.BUILDING_CREATE:
                        progress.SetActive(false);
                        break;
                }
                actions.RemoveAt(0);
            }
        }
    }
    public override void UpdateUnderAttack()
    {
        Meta.Building meta = MetaManager.Instance.buildingInfo[this.id];
        while(underAttackQ.Count > 0)
        {
            UnderAttack p = underAttackQ.Dequeue();
            
            this.currentHP -= p.amount;

            //callback
            Context.Instance.onAttack(p.from, this, p.amount);

            ShowHP(meta.level[this.level].HP);
            if(this.currentHP <= 0)
            {
                HideProgress();
                underAttackQ.Clear();
                this.actions.Clear();
                Updater.Instance.AddQ(ActionType.BUILDING_DESTROY, this.mapId, this.id, null, false);
                return;
            }
        }
    }
}

/* --------------------------- */
public struct QNode
{
    public ActionType type;
    public int mapId; //mapid
    public int id;
    public bool immediately;
    public List<int> values;    //caller쪽과 protocol을 맞춰야 한다.
    public int insertIndex;

    public QNode(ActionType type, int mapId, int id, List<int> values, bool immediately, int insertIndex)
    {
        this.type = type;
        this.mapId = mapId;
        this.id = id;
        this.values = values;
        this.immediately = immediately;
        this.insertIndex = insertIndex;
    }
}
/* --------------------------- */
//시대 정보
public struct TimeNode
{
    public string name; //시대 이름
    public int maxTime; //종료 시점
}