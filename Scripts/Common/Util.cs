using UnityEngine;
public class Util
{
    public static Vector3 AdjustY(Vector3 pos, bool flying)
    {
        if(!flying)
        {
            return pos + new Vector3(0, 0.1f, 0);
        }
        return new Vector3(pos.x, 1, pos.z);
    }
    public static Vector3 GetFlyingPosition(Vector3 pos, float height)
    {
        return new Vector3(pos.x, height, pos.z);
    }
    public static int GetIntFromGameObjectName(string name)
    {
        string sz = name.Replace("(Clone)", "");
        int n = -1;
        if(int.TryParse(sz,  out n))
            return n;
        return -1;
    }
    public static string GetObjectName(GameObject obj)
    {
        return obj.name.Replace("(Clone)", "");
    }

    public static bool Random(int probability)
    {
        if(UnityEngine.Random.Range(0, probability) == 0)
        {
            return true;
        }

        return false;
    }
    public static string GetNameInGame(TAG tag, int id)
    {
        switch(tag)
        {
            case TAG.BUILDING:
                return MetaManager.Instance.buildingInfo[id].name;
            case TAG.ACTOR:
                return MetaManager.Instance.actorInfo[id].name;
            case TAG.MOB:
                return MetaManager.Instance.mobInfo[id].name;
            case TAG.NEUTRAL:
                return MetaManager.Instance.neutralInfo[id].name;
        }

        return string.Empty;
    }

    public static int GetIdInGame(TAG tag, int mapId)
    {
        switch(tag)
        {
            case TAG.BUILDING:
                return BuildingManager.Instance.objects[mapId].id;
            case TAG.ACTOR:
                return ActorManager.Instance.actors[mapId].id;
            case TAG.MOB:
                return MobManager.Instance.mobs[mapId].id;
            case TAG.NEUTRAL:
                return NeutralManager.Instance.objects[mapId].id;
        }

        return -1;
    }
}