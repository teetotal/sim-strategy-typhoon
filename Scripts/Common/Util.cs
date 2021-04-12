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
        return int.Parse(name.Replace("(Clone)", ""));
    }

    public static bool Random(int probability)
    {
        if(UnityEngine.Random.Range(0, probability) == 0)
        {
            return true;
        }

        return false;
    }
}