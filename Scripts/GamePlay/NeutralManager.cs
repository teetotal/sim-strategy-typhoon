using System;
using System.Collections.Generic;
using UnityEngine;
public class NeutralManager
{
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

    public void Fetch(QNode q)
    {
        switch(q.type)
        {
            case ActionType.NEUTRAL_CREATE:
                if(Context.Instance.onCreationEvent(q))
                    Create(q.mapId, q.id, 0, true);
                break;
            case ActionType.NEUTRAL_DESTROY:
                ((NeutralBuilding)ObjectManager.Instance.Get(q.requestInfo.mySeq)).Destroy();
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
            //objects[obj.mapId] = obj;
            MapManager.Instance.AssignBuilding(obj.mapId);
            return true;
        }
        return false;
    }
    public void Update()
    {
        List<int> seqs = ObjectManager.Instance.GetObjectSeqs(TAG.NEUTRAL);
        
        for(int n = 0; n < seqs.Count; n++)
        {
            Object obj = ObjectManager.Instance.Get(seqs[n]);
            if(obj != null)
            {
                obj.Update();
                obj.UpdateUIPosition();
                obj.UpdateUnderAttack();
                obj.UpdateDefence();
            }
        }
        /*
        foreach(KeyValuePair<int, NeutralBuilding> kv in objects)
        {
            kv.Value.Update();
            kv.Value.UpdateUIPosition();
            kv.Value.UpdateUnderAttack();
            kv.Value.UpdateDefence();
        }
        */
    }
}