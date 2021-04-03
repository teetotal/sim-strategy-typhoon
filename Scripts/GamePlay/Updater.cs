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

    public List<Object> Update()
    {
        List<Object> list = null;
        if(queue.Count > 0)
        {
            list = new List<Object>();
        }

        while(queue.Count > 0)
        {
            QNode q = queue.Dequeue();
            Object o = Fetch(q);      
            if(o != null)
            {
                list.Add(o); 
            }     
        }

        //nodeManager.nodes로 부터 빌딩, 액터까지 모두 찾아서 이벤트 처리

            //building event 처리
            //actor event 처리
            
            //일정 주기로 update node resource

        

        return list;
    }

    private Object Fetch(QNode q)
    {
        switch(q.type)
        {
            case ActionType.BUILDING_CREATE:
                return nodeManager.Create(q.nodePos, nodeManager.GetName());
            default:
                return null;
        }
    }

    /* ---------------------------------------------------------------------------- */
    public void AddQ(ActionType type, int node, int building, List<int> values)
    {
        queue.Enqueue(new QNode(type, node, building, values));
    }
}