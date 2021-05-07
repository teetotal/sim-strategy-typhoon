/*
바닥을 다양하게 표현하기 위한 함수.
게임마다 이 부분만 수정하거나 스테이지 마다 구성하면 될 듯 
*/
public class Bottom{
    public static string GetBottomPrefab(int x, int y)
    {
        int count = MetaManager.Instance.environmentInfo.Count;
        int[] arr = new int[5] { 0, 0, 0, 0, 18 };
        int n = UnityEngine.Random.Range(0, arr.Length);
        string prefab = MetaManager.Instance.environmentInfo[arr[n]].name;
        //MapManager.Instance.mapMeta.defaultVal.prefabId
        return prefab;
    }

}