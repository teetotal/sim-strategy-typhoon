using UnityEngine; 
public abstract class IUIInterface : MonoBehaviour
{
    public abstract void Init();
    public abstract void Show();
    public abstract void UpdateUI();
    public abstract void Close();
}