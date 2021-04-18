using UnityEngine;

public class Chick : IAnimation
{
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
}