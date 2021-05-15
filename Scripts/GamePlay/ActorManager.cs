using System;
using System.Collections.Generic;
using UnityEngine;
public class ActorManager
{
    private static readonly Lazy<ActorManager> hInstance = new Lazy<ActorManager>(() => new ActorManager());
    public static ActorManager Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected ActorManager()
    {
    }
    public void Fetch(QNode q)
    {
        if(q.type == ActionType.ACTOR_CREATE)
        {
            q.tribeId = q.requestInfo.fromObject.tribeId;
            if(!Context.Instance.onCreationEvent(q))
                return;
            Actor actor = Create((BuildingObject)q.requestInfo.fromObject, q.id, true);
            q.requestInfo.mySeq = actor.seq;
        }
        
        Object obj = ObjectManager.Instance.Get(q.requestInfo.mySeq);
        obj.AddAction(q);
    }

    public void SetActor(int buildingSeq, int actorId, float HP)
    {
        BuildingObject building = (BuildingObject)ObjectManager.Instance.Get(buildingSeq);
        Actor actor = Create(building, actorId, false);
        actor.currentHP = HP;
    }

    private Actor Create(BuildingObject building, int id, bool isInstantiate)
    {
        //BuildingObject building = BuildingManager.Instance.objects[buildingMapId];
        //BuildingObject building = (BuildingObject)ObjectManager.Instance.Get(buildingSeq);
        int tribeId = building.tribeId;
        
        Actor obj = new Actor();
        if(obj.Create(tribeId, building.mapId, id, isInstantiate))
        {
            //actor등록
            //actors[obj.mapId] = obj;
            //building에 actor등록
            building.actors.Add(obj);
            //actor에 building등록
            obj.attachedBuilding = building;

            return obj;
        }

        return null;
    }
    
    public void Update(bool onlyBasicUpdate)
    {
        List<int> seqs = ObjectManager.Instance.GetObjectSeqs(TAG.ACTOR);
        
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
        List<Actor> list = new List<Actor>();
        foreach(KeyValuePair<int, Actor> kv in actors)
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