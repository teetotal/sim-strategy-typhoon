using System;
using System.Collections.Generic;
using UnityEngine;
public class MobManager
{
    struct RegenCounting
    {
        public int mobId;
        public int amount;

        public RegenCounting(int mobId, int amount)
        {
            this.mobId = mobId;
            this.amount = amount;
        }
    }
    float time;
    int lastRegenTime = 0;
    public Dictionary<int, Mob> mobs = new Dictionary<int, Mob>();
    private static readonly Lazy<MobManager> hInstance = new Lazy<MobManager>(() => new MobManager());
    
    public static MobManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected MobManager()
    {
    }
    public void Clear()
    {
        mobs.Clear();
    }
    public void Fetch(QNode q)
    {
        switch(q.type) 
        {   
            case ActionType.MOB_CREATE:
                Meta.Mob meta = MetaManager.Instance.mobInfo[q.id];
                //probability
                if(!Util.Random(meta.regenProbability))
                    return;

                int mapId = MapManager.Instance.GetRandomNearEmptyMapId(q.mapId, meta.movingRange); 
                if(mapId == -1)
                    return;

                if(!Context.Instance.onCreationEvent(q))
                    return;

                /*
                Mob obj = new Mob();
                obj.attachedId = q.mapId;   //소속 위치 
                if(obj.Create(q.tribeId, mapId, q.id, true))
                {
                    mobs[obj.mapId] = obj;
                }
                */
                Mob obj = Create(mapId, q.mapId, q.id, 0, true);
                if(obj == null)
                    return;
            break;
            default:
                ObjectManager.Instance.Get(q.requestInfo.mySeq).AddAction(q);
                //mobs[q.mapId].AddAction(q);
            break;
        } 
    }
    public void Instantiate()
    {
        foreach(KeyValuePair<int, Mob> kv in mobs)
        {
            kv.Value.Instantiate();
        }
    }
    public Mob Create(int mapId, int regenMapId, int id, float roatation, bool isInstantiate)
    {
        Meta.Mob meta = MetaManager.Instance.mobInfo[id];

        Mob obj = new Mob();
        obj.attachedId = regenMapId;   //소속 위치 
        if(obj.Create(-1, mapId, id, isInstantiate))
        {
            mobs[obj.mapId] = obj;

            //routine 추가
            QNode q = new QNode();
            q.type = meta.flyingHeight == 0 ? ActionType.MOB_MOVING : ActionType.MOB_FLYING;
            q.requestInfo.mySeq = obj.seq;
            q.requestInfo.targetMapId = -1;

            obj.routine = new List<QNode>()
            {
                q
                /*
                new QNode(
                    meta.flyingHeight == 0 ? ActionType.MOB_MOVING : ActionType.MOB_FLYING, 
                    -1, -1, -1, null, false, -1)
                */

            };
            return obj;
        }

        return null;
    }

    public void Regen()
    {
        time += Time.deltaTime;
        int t = (int)time;
        if(lastRegenTime == t || t % 2 != 0)
            return;

        //소속된 위치 정보가 있어야 함
        Dictionary<int, RegenCounting> cnts = new Dictionary<int, RegenCounting>(); //소속 위치, mob id, count
        foreach(KeyValuePair<int, Mob> kv in mobs)
        {
            if(!cnts.ContainsKey(kv.Value.attachedId))
            {
                cnts[kv.Value.attachedId] = new RegenCounting(kv.Value.id, 1);
            }
            else
            {
                RegenCounting p = cnts[kv.Value.attachedId];
                p.amount++;
                cnts[kv.Value.attachedId] = p;
            }
        }

        foreach(KeyValuePair<int, RegenCounting> kv in cnts)
        {
            int regenMapId = kv.Key;
            RegenCounting v = kv.Value;
            Meta.Mob meta = MetaManager.Instance.mobInfo[v.mobId];
            if(meta.max > v.amount)
            {
                Updater.Instance.AddQ(ActionType.MOB_CREATE, -1, regenMapId, v.mobId, null, true);
            }
        }
    }
    public void Update(bool onlyBasicUpdate)
    {
        List<Mob> list = new List<Mob>();
        foreach(KeyValuePair<int, Mob> kv in mobs)
        {
            list.Add(kv.Value);
        }

        for(int n = 0; n < list.Count; n++)
        {
            list[n].Update();
            if(!onlyBasicUpdate)
            {
                list[n].UpdateUIPosition();
                list[n].UpdateUnderAttack();
                list[n].UpdateDefence();
            }
        }
    }
}