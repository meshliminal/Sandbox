using System.Collections;
using UnityEngine;

public class tps_player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float runMultiplier = 2f;

    [Header("Rotation")]

    // Régi TPS forgás mozgás irányába
    public bool rotateToMovement = true;

    // ÚJ TPS forgás kamera irányába
    public bool rotateToCamera = true;

    // Movement rotation smooth
    public float rotationSpeed = 10f;

    // Camera rotation smooth
    public float cameraTurnSmooth = 4f;

    [Header("Turn Animation")]
    public float turnAnimationSmooth = 8f;

    public float gravityForce = 25f;
    public float airControl = 0.65f;
    public float movementSmoothTime = 0.08f;

    [Header("Jump")]
    public float jumpHeight = 2.2f;
    public float jumpDuration = 0.65f;
    public float jumpForwardMultiplier = 2f;
    public float jumpMomentumBlend = 6f;
    public float landingSnapForce = 25f;

    [Header("Shoot")]
    public float shootCooldown = 0.5f;

    [Header("Ground Check")]
    public float groundCheckDistance = 1.2f;
    public LayerMask groundMask;

    [Header("Height Offset")]
    public float yOffset = 0f;

    private bool isJumping = false;
    private bool isShooting = false;

    private Animator animator;

    private float speedVelocity;
    private float currentSpeed;

    private float moveXVelocity;
    private float moveZVelocity;

    private float verticalVelocity;

    private Vector3 currentMoveDirection;
    private Vector3 moveDirectionVelocity;

    private Vector3 jumpHorizontalVelocity;

    // Kamera yaw
    private float cameraYaw;

    // TURN SYSTEM
    private float lastCameraYaw;
    private float currentTurnValue;
    private float turnVelocity;

    // NPC Health
    private NPCHealth npcHealth;

    // Rigidbody
    private Rigidbody rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        npcHealth = GetComponent<NPCHealth>();
        rb = GetComponent<Rigidbody>();

        lastCameraYaw = cameraYaw;
    }

    void Update()
    {
        // Dead
        if (npcHealth != null &&
            npcHealth.currentHealth <= 0)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveZ", 0f);
            animator.SetFloat("Turn", 0f);

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            return;
        }

        HandleMovement();

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) &&
            !isJumping &&
            IsGrounded())
        {
            StartJump();
        }

        // Shoot
        if (Input.GetKeyDown(KeyCode.E) &&
            !isShooting)
        {
            StartCoroutine(Shoot());
        }
    }

    public void SetCameraYaw(float yaw)
    {
        cameraYaw = yaw;
    }

    bool IsGrounded()
    {
        Vector3 origin =
            transform.position + Vector3.up * 0.25f;

        return Physics.Raycast(
            origin,
            Vector3.down,
            groundCheckDistance,
            groundMask
        );
    }

    Vector3 GetInputDirection()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W))
            moveZ += 1f;

        if (Input.GetKey(KeyCode.S))
            moveZ -= 1f;

        if (Input.GetKey(KeyCode.A))
            moveX -= 1f;

        if (Input.GetKey(KeyCode.D))
            moveX += 1f;

        Vector3 camForward =
            Quaternion.Euler(0f, cameraYaw, 0f) *
            Vector3.forward;

        Vector3 camRight =
            Quaternion.Euler(0f, cameraYaw, 0f) *
            Vector3.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        // S gomb → forduljon meg
        if (moveZ < 0f)
        {
            camForward *= -1f;

            // ne lehessen hátra strafelni
            moveX = 0f;

            // mindig forward motion legyen
            moveZ = 1f;
        }

        Vector3 moveDir =
            camForward * moveZ +
            camRight * moveX;

        return moveDir.normalized;
    }

    void StartJump()
    {
        isJumping = true;

        animator.SetTrigger("Jump");

        bool isRunning =
            Input.GetKey(KeyCode.LeftShift);

        float currentMoveSpeed =
            isRunning
            ? moveSpeed * runMultiplier
            : moveSpeed;

        Vector3 inputDir = GetInputDirection();

        jumpHorizontalVelocity =
            inputDir *
            currentMoveSpeed *
            jumpForwardMultiplier;

        verticalVelocity =
            Mathf.Sqrt(
                jumpHeight *
                gravityForce *
                2f
            );

        StartCoroutine(JumpCoroutine());
    }

    void HandleMovement()
    {
        float moveX = 0f;
        float moveZ = 0f;

        bool isRunning =
            Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKey(KeyCode.W))
            moveZ += 1f;

        if (Input.GetKey(KeyCode.S))
            moveZ -= 1f;

        if (Input.GetKey(KeyCode.A))
            moveX -= 1f;

        if (Input.GetKey(KeyCode.D))
            moveX += 1f;

        // hátra strafe tiltás
        if (moveZ < 0)
            moveX = 0f;

        bool isMoving =
            moveX != 0 || moveZ != 0;

        float targetSpeed = 0f;

        if (isMoving)
        {
            targetSpeed =
                isRunning ? 2f : 1f;
        }

        currentSpeed = Mathf.SmoothDamp(
            currentSpeed,
            targetSpeed,
            ref speedVelocity,
            0.1f
        );

        animator.SetFloat("Speed", currentSpeed);

        float animMoveZ = moveZ;

        // S gombnál is forward anim legyen
        if (moveZ < 0f)
        {
            animMoveZ = 1f;
        }

        float smoothX = Mathf.SmoothDamp(
            animator.GetFloat("MoveX"),
            moveX,
            ref moveXVelocity,
            0.05f
        );

        float smoothZ = Mathf.SmoothDamp(
            animator.GetFloat("MoveZ"),
            animMoveZ,
            ref moveZVelocity,
            0.05f
        );

        animator.SetFloat("MoveX", smoothX);
        animator.SetFloat("MoveZ", smoothZ);

        Vector3 targetMoveDir =
            GetInputDirection();

        currentMoveDirection =
            Vector3.SmoothDamp(
                currentMoveDirection,
                targetMoveDir,
                ref moveDirectionVelocity,
                movementSmoothTime
            );

        float currentMoveSpeed =
            isRunning
            ? moveSpeed * runMultiplier
            : moveSpeed;

        // GRAVITY
        if (!IsGrounded() || verticalVelocity > 0f)
        {
            verticalVelocity -=
                gravityForce * Time.deltaTime;
        }
        else if (!isJumping)
        {
            verticalVelocity = -2f;
        }

        Vector3 horizontalVelocity;

        // AIR CONTROL TPS STYLE
        if (isJumping)
        {
            Vector3 desiredAirVelocity =
                currentMoveDirection *
                currentMoveSpeed *
                jumpForwardMultiplier;

            jumpHorizontalVelocity =
                Vector3.Lerp(
                    jumpHorizontalVelocity,
                    desiredAirVelocity,
                    airControl * Time.deltaTime * 10f
                );

            horizontalVelocity =
                jumpHorizontalVelocity;
        }
        else
        {
            horizontalVelocity =
                currentMoveDirection *
                currentMoveSpeed;
        }

        Vector3 finalVelocity =
            horizontalVelocity;

        finalVelocity.y = verticalVelocity;

        transform.position +=
            finalVelocity * Time.deltaTime;

        // TALAJ SNAP
        if (!isJumping &&
            verticalVelocity <= 0f)
        {
            if (Physics.Raycast(
                transform.position + Vector3.up,
                Vector3.down,
                out RaycastHit snapHit,
                3f,
                groundMask))
            {
                float targetY =
                    snapHit.point.y + yOffset;

                transform.position =
                    Vector3.Lerp(
                        transform.position,
                        new Vector3(
                            transform.position.x,
                            targetY,
                            transform.position.z
                        ),
                        landingSnapForce *
                        Time.deltaTime
                    );
            }
        }

        // Rigidbody stabil
        if (rb != null)
        {
            rb.linearVelocity =
                new Vector3(
                    0f,
                    rb.linearVelocity.y,
                    0f
                );

            rb.angularVelocity =
                Vector3.zero;
        }

        // RÉGI TPS forgás mozgási irány alapján
        if (rotateToMovement)
        {
            if (currentMoveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(
                        currentMoveDirection
                    );

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
            }
        }

        // ÚJ TPS forgás kamera irányába
        if (rotateToCamera)
        {
            float targetYaw = cameraYaw;

            // S gombnál forduljon meg
            if (Input.GetKey(KeyCode.S))
            {
                targetYaw += 180f;
            }

            Quaternion cameraRotation =
                Quaternion.Euler(
                    0f,
                    targetYaw,
                    0f
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    cameraRotation,
                    cameraTurnSmooth * Time.deltaTime
                );
        }

        // TURN ANIMATION

        float yawDelta =
            Mathf.DeltaAngle(
                lastCameraYaw,
                cameraYaw
            );

        float targetTurn = 0f;

        // Jobbra
        if (yawDelta > 0.1f)
        {
            targetTurn = 1f;
        }
        // Balra
        else if (yawDelta < -0.1f)
        {
            targetTurn = -1f;
        }

        // Mozgás közben ne turn anim legyen
        if (isMoving)
        {
            targetTurn = 0f;
        }

        currentTurnValue =
            Mathf.SmoothDamp(
                currentTurnValue,
                targetTurn,
                ref turnVelocity,
                1f / turnAnimationSmooth
            );

        animator.SetFloat(
            "Turn",
            currentTurnValue
        );

        lastCameraYaw = cameraYaw;
    }

    IEnumerator JumpCoroutine()
    {
        while (true)
        {
            if (npcHealth != null &&
                npcHealth.currentHealth <= 0)
            {
                isJumping = false;
                yield break;
            }

            // Land detection
            if (verticalVelocity <= 0f &&
                IsGrounded())
            {
                break;
            }

            yield return null;
        }

        // Ground snap
        if (Physics.Raycast(
            transform.position + Vector3.up * 2f,
            Vector3.down,
            out RaycastHit hit,
            5f,
            groundMask))
        {
            transform.position =
                new Vector3(
                    transform.position.x,
                    hit.point.y + yOffset,
                    transform.position.z
                );
        }

        verticalVelocity = -2f;

        jumpHorizontalVelocity = Vector3.zero;

        isJumping = false;
    }

    IEnumerator Shoot()
    {
        isShooting = true;

        animator.SetTrigger("Shoot");

        yield return new WaitForSeconds(
            shootCooldown
        );

        isShooting = false;
    }
}