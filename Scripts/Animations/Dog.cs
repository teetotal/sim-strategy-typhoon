using UnityEngine;

public class Dog : IAttacking
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
        animator.SetBool("Attack", false);
        animator.SetBool("Attack2", false);

        animator.SetBool("Idle", true);
        
    }

    public override void SetMoving()
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Attack", false);
        animator.SetBool("Attack2", false);

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

    public override void SetAttack()
    {
        if(UnityEngine.Random.Range(0, 3) == 0)
        {
            animator.SetBool("Attack", false);
            animator.SetBool("Attack2", true);
        }
        else
        {
            animator.SetBool("Attack", true);
            animator.SetBool("Attack2", false);
        }
        
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        animator.SetBool("Idle", false);
    }
}