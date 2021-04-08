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
            
        //일정 주기로 update node resource
    }

    private void Fetch(QNode q)
    {
        switch(q.type)
        {
            case ActionType.BUILDING_CREATE:
                BuildingManager.Instance.Construct(q.mapId, q.id);
                break;
            case ActionType.BUILDING_DESTROY:
                BuildingManager.Instance.Destroy(q.mapId);
                break;
            case ActionType.ACTOR_CREATE:
                ActorManager.Instance.Create(q.mapId, q.id);
                break;
            case ActionType.ACTOR_MOVING:
                ActorManager.Instance.Moving(q);
                break;
            default:
                return;
        }
    }

    /* ---------------------------------------------------------------------------- */
    public void AddQ(ActionType type, int mapId, int id, List<int> values)
    {
        queue.Enqueue(new QNode(type, mapId, id, values));
    }
}