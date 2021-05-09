using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadMap : MonoBehaviour
{
    public Transform canvas;
    string filePath;
    // Start is called before the first frame update
    void Start()
    {
        LoaderPerspective.Instance.SetUI(Camera.main, ref canvas, OnClickButton);
        if(!LoaderPerspective.Instance.LoadJsonFile("load_map"))
        {
            Debug.LogError("ui.json loading failure");
            return;
        } 

        LoaderPerspective.Instance.AddComponents(null, OnCreatePost);
        SetFileList();
    }

    void OnCreatePost(GameObject obj, string layerName)
    {
    }
    void SetFileList()
    {
        GameObject obj = GameObject.Find("files");

        GameObject content = obj.transform.Find("Viewport").transform.Find("Content").gameObject;
        for(int n = 0; n < content.transform.childCount; n++)
        {
            GameObject.Destroy(content.transform.GetChild(n).gameObject);
        }


        List<GameObject> items = GetFiles();
        LoaderPerspective.Instance.CreateScrollViewItems(items
                                                        , new Vector2(15, 15)
                                                        , new Vector2(10, 10)
                                                        , OnClickButton
                                                        , obj
                                                        , 1);
    }

    List<GameObject> GetFiles()
    {
        List<GameObject> list = new List<GameObject>();
        string[] files = Directory.GetFiles(Application.persistentDataPath);

        for(int n = 0; n < files.Length; n++)
        {
            string[] arr = files[n].Split('/');
            string file = arr[arr.Length-1];

            GameObject obj = Resources.Load<GameObject>("button_normal");
            obj.GetComponentInChildren<Text>().text = file;
            obj.name = string.Format("{0}", file);
            list.Add(Instantiate(obj));
        }

        return list;
    }

    void Clear()
    {
        GameObject.Find("selectedFile").GetComponentInChildren<Text>().text = "";
        GameObject.Find("info").GetComponent<Text>().text = "";
    }

    void OnClickButton(GameObject obj)
    {
        switch(obj.name)
        {
            case "new":
            SceneManager.LoadScene("MapMaker");
            break;
            case "load":
            break;
            case "delete":
            {
                string p = string.Format("{0}/{1}", Application.persistentDataPath, filePath);
                File.Delete(p);
                SetFileList();
                Clear();
            }
            break;
            default:
                filePath = Util.GetObjectName(obj);
                string path = string.Format("{0}/{1}", Application.persistentDataPath, filePath);
                GameObject.Find("selectedFile").GetComponentInChildren<Text>().text = filePath;

                FileInfo info = new FileInfo(path);
                GameObject.Find("info").GetComponent<Text>().text = 
                    string.Format("{0} bytes\n{1}\n{2}\n{3}", 
                    Util.GetCurrencyString(info.Length), 
                    info.CreationTime, 
                    info.LastWriteTime, 
                    info.LastAccessTime);
            break;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
