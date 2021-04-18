using UnityEngine;

public class Robot : IAnimation
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

        animator.SetBool("Idle", true);
        
    }

    public override void SetMoving()
    {
        animator.SetBool("Idle", false);

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
        animator.SetBool("Idle", false);
        
        animator.SetBool("Die", true);
    }
}