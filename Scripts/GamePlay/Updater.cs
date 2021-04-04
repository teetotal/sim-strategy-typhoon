using System;
using System.Collections.Generic;
using UnityEngine;

public class Updater 
{
    private NodeManager nodeManager = new NodeManager();                //node
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

        //nodeManager.nodes로 부터 빌딩, 액터까지 모두 찾아서 이벤트 처리

            //building event 처리
        BuildingManager.Instance.Update();
            //actor event 처리
            
            //일정 주기로 update node resource
    }

    private void Fetch(QNode q)
    {
        switch(q.type)
        {
            case ActionType.BUILDING_CREATE:
                BuildingManager.Instance.Construct(q.mapId, q.buildingId);
                break;
            default:
                return;
        }
    }

    /* ---------------------------------------------------------------------------- */
    public void AddQ(ActionType type, int id, int building, List<int> values)
    {
        queue.Enqueue(new QNode(type, id, building, values));
    }
}