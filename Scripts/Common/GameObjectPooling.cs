using System;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPooling
{
    private Dictionary<string, Stack<GameObject> > pooling;
    private Dictionary<string, int> allocCount;
    private static readonly Lazy<GameObjectPooling> hInstance = new Lazy<GameObjectPooling>(() => new GameObjectPooling());
    
    public static GameObjectPooling Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected GameObjectPooling() { }
    //scene이 이동하면 gameobject들은 모두 날라간다. 초기화로 셋해줘야 함
    //scene에 종속적일 수 밖에 없고, 하나의 scene을 pooling할 수 밖에 없다.
    public void Reset()
    {
        pooling = new Dictionary<string, Stack<GameObject>>();
        allocCount = new Dictionary<string, int>();
    }
    public GameObject Get(string prefab)
    {
        return Get(prefab, Vector3.zero, Quaternion.identity);
    }
    public GameObject Get(string prefab, Vector3 position, Quaternion quaternion)
    {
        GameObject gameObject;
        if(GetCount(prefab) > 0)
        {
            gameObject = pooling[prefab].Pop();
            gameObject.SetActive(true);
        }
        else
        {
            gameObject = Alloc(prefab);
            Debug.Log(string.Format("Allocation {0} {1}", prefab, allocCount[prefab]));
        }
            
        gameObject.transform.position = position;
        gameObject.transform.rotation = quaternion;

        return gameObject;
    }

    private int GetCount(string prefab)
    {
        if(!pooling.ContainsKey(prefab))
        {
            pooling[prefab] = new Stack<GameObject>();
        }
        return pooling[prefab].Count;
    }

    private GameObject Alloc(string prefab)
    {
        GameObject obj = Resources.Load<GameObject>(prefab);
        obj = GameObject.Instantiate(obj);

        if(!allocCount.ContainsKey(prefab))
            allocCount[prefab] = 1;
        else
            allocCount[prefab]++;

        return obj;
    }

    public void Release(string prefab, GameObject gameObject)
    {
        gameObject.name = prefab + "_pool";
        gameObject.SetActive(false);
        pooling[prefab].Push(gameObject);
    }

}