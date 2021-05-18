using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mob : ActingObject
{
    public int attachedId;
    float elapseRoutine = 0;
    
    public override bool AddAction(QNode node)
    {
        Meta.Mob meta = MetaManager.Instance.mobInfo[this.id];
        switch(node.type)
        {
            case ActionType.MOB_CREATE:
                break;
            case ActionType.MOB_MOVING:
            case ActionType.MOB_FLYING:
            {
                Action action = GetMovingFlyingAction(node.type, node.requestInfo.targetMapId);
                if(action.type != ActionType.MAX)
                {
                    actions.Add(action);
                    return true;
                }
                
                return false;
            }
            case ActionType.MOB_ATTACK:
                actions.Add(new Action(node.type, meta.ability.attackSpeed)); //공격 속도
                break;
            case ActionType.MOB_UNDER_ATTACK:
                underAttackQ.Enqueue(new UnderAttack(node.requestInfo.fromObject, (int)node.requestInfo.amount));
                break;
            case ActionType.MOB_DIE:
                actions.Add(new Action(node.type, node.requestInfo));
                break;
        }

        return true;
    }
    public Action GetMovingFlyingAction(ActionType type, int targetMapId)
    {
        Meta.Mob meta =  MetaManager.Instance.mobInfo[this.id];

        int to = targetMapId;
        if(to == -1)
        {
            to = MapManager.Instance.GetRandomNearEmptyMapId(this.mapId, meta.movingRange);
            if(to == -1)
                return new Action(ActionType.MAX, 0);
        }

        //MapManager 변경. 
        MapManager.Instance.Move(mapId, to);

        Action  action = (type == ActionType.MOB_MOVING) ? GetMovingAction(to, meta.ability, type) : GetFlyingAction(to, meta.ability, type);
        if(action.type == ActionType.MAX)
            return action;

        RemoveActionType(type); //이전 이동 액션을 제거
        isMovingStarted = false;

        //Mob map id변경
        this.mapId = to;
        //GameObject parent = MapManager.Instance.defaultGameObjects[to];
        //gameObject.transform.SetParent(parent.transform);

        return action;
    }

    public override void Instantiate()
    {
        Meta.Mob meta = MetaManager.Instance.mobInfo[id];
        Instantiate(meta.prefab, meta.flyingHeight > 0 ? true: false);
    }

    public void Destroy()
    {
        Clear();
        //object삭제
        this.Release();
    }

    public override bool Create(int tribeId, int mapId, int id, bool isInstantiate, float rotation)
    {
        Meta.Mob meta = MetaManager.Instance.mobInfo[id];
        this.Init(tribeId, id, mapId, TAG.MOB, meta.ability.HP, 0, rotation);

        if(isInstantiate)
            this.Instantiate();
            
        MapManager.Instance.SetMapId(mapId, meta.mapCost);
        MapManager.Instance.SetCurrentMap(this, TAG.MOB);
        
        return true;
    }

    public override void Update()
    {
        if(actions.Count == 0)
        {
            elapseRoutine += Time.deltaTime;
            if(elapseRoutine > 1)
            {
                if(this.currentHP > 0 && Util.Random(MetaManager.Instance.mobInfo[this.id].movingProbability))
                {
                    ApplyRoutine();
                }

                elapseRoutine = 0;
            }
            
        }

        List<int> removeActionIds = new List<int>();
        if(actions.Count > 0)
        {
            Action action = actions[0];
            
            action.currentTime += Time.deltaTime;
            actions[0] = action;

            Meta.Mob mob = MetaManager.Instance.meta.mobs[id];

            switch(action.type)
            {
                case ActionType.MOB_MOVING:
                    Moving(action);
                    break;
                case ActionType.MOB_FLYING:
                    Flying(action, mob.flyingHeight);
                    break;
                case ActionType.MOB_DIE:
                    SetAnimation(ActionType.MOB_DIE);
                    break;
            }

            //finish
            if(action.currentTime >= action.totalTime)
            {
                switch(action.type)
                {
                    case ActionType.MOB_CREATE:
                        Context.Instance.onCreationFinish(action.type, this);
                        break;
                    case ActionType.MOB_DIE:
                        Context.Instance.onDie(action.type, this, action.requestInfo.fromObject);
                        Destroy();
                        return;
                }
                actions.RemoveAt(0);
            }
        }
        SetCurrentMapId();
    }
    public override void UpdateUnderAttack()
    {
        Meta.Mob meta = MetaManager.Instance.mobInfo[this.id];
        while(underAttackQ.Count > 0)
        {
            UnderAttack p = underAttackQ.Dequeue();
            
            this.currentHP -= p.amount;

            //callback
            Context.Instance.onAttack(p.from, this, p.amount);

            ShowHP(meta.ability.HP);
            if(this.currentHP <= 0)
            {
                HideProgress();
                underAttackQ.Clear();
                this.actions.Clear();

                QNode q = new QNode(ActionType.MOB_DIE, this.seq);
                q.requestInfo.fromObject = p.from;
                Updater.Instance.AddQ(q);
                return;
            }
            else
            {
                //도망가기
                if(!this.HasActionType(ActionType.MOB_ATTACK))
                {
                    QNode q = new QNode(meta.flyingHeight > 0 ? ActionType.MOB_FLYING : ActionType.MOB_MOVING, this.seq);
                    q.requestInfo.targetMapId = MapManager.Instance.GetRandomNearEmptyMapId(this.mapId, meta.movingRange);
                    Updater.Instance.AddQ(q);
                }
                    
            }
            return;
        }
    }

    public override void UpdateDefence()
    {

    }
    public override void UpdateEarning()
    {
    }
}