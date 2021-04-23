using System.Collections.Generic;
using UnityEngine;

public class DefenseTower : IBuildingDefensing
{
    public GameObject startParticle, earningParticle;
    GameObject turret, cannonball;
    Vector3 defaultPos;
    Vector3 startPos;
    bool isStarted;
    public void Awake()
    {
        turret = this.transform.Find("Turret").gameObject;
        cannonball = turret.transform.Find("cannonball").gameObject;
        defaultPos = cannonball.transform.localPosition;

        isStarted= false;
        cannonball.SetActive(false); //이거때문에 파티클이 제때 안나타난다. 그래서 파티클을 같은레벨 자식으로 등록한다.
    }
    public override void Rotation(List<Transform> rots)
    {
        turret.transform.LookAt(rots[0]);
        //turret.transform.eulerAngles += new Vector3(0, -270, 0);
    }

    public override void Attack(List<Vector3> targets, float ratio)
    {
        if(targets.Count == 0)
            return;

        cannonball.SetActive(true);

        DisposeParticle();

        //first
        if(!isStarted)
        {
            startPos = cannonball.transform.position;
            isStarted = true;
            GameObject p = GameObject.Instantiate(startParticle, startPos, Quaternion.identity);
            p.name = "particle";
            p.transform.SetParent(this.transform);
        }
        cannonball.transform.position = Vector3.Lerp(startPos, targets[0], ratio);
        
    }
    public override void AttackEnd()
    {
        Debug.Log("finish");
        Vector3 pos = cannonball.transform.position;
        cannonball.transform.localPosition = defaultPos;

        GameObject p = GameObject.Instantiate(startParticle, pos, Quaternion.identity);
        p.name = "particle";
        p.transform.SetParent(this.transform);

        cannonball.SetActive(false);
        isStarted = false;
    }
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
