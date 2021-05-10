using System;
using System.Collections.Generic;
using UnityEngine;
public class ActorManager
{
    public Dictionary<int, Actor> actors = new Dictionary<int, Actor>();
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
    public void Clear()
    {
        actors.Clear();
    }
    public void Fetch(QNode q)
    {
        int mapId = q.mapId;
        if(q.type == ActionType.ACTOR_CREATE)
        {
            q.tribeId = BuildingManager.Instance.objects[mapId].tribeId;
            if(!Context.Instance.onCreationEvent(q))
                return;
            Actor actor = Create(q.mapId, q.id, true);
            mapId = actor.mapId;
        }
        
        if(actors.ContainsKey(mapId) == false)
        {
            Debug.Log(string.Format("Invalid mapId. {0}", q.type.ToString()));
        }
        else
        {
            actors[mapId].AddAction(q);
        }
    }

    public void SetActor(int buildingMapId, int actorId, float HP)
    {
        Actor actor = Create(buildingMapId, actorId, false);
        actor.currentHP = HP;
    }

    private Actor Create(int buildingMapId, int id, bool isInstantiate)
    {
        BuildingObject building = BuildingManager.Instance.objects[buildingMapId];
        int tribeId = building.tribeId;
        
        Actor obj = new Actor();
        if(obj.Create(building.tribeId, buildingMapId, id, isInstantiate))
        {
            //actor등록
            actors[obj.mapId] = obj;
            //building에 actor등록
            building.actors.Add(obj);
            //actor에 building등록
            obj.attachedBuilding = building;

            return obj;
        }

        return null;
    }
    
    public void Update()
    {
        List<Actor> list = new List<Actor>();
        foreach(KeyValuePair<int, Actor> kv in actors)
        {
            list.Add(kv.Value);
        }

        for(int n = 0; n < list.Count; n++)
        {
            list[n].Update();
            list[n].UpdateUIPosition();
            list[n].UpdateUnderAttack();
            list[n].UpdateDefence();
            list[n].UpdateEarning();
        }
    }
}