using UnityEngine;

public class Instantiation : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        MapManager.Instance.CreatePrefabs();
        //GameStatusManager.Instance.Load();
        Debug.Log("CreatePrefabs completed");
    }
}
