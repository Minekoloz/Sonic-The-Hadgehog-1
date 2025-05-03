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
    private bool isDucking;
    private bool isLookingUp;

    public CameraController cameraController;

    public CapsuleCollider2D capsuleCollider;
    public CircleCollider2D circleCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentSpeed = moveSpeed;

        cameraController ??= Camera.main.GetComponent<CameraController>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        circleCollider = GetComponent<CircleCollider2D>();

        capsuleCollider.enabled = true;
        circleCollider.enabled = false;
    }

    void Update()
    {
        float move = Input.GetAxis("Horizontal");
        HandleDuckingAndLookingUp(move);
        HandleMovement(move);
        HandleJumping();
        HandleAnimations(move);
        FlipSprite(move);
    }

    void HandleDuckingAndLookingUp(float move)
    {
        bool isDuckingOrLookingUp = (Input.GetKey(KeyCode.S) && isGrounded && Mathf.Abs(move) < 0.1f) || 
                                      (Input.GetKey(KeyCode.W) && isGrounded && Mathf.Abs(move) < 0.1f);

        if (isDuckingOrLookingUp)
        {
            if (Input.GetKey(KeyCode.S))
            {
                isDucking = true;
                isLookingUp = false;
                SetAnimation("Sonic_Ducking");
                cameraController?.SetVerticalOffset(-0.5f);
                SetColliderState(false, true);
            }
            else if (Input.GetKey(KeyCode.W))
            {
                isLookingUp = true;
                isDucking = false;
                SetAnimation("Sonic_Looking_Up");
                cameraController?.SetVerticalOffset(0.5f);
                SetColliderState(true, false);
            }

            rb.velocity = new Vector2(0f, rb.velocity.y);  // Freeze horizontal movement
        }
        else
        {
            ResetDuckingAndLookingUp();
        }
    }

    void ResetDuckingAndLookingUp()
    {
        isDucking = false;
        isLookingUp = false;
        cameraController?.ResetVerticalOffset();
        SetColliderState(true, false);
    }

    void SetColliderState(bool capsuleEnabled, bool circleEnabled)
    {
        if (capsuleCollider != null) capsuleCollider.enabled = capsuleEnabled;
        if (circleCollider != null) circleCollider.enabled = circleEnabled;
    }

    void HandleMovement(float move)
    {
        if (!isDucking && !isLookingUp)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, move != 0 ? maxSpeed : moveSpeed, 
                                             (move != 0 ? acceleration : deceleration) * Time.deltaTime);
            rb.velocity = new Vector2(currentSpeed * move, rb.velocity.y);
        }
    }

    void HandleJumping()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && !isDucking && !isLookingUp)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            isJumping = true;
            SetAnimation("Sonic_Spinning");
        }
    }

    void HandleAnimations(float move)
    {
        if (isJumping)
        {
            SetAnimation("Sonic_Spinning");
        }
        else if (isGrounded && !isDucking && !isLookingUp)
        {
            SetAnimation(Mathf.Abs(move) > 0.01f ? (Mathf.Abs(rb.velocity.x) > 8f ? "Sonic_Run_Fast" : "Sonic_Running") : "Sonic_Idle");
        }
    }

    void FlipSprite(float move)
    {
        if (move > 0) spriteRenderer.flipX = false;
        else if (move < 0) spriteRenderer.flipX = true;
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
