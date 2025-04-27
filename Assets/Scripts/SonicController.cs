using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonicController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float maxSpeed = 12f;
    public float acceleration = 2f;
    public float deceleration = 5f;
    public float jumpForce = 9f;

    public Rigidbody2D rb;
    public bool isGrounded;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private bool isJumping;
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentSpeed = moveSpeed;
    }

    void Update()
    {
        float move = Input.GetAxis("Horizontal");

        if (move != 0)
        {
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        }
        else
        {
            currentSpeed -= deceleration * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, moveSpeed); 
        }

        rb.velocity = new Vector2(currentSpeed * move, rb.velocity.y);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            isGrounded = false;
            isJumping = true;
            SetAnimation("Sonic_Spinning");
        }

        if (isJumping)
        {
            SetAnimation("Sonic_Spinning");
        }
        else if (isGrounded)
        {
            if (Mathf.Abs(move) > 0.01f)
            {
                if (Mathf.Abs(rb.velocity.x) > 8f) 
                    SetAnimation("Sonic_Run_Fast");
                else
                    SetAnimation("Sonic_Running");
            }
            else
            {
                SetAnimation("Sonic_Idle");
            }
        }

        if (move > 0)
            spriteRenderer.flipX = false;
        else if (move < 0)
            spriteRenderer.flipX = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
            isJumping = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
{
    if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
    {
        isGrounded = false;
        isJumping = true;
    }
}

    private void SetAnimation(string animationName)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            animator.Play(animationName);
        }
    }
}
