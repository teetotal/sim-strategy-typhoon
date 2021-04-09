using UnityEngine;

public class Cat01 : IAnimation
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
        animator.SetBool("Idle", true);
        animator.SetBool("Run", false);
    }

    public override void SetMoving()
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Run", true);
    }
}