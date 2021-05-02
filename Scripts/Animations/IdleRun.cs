using UnityEngine;

public class IdleRun : IActor
{
    public GameObject earningParticle;
    Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
    }

    public override void SetIdle()
    {
        if(animator.GetBool("Idle") == false)
            animator.SetBool("Idle", true);

        animator.SetBool("Run", false);
        animator.SetBool("Die", false);
    }

    public override void SetMoving()
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Die", false);

        if(!animator.GetBool("Run"))
            animator.SetBool("Run", true);
    }
    public override void SetDie()
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Run", false);

        animator.SetBool("Die", true);
    }
    public override void Earning(bool success)
    {
        DisposeParticle();
        if(success)
        {
            Transform earningPoint = this.transform.Find("earningPoint");
            if(earningPoint == null)
                return;
                
            Vector3 pos = earningPoint.position;
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