using System;
using System.Collections.Generic;
using UnityEngine;
public class GameStatusManager
{
    GameStatus gameStatus;
    /*
    TribeId,
    ResourceId
    amount
    */
    public Dictionary<int, Dictionary<int, float>> resourceInfo = new Dictionary<int, Dictionary<int, float>>();
    
    private static readonly Lazy<GameStatusManager> hInstance = new Lazy<GameStatusManager>(() => new GameStatusManager());
    public static GameStatusManager Instance { get { return hInstance.Value; } }
    protected GameStatusManager() {}
    public void Load(string savedFile)
    {
        if(savedFile == "")
            return;
        
        resourceInfo.Clear();

        gameStatus = Json.LoadJsonFile<GameStatus>(savedFile);
        
        //tribes
        for(int tribeId = 0; tribeId < gameStatus.tribes.Count; tribeId++)
        {
            //resources
            if(!resourceInfo.ContainsKey(tribeId))
            {
                resourceInfo[tribeId] = new Dictionary<int, float>();
            }

            GameStatus.Tribe tribe = gameStatus.tribes[tribeId];
            for(int m = 0; m < tribe.resources.Count; m++)
            {
                GameStatus.ResourceIdAmount r = tribe.resources[m];
                resourceInfo[tribeId][r.resourceId] = r.amount;
            }
            //buildings
            for(int m = 0; m < tribe.buildings.Count; m++)
            {
                GameStatus.Building building = tribe.buildings[m];
                int seq = BuildingManager.Instance.SetBuilding(tribeId, building.mapId, building.buildingId, building.rotation);
                /*
                Updater.Instance.AddQ(ActionType.BUILDING_CREATE, 
                                        n,
                                        building.mapId, building.buildingId, new List<int>() {  (int)building.rotation }, true);
                */
                //actors
                for(int i = 0; i < building.actors.Count; i++)
                {
                    GameStatus.MapIdActorIdHP p = building.actors[i];
                    ActorManager.Instance.SetActor(seq, p.actorId, p.HP);
                    //Updater.Instance.AddQ(ActionType.ACTOR_CREATE, n, building.mapId, p.actorId, new List<int>() { p.HP }, true);
                }
            }
        }
        
        //Neutral
        for(int n=0; n < gameStatus.neutrals.Count; n++)
        {
            GameStatus.Neutral neutral = gameStatus.neutrals[n];
            NeutralManager.Instance.Create(neutral.mapId, neutral.neutralId, neutral.rotation, false);
        }

        //environment
        for(int n=0; n < gameStatus.environments.Count; n++)
        {
            GameStatus.Environment environment = gameStatus.environments[n];
            EnvironmentManager.Instance.Create(environment.mapId, environment.environmentId, environment.rotation, false);
        }
        //mob
        for(int n = 0; n < gameStatus.mobs.Count; n++)
        {
            GameStatus.Mob mob = gameStatus.mobs[n];
            MobManager.Instance.Create(mob.mapId, mob.mapId, mob.mobId, 0, false);
        }
    }
    public float GetResource(int tribeId, int resourceId)
    {
        if(!resourceInfo[tribeId].ContainsKey(resourceId))
            return 0;
        return resourceInfo[tribeId][resourceId];
    }
    public bool Spend(int tribeId, List<Meta.ResourceIdAmount> resources)
    {
        //Debug.Log("Spend");
        //check validation
        for(int n = 0; n < resources.Count; n++)
        {
            if(GetResource(tribeId, resources[n].resourceId) < resources[n].amount)
                return false;
        }
        //spend
        for(int n = 0; n < resources.Count; n++)
        {
            resourceInfo[tribeId][resources[n].resourceId] -= resources[n].amount;
        }
        return true;
        
    }
    public bool Earn(int tribeId, List<Meta.ResourceIdAmount> resources)
    {
        Debug.Log("Earn");
        //earn
        for(int n = 0; n < resources.Count; n++)
        {
            resourceInfo[tribeId][resources[n].resourceId] += resources[n].amount;
        }
        return true;
    }
    public void AddResource(int tribeId, int resourceId, float amount)
    {
        resourceInfo[tribeId][resourceId] += amount;
    }
    public void ReduceResource(int tribeId, int resourceId, float amount)
    {
        resourceInfo[tribeId][resourceId] -= amount;
    }
}