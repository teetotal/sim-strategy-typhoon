using System.Collections.Generic;
using UnityEngine;

public class DefenseTower : IBuildingAttack
{
    public GameObject startParticle;
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
        cannonball.SetActive(false);
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

        if(cannonball.transform.childCount > 0)
        {
            for(int n = 0; n < cannonball.transform.childCount; n++)
            {
                GameObject.Destroy(cannonball.transform.GetChild(n).gameObject);
            }
        }
        //first
        if(!isStarted)
        {
            startPos = cannonball.transform.position;
            isStarted = true;
            GameObject p = GameObject.Instantiate(startParticle, startPos, Quaternion.identity);
            p.transform.SetParent(cannonball.transform);
        }
        cannonball.transform.position = Vector3.Lerp(startPos, targets[0], ratio);
        
    }
    public override void AttackEnd()
    {
        Debug.Log("finish");
        //turret.transform.rotation = defaultRoation;
        cannonball.transform.localPosition = defaultPos;
        
        cannonball.SetActive(false);
        isStarted = false;
    }
}
