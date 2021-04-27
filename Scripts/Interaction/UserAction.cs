using UnityEngine;

public class UserAction : MonoBehaviour
{
    void Update()
    {
        Updater.Instance.Update();
        Context.Instance.Update();
    }
}
