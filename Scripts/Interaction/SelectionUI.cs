using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionUI 
{
    public GameObject selectedObject;
    TAG selectedTag;

    public struct UI
    {
        public TAG tag;
        public GameObject top;
        public GameObject bottom;  
        public UI(TAG tag, GameObject top, GameObject bottom)
        {
            this.tag = tag;
            this.top = top;
            this.bottom = bottom;
        }
    }
    Dictionary<TAG, UI> uiInfo = new Dictionary<TAG, UI>();
    
    private static readonly Lazy<SelectionUI> hInstance = new Lazy<SelectionUI>(() => new SelectionUI());
    public static SelectionUI Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected SelectionUI()
    {
    }
    public void Init(List<UI> uiList)
    {
        for(int n = 0; n < uiList.Count; n++)
        {
            uiInfo[uiList[n].tag] = uiList[n];
        }
        Hide();
    }

    public void Activate(TAG tag, GameObject obj, string[] topTexts = null, string[] bottomTexts = null)
    {
        Hide();

        selectedTag = tag;
        selectedObject = obj;

        if(uiInfo.ContainsKey(selectedTag))
        {
            if(uiInfo[selectedTag].top != null)
                uiInfo[selectedTag].top.SetActive(true);
            if(uiInfo[selectedTag].bottom != null)
                uiInfo[selectedTag].bottom.SetActive(true);

            if(topTexts != null)
            {
                Text[] topTextArray = uiInfo[selectedTag].top.GetComponentsInChildren<Text>();
                for(int n = 0; n < topTexts.Length; n++)
                {
                    topTextArray[n].text = topTexts[n];
                }
            }

            if(bottomTexts != null)
            {
                Text[] bottomTextArray = uiInfo[selectedTag].bottom.GetComponentsInChildren<Text>();
                for(int n = 0; n < bottomTexts.Length; n++)
                {
                    bottomTextArray[n].text = bottomTexts[n];
                }
            }
        }
        Update();
    }
    public void Hide()
    {
        selectedObject = null;
        foreach(KeyValuePair<TAG, UI> kv in uiInfo)
        {
            if(kv.Value.top != null)
                kv.Value.top.SetActive(false);
            if(kv.Value.bottom != null)
                kv.Value.bottom.SetActive(false);
        }
    }
    public int GetSelectedMapId()
    {
        if(selectedObject == null)
            return -1;
        return Util.GetIntFromGameObjectName(selectedObject.name);
    }
    public void Update()
    {
        if(selectedObject != null)
        {
            if(uiInfo.ContainsKey(selectedTag))
            {
                Vector3 pos = selectedObject.transform.position;
                if(uiInfo[selectedTag].top != null)
                    uiInfo[selectedTag].top.transform.position = Camera.main.WorldToScreenPoint(pos + new Vector3(0, 0.5f, 0));
                if(uiInfo[selectedTag].bottom != null)
                    uiInfo[selectedTag].bottom.transform.position = Camera.main.WorldToScreenPoint(pos + new Vector3(0, -0.5f, 0));
            }
        }
    }
}