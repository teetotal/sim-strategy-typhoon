using System.Collections.Generic;
using UnityEngine;
using System;
public class GameSystem
{
    private static readonly Lazy<GameSystem> hInstance = new Lazy<GameSystem>(() => new GameSystem());
    public static GameSystem Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected GameSystem()
    {
    }

    public void OnTargetingBuilding(Actor actor, int targetMapId)
    {
        Meta.Actor metaActor = MetaManager.Instance.actorInfo[actor.id];
        BuildingObject building = BuildingManager.Instance.objects[targetMapId];
        Meta.Building metaBuilding = MetaManager.Instance.buildingInfo[building.id];

        Debug.Log(string.Format("OnActorEvent {0} -> {1}", metaActor.name, metaBuilding.name));
    }
}