using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    public Transform canvas;
    // Start is called before the first frame update
    void Start()
    {
        LoaderPerspective.Instance.SetUI(Camera.main, ref canvas, OnClickButton);
        if(!LoaderPerspective.Instance.LoadJsonFile("lobby"))
        {
            Debug.LogError("lobby.json loading failure");
        } 
        else
        {
            LoaderPerspective.Instance.AddComponents(OnCreate, OnCreatePost);
        }
    }
    void OnCreatePost(GameObject obj, string layerName)
    {
    }
    void OnClickButton(GameObject obj)
    {
        //Debug.Log(string.Format("OnClick {0}, {1}", obj.name, Context.Instance.mode));
        string name = Util.GetObjectName(obj);
        switch(name)
        {
            case "levelup":
            //SceneManager.LoadScene("LevelUp");
            Test();
            break;
            case "play":
            SceneManager.LoadScene("GamePlay");
            
            break;
            default:
            break;
        }
    }
    GameObject OnCreate(string layerName,string name, string tag, Vector2 position, Vector2 size)
    {
        return null;
    } 

    void Test()
    {
        int resourceId = 1;
        bool ret = TradingManager.Instance.Buy(0, resourceId, 10);
        float sellPrice = TradingManager.Instance.GetSellPrice(resourceId);
        float buyPrice = TradingManager.Instance.GetBuyPrice(resourceId);
        int holding = TradingManager.Instance.GetHoding(resourceId);

        Debug.Log(string.Format("ret {0}, sell {1}, buy {2}, holding {3}", ret, sellPrice, buyPrice, holding));
    }
}
