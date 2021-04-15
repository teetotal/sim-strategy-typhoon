using System;
using System.Collections.Generic;
using UnityEngine;

public class Updater 
{
    private Queue<QNode> queue = new Queue<QNode>();

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
        BuildingManager.Instance.Update();
        //actor event 처리
        ActorManager.Instance.Update();
        //mob
        MobManager.Instance.Update();
        //mob regen
        MobManager.Instance.Regen();
        //일정 주기로 update node resource
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
    }

    /* ---------------------------------------------------------------------------- */
    public void AddQ(ActionType type, int mapId, int id, List<int> values, bool immediately)
    {
        queue.Enqueue(new QNode(type, mapId, id, values, immediately));
    }
}