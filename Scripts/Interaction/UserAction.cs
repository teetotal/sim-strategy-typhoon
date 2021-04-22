using UnityEngine;

public class UserAction : MonoBehaviour
{
    void Awake()
    {
        MarketManager.Instance.Load();
        MetaManager.Instance.Load();
        MapManager.Instance.Load();
        GameStatusManager.Instance.Load();
        Context.Instance.isInitialized = true;
    }

    void Update()
    {
        Updater.Instance.Update();
        Context.Instance.Update();
    }
}
