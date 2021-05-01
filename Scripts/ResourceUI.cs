using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceUI : MonoBehaviour
{
    Text[] txts;
    Image[] progresses;
    // Start is called before the first frame update
    void Awake()
    {
        txts = new Text[3]
        {
            GameObject.Find("txt1").GetComponent<Text>(),
            GameObject.Find("txt2").GetComponent<Text>(),
            GameObject.Find("txt3").GetComponent<Text>()
        };

        progresses = new Image[2]
        {
            GameObject.Find("progress2").GetComponent<Image>(),
            GameObject.Find("progress3").GetComponent<Image>()
        };

        for(int n=0; n < txts.Length; n++)
        {
            txts[n].text = "0";
        }
    }

    public void SetValues(int[] v, float[] r)
    {
        for(int n=0; n < txts.Length; n++)
        {
            txts[n].text = string.Format("{0:n0}", v[n]);
        }

        for(int n=0; n < progresses.Length; n++)
        {
            progresses[n].fillAmount = r[n];
        }
    }

   
}
