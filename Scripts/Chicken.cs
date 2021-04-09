using UnityEngine;

public class Chicken : IAnimation
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
            animator.SetBool("Turn Head", true);
            animator.SetBool("Eat", false);
        }
        else
        {
            animator.SetBool("Turn Head", false);
            animator.SetBool("Eat", true);
        }
        
    }

    public override void SetMoving()
    {
        animator.SetBool("Turn Head", false);
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
}