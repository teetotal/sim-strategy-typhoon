using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Lobby start");
        SceneManager.LoadScene("GamePlay");
        Debug.Log("Lobby end");
    }
}
