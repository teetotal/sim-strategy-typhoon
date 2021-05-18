using System;
using System.Collections.Generic;
using UnityEngine;
public class GameStatusManager
{
    GameStatus gameStatus;
    public string savedFilePath;
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
        
        this.savedFilePath = savedFile;
        
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
                    ActorManager.Instance.SetActor(seq, p.actorId, p.HP, p.mapId, p.rotation);
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

    private GameStatus.ResourceIdAmount[] GetResourceIdAmounts(int tribeId)
    {
        int resourceCount = MetaManager.Instance.meta.resources.Count;
        GameStatus.ResourceIdAmount[] p = new GameStatus.ResourceIdAmount[resourceCount];

        for(int n = 0; n < resourceCount; n++)
        {
            p[n] = new GameStatus.ResourceIdAmount();
            p[n].resourceId = MetaManager.Instance.meta.resources[n].id;
            p[n].amount = resourceInfo[tribeId][p[n].resourceId];
        }
        
        return p;
    }
    public void Save(string path)
    {
        GameStatus save = new GameStatus();
        /*
        GameStatus.ResourceIdAmount r0 = new GameStatus.ResourceIdAmount();
        r0.resourceId = 0;
        r0.amount = int.Parse(GameObject.Find("resource0").GetComponent<InputField>().text);

        GameStatus.ResourceIdAmount r1 = new GameStatus.ResourceIdAmount();
        r1.resourceId = 1;
        r1.amount = int.Parse(GameObject.Find("resource1").GetComponent<InputField>().text);

        GameStatus.ResourceIdAmount r2 = new GameStatus.ResourceIdAmount();
        r2.resourceId = 2;
        r2.amount = int.Parse(GameObject.Find("resource2").GetComponent<InputField>().text);
        */
        
        SortedDictionary<int, List<BuildingObject>> temp = new SortedDictionary<int, List<BuildingObject>>();
        List<int> list = ObjectManager.Instance.GetObjectSeqs(TAG.BUILDING);
        for(int n = 0; n < list.Count; n++)
        {
            int seq = list[n];
            Object obj = ObjectManager.Instance.Get(seq);

            int tribeId = obj.tribeId;
            if(!temp.ContainsKey(tribeId))
            {
                temp[tribeId] = new List<BuildingObject>();
            }

            temp[tribeId].Add((BuildingObject)obj);
        }

        //tribes
        save.tribes = new List<GameStatus.Tribe>();

        foreach(KeyValuePair<int, List<BuildingObject>> kv in temp)
        {
            GameStatus.Tribe tribe = new GameStatus.Tribe();
            //resource
            tribe.resources = new List<GameStatus.ResourceIdAmount>(GetResourceIdAmounts(save.tribes.Count));
            //building
            tribe.buildings = new List<GameStatus.Building>();
            for(int n = 0; n < kv.Value.Count; n++)
            {
                BuildingObject obj = kv.Value[n];

                GameStatus.Building building = new GameStatus.Building();
                building.mapId = obj.mapId;
                building.buildingId = obj.id;
                building.rotation = obj.gameObject.transform.rotation.eulerAngles.y;

                //actor
                building.actors = new List<GameStatus.MapIdActorIdHP>();
                for(int i = 0; i < kv.Value[n].actors.Count; i++)
                {
                    Actor actor = kv.Value[n].actors[i];
                    GameStatus.MapIdActorIdHP p = new GameStatus.MapIdActorIdHP();
                    p.mapId = actor.mapId;
                    p.actorId = actor.id;
                    p.HP = actor.currentHP;
                    p.rotation = actor.gameObject.transform.rotation.eulerAngles.y;
                    building.actors.Add(p);
                }

                tribe.buildings.Add(building);
            }

            save.tribes.Add(tribe);
        }
        
        //mob
        save.mobs = new List<GameStatus.Mob>();
        List<int> mobs = ObjectManager.Instance.GetObjectSeqs(TAG.MOB);
        for(int n = 0; n < mobs.Count; n++)
        {
            int seq = mobs[n];
            Mob obj = (Mob)ObjectManager.Instance.Get(seq);
            GameStatus.Mob mob = new GameStatus.Mob();
            mob.mapId = obj.attachedId;
            mob.mobId = obj.id;
            mob.amount = 1;
            save.mobs.Add(mob);
        }

        //environment
        save.environments = new List<GameStatus.Environment>();
        foreach(KeyValuePair<int, EnvironmentManager.Environment> kv in EnvironmentManager.Instance.environments)
        {
            GameStatus.Environment env = new GameStatus.Environment();
            env.mapId = kv.Key;
            env.environmentId = kv.Value.id;
            env.rotation = kv.Value.gameObject.transform.rotation.eulerAngles.y;

            save.environments.Add(env);
        }

        //neutral
        save.neutrals = new List<GameStatus.Neutral>();
        List<int> neutrals = ObjectManager.Instance.GetObjectSeqs(TAG.NEUTRAL);
        for(int n = 0; n < neutrals.Count; n++)
        {
            int seq = neutrals[n];
            Object p = ObjectManager.Instance.Get(seq);

            GameStatus.Neutral neutral = new GameStatus.Neutral();
            neutral.mapId = p.mapId;
            neutral.neutralId = p.id;
            neutral.rotation = p.gameObject.transform.rotation.eulerAngles.y;

            save.neutrals.Add(neutral);
        }
        
        path = Json.SaveJsonFile(path, save);
        Debug.Log(string.Format("Save {0}", path));
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
        if(!resourceInfo.ContainsKey(tribeId))
        {
            resourceInfo[tribeId] = new Dictionary<int, float>();
        }

        if(!resourceInfo[tribeId].ContainsKey(resourceId))
        {
            resourceInfo[tribeId][resourceId] = amount;
        }
        else
        {
            resourceInfo[tribeId][resourceId] += amount;
        }
    }
    public void ReduceResource(int tribeId, int resourceId, float amount)
    {
        resourceInfo[tribeId][resourceId] -= amount;
    }
}