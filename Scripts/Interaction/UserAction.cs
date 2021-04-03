using UnityEngine;

public class UserAction : MonoBehaviour
{
    void Awake()
    {
        MetaManager.Instance.Load();
        MapManager.Instance.Load();
        Context.Instance.isInitialized = true;
    }

    void Update()
    {
        Context.Instance.Update();
    }
}
