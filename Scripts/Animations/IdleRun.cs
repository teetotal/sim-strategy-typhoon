using UnityEngine;

public class IdleRun : IAnimation
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
}