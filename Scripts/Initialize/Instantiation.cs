using UnityEngine;

public class Instantiation : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        GameObjectPooling.Instance.Reset();
        MapManager.Instance.CreatePrefabs();
        BuildingManager.Instance.Instantiate();
        NeutralManager.Instance.Instantiate();
        MobManager.Instance.Instantiate();
        //GameStatusManager.Instance.Load();
    }
}
