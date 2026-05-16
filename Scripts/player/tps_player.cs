using System.Collections;
using UnityEngine;

public class npc_move_anim : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float runMultiplier = 2f;
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpHeight = 2f;
    public float jumpDuration = 0.5f;

    [Header("Shoot")]
    public float shootCooldown = 0.5f;

    [Header("Ground Check")]
    public float groundCheckDistance = 2f;
    public LayerMask groundMask;

    [Header("Height Offset")]
    public float yOffset = 0f;
    public float scrollSpeed = 2f;
    public float minYOffset = -5f;
    public float maxYOffset = 5f;

    private bool isJumping = false;
    private bool isShooting = false;

    private Animator animator;
    private Vector3 jumpDirection;

    private float speedVelocity;
    private float currentSpeed;

    private float moveXVelocity;
    private float moveZVelocity;

    // NPC Health
    private NPCHealth npcHealth;

    void Start()
    {
        animator = GetComponent<Animator>();
        npcHealth = GetComponent<NPCHealth>();
    }

    void Update()
    {
        // Ha meghalt, ne mozogjon
        if (npcHealth != null && npcHealth.currentHealth <= 0)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveZ", 0f);
            return;
        }

        HandleMovement();

        // Jump
        if (Input.GetKey(KeyCode.Space) && !isJumping)
        {
            StartJump();
        }

        // Shoot
        if (Input.GetKeyDown(KeyCode.E) && !isShooting)
        {
            StartCoroutine(Shoot());
        }
    }

    void StartJump()
    {
        float moveX = 0f;
        float moveZ = 0f;

        // Strafe input
        if (Input.GetKey(KeyCode.A))
            moveX = -1f;

        if (Input.GetKey(KeyCode.D))
            moveX = 1f;

        // Forward / backward
        if (Input.GetKey(KeyCode.W))
            moveZ = 1f;

        if (Input.GetKey(KeyCode.S))
            moveZ = -1f;

        // Hátramenetnél ne lehessen oldalra menni
        if (moveZ < 0)
            moveX = 0f;

        jumpDirection =
            (transform.forward * moveZ +
             transform.right * moveX).normalized;

        jumpDirection.y = 0f;

        StartCoroutine(Jump());
    }

    bool GetGroundPoint(out Vector3 groundPoint)
    {
        Vector3 origin = transform.position + Vector3.up;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 10f, groundMask))
        {
            groundPoint = hit.point;
            return true;
        }

        groundPoint = transform.position;
        return false;
    }

    void HandleMovement()
    {
        float moveX = 0f;
        float moveZ = 0f;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // WASD input
        if (Input.GetKey(KeyCode.W))
            moveZ = 1f;

        if (Input.GetKey(KeyCode.S))
            moveZ = -1f;

        if (Input.GetKey(KeyCode.A))
            moveX = -1f;

        if (Input.GetKey(KeyCode.D))
            moveX = 1f;



        // Hátramenetnél mindig egyenes hátra
        if (moveZ < 0)
            moveX = 0f;

        bool isMoving = moveX != 0 || moveZ != 0;

        float targetSpeed = 0f;

        if (isMoving)
            targetSpeed = isRunning ? 2f : 1f;

        // Smooth speed
        currentSpeed = Mathf.SmoothDamp(
            currentSpeed,
            targetSpeed,
            ref speedVelocity,
            0.1f
        );

        animator.SetFloat("Speed", currentSpeed);

        // Smooth animator values
        float smoothX = Mathf.SmoothDamp(
            animator.GetFloat("MoveX"),
            moveX,
            ref moveXVelocity,
            0.05f
        );

        float smoothZ = Mathf.SmoothDamp(
            animator.GetFloat("MoveZ"),
            moveZ,
            ref moveZVelocity,
            0.05f
        );

        animator.SetFloat("MoveX", smoothX);
        animator.SetFloat("MoveZ", smoothZ);

        animator.speed = 1f;

        if (isJumping)
            return;

        if (!isMoving)
            return;

        float currentMoveSpeed =
            isRunning
            ? moveSpeed * runMultiplier
            : moveSpeed;

        // Movement direction
        Vector3 moveDir =
            transform.forward * moveZ +
            transform.right * moveX;

        if (moveDir != Vector3.zero)
        {
            moveDir.y = 0f;
            moveDir.Normalize();

            transform.position +=
                moveDir * currentMoveSpeed * Time.deltaTime;

            // Csak előremenetnél forduljon
            if (moveZ > 0)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(
                        new Vector3(moveDir.x, 0f, moveDir.z)
                    );

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    IEnumerator Jump()
    {
        isJumping = true;

        animator.SetTrigger("Jump");

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 startPos;

        if (!GetGroundPoint(out startPos))
        {
            startPos = transform.position;
        }

        float speed =
            isRunning
            ? moveSpeed * runMultiplier
            : moveSpeed;

        float time = 0f;

        while (time < jumpDuration)
        {
            // Ha meghalt ugrás közben
            if (npcHealth != null && npcHealth.currentHealth <= 0)
            {
                isJumping = false;
                yield break;
            }

            float t = time / jumpDuration;

            float height =
                Mathf.Sin(t * Mathf.PI) * jumpHeight;

            Vector3 horizontal =
                jumpDirection * speed * 0.6f * time;

            Vector3 baseGround;

            GetGroundPoint(out baseGround);

            transform.position = new Vector3(
                startPos.x + horizontal.x,
                baseGround.y + height + yOffset,
                startPos.z + horizontal.z
            );

            time += Time.deltaTime;
            yield return null;
        }

        if (GetGroundPoint(out Vector3 finalGround))
        {
            transform.position = new Vector3(
                transform.position.x,
                finalGround.y + yOffset,
                transform.position.z
            );
        }

        isJumping = false;
    }

    IEnumerator Shoot()
    {
        isShooting = true;

        animator.SetTrigger("Shoot");

        yield return new WaitForSeconds(shootCooldown);

        isShooting = false;
    }
}