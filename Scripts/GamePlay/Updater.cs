using System;
using System.Collections.Generic;
using UnityEngine;

public class Updater 
{
    private Queue<QNode> queue = new Queue<QNode>();
    public bool onlyBasicUpdate = false;

    private static readonly Lazy<Updater> hInstance = new Lazy<Updater>(() => new Updater());
 
    public static Updater Instance
    {
        get {
            return hInstance.Value;
        } 
    }

    protected Updater()
    {
    }

    public void Update()
    {
        while(queue.Count > 0)
        {
            Fetch(queue.Dequeue());     
        }

        //building event 처리
        BuildingManager.Instance.Update(onlyBasicUpdate);
        //actor event 처리
        ActorManager.Instance.Update(onlyBasicUpdate);
        //mob
        MobManager.Instance.Update(onlyBasicUpdate);
        if(!onlyBasicUpdate)
        {
            //mob regen
            MobManager.Instance.Regen();
            //객체 선택 ui
            SelectionUI.Instance.Update();
            //trading 시세 기록
            TradingManager.Instance.Update();
        }
        
    }

    private void Fetch(QNode q)
    {
        if(q.type < ActionType.BUILDING_MAX)
        {
            BuildingManager.Instance.Fetch(q);
        }
        else if(q.type < ActionType.ACTOR_MAX)
        {
            ActorManager.Instance.Fetch(q);
        }
        else if(q.type < ActionType.MOB_MAX)
        {
            MobManager.Instance.Fetch(q);
        }
        else if(q.type < ActionType.NEUTRAL_MAX)
        {
            NeutralManager.Instance.Fetch(q);
        }
    }

    /* ---------------------------------------------------------------------------- */
    public void AddQ(ActionType type, int tribeId, int mapId, int id, List<int> values, bool immediately = false, int insertIndex = -1)
    {
        queue.Enqueue(new QNode(type, tribeId, mapId, id, values, immediately, insertIndex));
    }
    public void AddQs(List<QNode> list)
    {
        for(int n = 0; n < list.Count; n++)
        {
            queue.Enqueue(list[n]);
        }
    }
}