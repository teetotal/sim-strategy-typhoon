using UnityEngine;

public class DefenseTower : IBuildingAttack
{
    public GameObject startParticle;
    GameObject turret, cannonball;
    Vector3 defaultPos;
    Quaternion defaultRoation;
    Vector3 startPos;
    bool isStarted;
    public void Awake()
    {
        turret = this.transform.Find("Turret").gameObject;
        cannonball = turret.transform.Find("cannonball").gameObject;
        defaultPos = cannonball.transform.position;
        defaultRoation = turret.transform.rotation;

        isStarted= false;
        cannonball.SetActive(false);
    }
    public override void Rotation(Quaternion rot)
    {
        turret.transform.rotation = rot;
    }

    public override void Attack(Vector3 target, float ratio)
    {
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
        cannonball.transform.position = Vector3.Lerp(startPos, target, ratio);
        
    }
    public override void AttackEnd()
    {
        Debug.Log("finish");
        turret.transform.rotation = defaultRoation;
        cannonball.transform.position = defaultPos;
        
        cannonball.SetActive(false);
        isStarted = false;
    }
}
