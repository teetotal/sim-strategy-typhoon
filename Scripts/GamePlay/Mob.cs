using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mob : ActingObject
{
    public int attachedId;
    public override bool AddAction(QNode node)
    {
        switch(node.type)
        {
            case ActionType.MOB_CREATE:
                break;
            case ActionType.MOB_MOVING:
            case ActionType.MOB_FLYING:
            {
                Action action = GetMovingFlyingAction(node.type, node.id);
                if(action.type != ActionType.MAX)
                {
                    actions.Add(action);
                    return true;
                }
                
                return false;
            }
            case ActionType.MOB_ATTACK:
                break;
            case ActionType.MOB_UNDER_ATTACK:
            /*
            id: from mapId
            actions[0]: from TAG
            actions[1]: attack amount
            */
                Object obj = Util.GetObject(node.id, (TAG)node.values[0]);
                //일부러 null 체크 안함
                underAttackQ.Enqueue(new UnderAttack(obj, node.values[1]));
                break;
            case ActionType.MOB_DIE:
            /*
            id: from mapId
            actions[0]: from TAG
            */
                actions.Add(new Action(node.type, 2, new List<int>() { node.id, node.values[0] }));
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
        //MobManager변경
        MobManager.Instance.mobs[to] = this;
        MobManager.Instance.mobs.Remove(mapId);

        Action  action = (type == ActionType.MOB_MOVING) ? GetMovingAction(to, meta.ability, type) : GetFlyingAction(to, meta.ability, type);
        if(action.type == ActionType.MAX)
            return action;

        RemoveActionType(type); //이전 이동 액션을 제거
        isMovingStarted = false;

        //Mob map id변경
        this.mapId = to;
        GameObject parent = MapManager.Instance.defaultGameObjects[to];
        gameObject.name = this.mapId.ToString();
        gameObject.transform.SetParent(parent.transform);

        return action;
    }

    public void Instantiate()
    {
        Instantiate(-1, mapId, id, MetaManager.Instance.mobInfo[id].prefab, TAG.MOB, false);
    }

    public override bool Create(int tribeId, int mapId, int id, bool isInstantiate)
    {
        Meta.Mob mob = MetaManager.Instance.meta.mobs[id];
        //HP
        this.currentHP = mob.ability.HP;
        //level
        this.level = 0;

        this.tribeId = tribeId;
        this.id = id;
        this.mapId = mapId;

        if(isInstantiate)
            this.Instantiate(tribeId, mapId, id, mob.prefab, TAG.MOB, mob.flyingHeight > 0 ? true: false);
            
        MapManager.Instance.SetMapId(mapId, mob.mapCost);

        MapManager.Instance.SetCurrentMap(this, TAG.MOB);
        
        return true;
    }

    public override void Update()
    {
        if(actions.Count == 0)
        {
            if(Util.Random(MetaManager.Instance.mobInfo[this.id].movingProbability))
                ApplyRoutine();
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
                        Context.Instance.onDie(action.type, this, Util.GetObject(action.values[0], (TAG)action.values[1]));
                        Clear();
                        //object삭제
                        this.DestroyGameObject();
                        MobManager.Instance.mobs.Remove(this.mapId);
                        MapManager.Instance.Remove(this.mapId, TAG.MOB);
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
                Updater.Instance.AddQ(ActionType.MOB_DIE, this.tribeId, this.mapId, p.from.mapId
                    , new List<int>() { (int)MetaManager.Instance.GetTag(p.from.gameObject.tag) }
                    , false);
                return;
            }

            //도망가기
            /*
            if(!this.HasActionType(ActionType.ACTOR_ATTACK))
                Updater.Instance.AddQ(
                    meta.flying ? ActionType.ACTOR_FLYING : ActionType.ACTOR_MOVING, 
                    this.mapId,
                    MapManager.Instance.GetRandomNearEmptyMapId(this.mapId, 2),
                    null,
                    false
                    );
            
            */
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