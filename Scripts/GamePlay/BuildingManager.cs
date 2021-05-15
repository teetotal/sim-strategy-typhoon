using System;
using System.Collections.Generic;
using UnityEngine;
public class BuildingManager
{
    private static readonly Lazy<BuildingManager> hInstance = new Lazy<BuildingManager>(() => new BuildingManager());
    public static BuildingManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected BuildingManager()
    {
    }
    public void Fetch(QNode q)
    {
        if(q.type == ActionType.BUILDING_CREATE)
        {
            if(Context.Instance.onCreationEvent(q))
            {
                q.requestInfo.mySeq = Create(q.tribeId, q.mapId, q.id, 0, true);
            }
            else
            {
                return;
            }
        }

        Object obj = ObjectManager.Instance.Get(q.requestInfo.mySeq);
        if(obj != null)
        {
            obj.AddAction(q);
        }
        /*
        if(objects.ContainsKey(q.mapId))
        {
            objects[q.mapId].AddAction(q);
        }
        */
    }

    public int SetBuilding(int tribeId, int mapId, int id, float roatation)
    {
        return Create(tribeId, mapId, id, roatation, false);
    }

    private int Create(int tribeId, int mapId, int id, float roatation, bool isInstantiate)
    {
        BuildingObject obj = new BuildingObject();
        obj.rotation = roatation;
        if(obj.Create(tribeId, mapId, id, isInstantiate))
        {
            //objects[obj.mapId] = obj;
            MapManager.Instance.AssignBuilding(obj.mapId);
            return obj.seq;
        }

        return -1;
    }
    /*
    public void Instantiate()
    {
        foreach(KeyValuePair<int, BuildingObject> kv in objects)
        {
            kv.Value.Instantiate();
            kv.Value.gameObject.transform.localEulerAngles += new Vector3(0, kv.Value.rotation, 0);
            for(int n = 0; n < kv.Value.actors.Count; n++)
            {
                Actor actor = kv.Value.actors[n];
                actor.Instantiate();
            }
        }
    }
    */
    /*
    public void Construct(QNode q)
    {
        //화면 처리에 필요한 object 설정
        BuildingObject obj = new BuildingObject();
        //map에 설정 & prefab생성. environment object를 map에 적절히 assign해야 해서 mapmanager에서 처리함
        //obj.gameObject = MapManager.Instance.CreateBuilding(q.mapId, MetaManager.Instance.buildingInfo[q.id].level[0].prefab); //건물의 a* cost는 -1. 지나가지 못함
            
        if(obj.Create(q.tribeId, q.mapId, q.id, true))
        {
            obj.actions.Add(new Action(ActionType.BUILDING_CREATE, q.immediately ? 0 : MetaManager.Instance.buildingInfo[q.id].level[0].buildTime, null));   
            objects[obj.mapId] = obj;

            //roatation
            if(q.values != null && q.values.Count == 1)
            {
                obj.gameObject.transform.localEulerAngles += new Vector3(0, q.values[0], 0);
            }
        }
    }
    */
    public void Update(bool onlyBasicUpdate)
    {
        List<int> seqs = ObjectManager.Instance.GetObjectSeqs(TAG.BUILDING);
        
        for(int n = 0; n < seqs.Count; n++)
        {
            Object obj = ObjectManager.Instance.Get(seqs[n]);
            if(obj != null)
            {
                obj.Update();
                if(!onlyBasicUpdate)
                {
                    obj.UpdateUIPosition();
                    obj.UpdateUnderAttack();
                    obj.UpdateDefence();
                    obj.UpdateEarning();
                }
            }
        }
        /*
        List<BuildingObject> list = new List<BuildingObject>();
        foreach(KeyValuePair<int, BuildingObject> kv in objects)
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
                list[n].UpdateEarning();
            }
        }
        */
    }
}