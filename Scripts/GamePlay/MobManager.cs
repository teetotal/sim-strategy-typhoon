using System;
using System.Collections.Generic;
using UnityEngine;
public class MobManager
{
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
        if(q.type == ActionType.MOB_CREATE)
        {   
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

            //routine 추가
            obj.routine = new List<QNode>()
            {
                new QNode(
                    meta.flyingHeight == 0 ? ActionType.MOB_MOVING : ActionType.MOB_FLYING, 
                    -1, -1, -1, null, false, -1)
            };
            
        }
        else
        {
            mobs[q.mapId].AddAction(q);
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
        Dictionary<int, Dictionary<int, int>> cnts = new Dictionary<int, Dictionary<int, int>>(); //소속 위치, mob id, count
        foreach(KeyValuePair<int, Mob> kv in mobs)
        {
            if(!cnts.ContainsKey(kv.Value.attachedId))
            {
                cnts[kv.Value.attachedId] = new Dictionary<int, int>();
                cnts[kv.Value.attachedId][kv.Value.id] = 0;
            }
                

            cnts[kv.Value.attachedId][kv.Value.id]++;
        }

        for(int n = 0; n < MapManager.Instance.mapMeta.mobs.Count; n++)
        {
            Map.Mob meta = MapManager.Instance.mapMeta.mobs[n];
            if(meta.max == 0)
                continue;

            if(cnts.ContainsKey(meta.mapId) && cnts[meta.mapId].ContainsKey(meta.id) && meta.max < cnts[meta.mapId][meta.id])
                continue;

            Updater.Instance.AddQ(ActionType.MOB_CREATE, -1, meta.mapId, meta.id, null, true);
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