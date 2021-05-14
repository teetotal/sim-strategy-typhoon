using System;
using System.Collections.Generic;
using UnityEngine;
public class NeutralManager
{
    public Dictionary<int, NeutralBuilding> objects = new Dictionary<int, NeutralBuilding>();
    private static readonly Lazy<NeutralManager> hInstance = new Lazy<NeutralManager>(() => new NeutralManager());
    
    public static NeutralManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected NeutralManager()
    {
    }

    public void Clear()
    {
        objects.Clear();
    }

    public void Fetch(QNode q)
    {
        switch(q.type)
        {
            case ActionType.NEUTRAL_CREATE:
                if(Context.Instance.onCreationEvent(q))
                    Create(q.mapId, q.id, 0, true);
                break;
            case ActionType.NEUTRAL_DESTROY:
                Destroy(q.mapId);
                break;
            default:
                return;
        }
    }
    /*
    public bool SetBuilding(int mapId, int id)
    {
        NeutralBuilding obj = new NeutralBuilding();
        if(obj.Create(-1, mapId, id, false))
        {
            objects[obj.mapId] = obj;
            MapManager.Instance.AssignBuilding(obj.mapId);
            return true;
        }

        return false;
    }
    
    public void Construct(QNode q)
    {
        //화면 처리에 필요한 object 설정
        NeutralBuilding obj = new NeutralBuilding();
        //map에 설정 & prefab생성. environment object를 map에 적절히 assign해야 해서 mapmanager에서 처리함
        //obj.gameObject = MapManager.Instance.CreateNeutral(q.mapId, MetaManager.Instance.neutralInfo[q.id].prefab); //건물의 a* cost는 -1. 지나가지 못함
            
        if(obj.Create(q.tribeId, q.mapId, q.id, true))
        {
            obj.actions.Add(new Action(ActionType.NEUTRAL_CREATE, 0, null));   
            objects[obj.mapId] = obj;

            //roatation
            if(q.values != null && q.values.Count == 1)
            {
                obj.gameObject.transform.localEulerAngles += new Vector3(0, q.values[0], 0);
            }
        }
    }
    */
    public bool Create(int mapId, int id, float roatation, bool isInstantiate)
    {
        NeutralBuilding obj = new NeutralBuilding();
        obj.rotation = roatation;
        if(obj.Create(-1, mapId, id, isInstantiate))
        {
            objects[obj.mapId] = obj;
            MapManager.Instance.AssignBuilding(obj.mapId);
            return true;
        }
        return false;
    }
    public void Instantiate()
    {
        foreach(KeyValuePair<int, NeutralBuilding> kv in objects)
        {
            kv.Value.Instantiate();
            kv.Value.gameObject.transform.localEulerAngles += new Vector3(0, kv.Value.rotation, 0);
        }
    }

    public void Destroy(int mapId)
    {
        MapManager.Instance.Remove(mapId, TAG.NEUTRAL);
        GameObject.Destroy(objects[mapId].gameObject); //map maker 에서만 쓰이니까 풀링 안한다.
        objects.Remove(mapId);

    }
    public void Update()
    {
        foreach(KeyValuePair<int, NeutralBuilding> kv in objects)
        {
            kv.Value.Update();
            kv.Value.UpdateUIPosition();
            kv.Value.UpdateUnderAttack();
            kv.Value.UpdateDefence();
        }
    }
}