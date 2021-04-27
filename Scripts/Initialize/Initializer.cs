using UnityEngine;

public class Initializer : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        MetaManager.Instance.Load();
        MarketManager.Instance.Load();

        MapManager.Instance.Load();
        GameStatusManager.Instance.Load();
        
        Debug.Log("Loading completed");
    }
}
