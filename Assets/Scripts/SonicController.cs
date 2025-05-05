using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonicController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float maxSpeed = 12f;
    public float acceleration = 2f;
    public float deceleration = 5f;
    public float jumpForce = 9f;

    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public CapsuleCollider2D capsuleCollider;
    public CircleCollider2D circleCollider;
    public CameraController cameraController;

    [Header("State Flags")]
    public bool isGrounded;
    public bool isDead;
    public bool isAFK;

    [Header("Idle Animation")]
    public float idleTimeThreshold = 10f;

    private float currentSpeed;
    private float idleTimer;
    private bool isJumping;
    private bool isDucking;
    private bool isLookingUp;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        cameraController ??= Camera.main.GetComponent<CameraController>();

        capsuleCollider.enabled = true;
        circleCollider.enabled = false;
        currentSpeed = moveSpeed;
    }

    void Update()
    {
        if (isDead) return;

        float move = Input.GetAxis("Horizontal");

        CheckForHazardsBelow();
        bool isMoving = Mathf.Abs(move) > Mathf.Epsilon || Mathf.Abs(rb.velocity.x) > 0.1f;

        UpdateAFKState(isMoving);

        HandleLookAndDuck(move);

        if (!isDucking && !isLookingUp && !isAFK)
        {
            HandleMovement(move);
            HandleJumping();
        }

        if (!isAFK)
            HandleAnimations(move);

        FlipSprite(move);
    }

    private void UpdateAFKState(bool isMoving)
    {
        if (!isMoving)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTimeThreshold && !isAFK)
            {
                isAFK = true;
                SetAnimation("Sonic_Waiting");
            }
        }
        else
        {
            idleTimer = 0f;
            if (isAFK) isAFK = false;
        }
    }

private void CheckForHazardsBelow()
{
    Vector2 origin = new Vector2(capsuleCollider.bounds.center.x, capsuleCollider.bounds.min.y - 0.05f);
    float rayLength = 0.2f;

    RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength);
    if (hit.collider != null &&
        (hit.collider.CompareTag("SonicSpikes") || hit.collider.CompareTag("SonicPit")))
    {
        Die();
    }
}

    private void HandleLookAndDuck(float move)
    {
        bool noInput = Mathf.Abs(move) < Mathf.Epsilon;
        if (!isGrounded || !noInput) return;

        if (Input.GetKey(KeyCode.S))
        {
            SetState(true, false, "Sonic_Ducking", -0.5f, false, true);
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        else if (Input.GetKey(KeyCode.W))
        {
            SetState(false, true, "Sonic_Looking_Up", 0.5f, true, false);
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        else if (isDucking || isLookingUp)
        {
            SetState(false, false, null, 0f, true, false);
        }
    }

    private void SetState(bool duck, bool lookUp, string anim, float camOffset, bool capsule, bool circle)
    {
        isDucking = duck;
        isLookingUp = lookUp;

        if (!string.IsNullOrEmpty(anim))
            SetAnimation(anim);

        if (cameraController != null)
        {
            if (Mathf.Abs(camOffset) > Mathf.Epsilon)
                cameraController.SetVerticalOffset(camOffset);
            else
                cameraController.ResetVerticalOffset();
        }

        capsuleCollider.enabled = capsule;
        circleCollider.enabled = circle;
    }

    private void HandleMovement(float move)
    {
        bool hasInput = Mathf.Abs(move) > Mathf.Epsilon;
        float targetSpeed = hasInput ? maxSpeed : moveSpeed;
        float accel = hasInput ? acceleration : deceleration;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accel * Time.deltaTime);
        rb.velocity = new Vector2(currentSpeed * move, rb.velocity.y);
    }

    private void HandleJumping()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            isJumping = true;
            SetAnimation("Sonic_Spinning");
        }
    }

    private void HandleAnimations(float move)
    {
        if (isJumping)
        {
            SetAnimation("Sonic_Spinning");
        }
        else if (isGrounded && !isDucking && !isLookingUp)
        {
            string anim = Mathf.Abs(move) > Mathf.Epsilon
                ? (Mathf.Abs(rb.velocity.x) > 8f ? "Sonic_Run_Fast" : "Sonic_Running")
                : "Sonic_Idle";

            SetAnimation(anim);
        }
    }

    private void FlipSprite(float move)
    {
        if (Mathf.Abs(move) > Mathf.Epsilon)
            spriteRenderer.flipX = move < 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        CheckForHazardsBelow();

        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
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
            animator.Play(animationName);
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        SetAnimation("Sonic_Dead");

        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * 150f, ForceMode2D.Impulse);
        rb.gravityScale = 3f;

        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        capsuleCollider.enabled = false;
        circleCollider.enabled = false;

        if (cameraController != null)
            cameraController.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (capsuleCollider == null) return;

        Gizmos.color = Color.red;
        Vector2 origin = new Vector2(capsuleCollider.bounds.center.x, capsuleCollider.bounds.min.y - 0.05f);
        Gizmos.DrawLine(origin, origin + Vector2.down * 0.1f);
    }
}
