using System.Collections.Generic;
using UnityEngine;

public class EarningHouse : IBuilding
{
    public GameObject earningParticle;
    public override void Earning(bool success)
    {
        DisposeParticle();
        if(success)
        {
            Vector3 pos = this.transform.Find("earningPoint").position;
            GameObject p = GameObject.Instantiate(earningParticle, pos, Quaternion.identity);
            p.name = "particle";
            p.transform.SetParent(this.transform);
        }
    }

    void DisposeParticle()
    {
        if(this.transform.childCount > 0)
        {
            for(int n = 0; n < this.transform.childCount; n++)
            {
                if(this.transform.GetChild(n).name == "particle")
                    GameObject.Destroy(this.transform.GetChild(n).gameObject);
            }
        }
    }
}