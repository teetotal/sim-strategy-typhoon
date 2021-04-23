using UnityEngine;

public class Chick : IActor
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
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        int n = UnityEngine.Random.Range(0, 3);
        if(n == 0)
        {
            //animator.SetBool("Jump", true);
            animator.SetBool("Eat", false);
        }
        else
        {
            //animator.SetBool("Jump", false);
            animator.SetBool("Eat", true);
        }
        
    }

    public override void SetMoving()
    {
        animator.SetBool("Jump", false);
        animator.SetBool("Eat", false);

        int n = UnityEngine.Random.Range(0, 3);
        if(n == 0)
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Run", true);
        }
        else
        {
            animator.SetBool("Walk", true);
            animator.SetBool("Run", false);
        }
    }

    public override void SetDie()
    {
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        animator.SetBool("Attack", false);
        animator.SetBool("Jump", false);
        animator.SetBool("Eat", false);

        animator.SetBool("Die", true);
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