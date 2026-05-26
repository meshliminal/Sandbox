using System.Collections;
using UnityEngine;

namespace sandbox
{
public class tps_player : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Weapon")]
    public Weapon_Glock weapon;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float runMultiplier = 2f;

    [Header("Rotation")]
    public bool rotateToMovement = true;

    public float rotationSpeed = 10f;
    public float cameraTurnSmooth = 4f;

    [Header("Aim")]
    public bool isAiming = false;

    [Header("Turn Animation")]
    public float turnAnimationSmooth = 8f;

    public float gravityForce = 25f;
    public float airControl = 0.65f;
    public float movementSmoothTime = 0.08f;

    [Header("Jump")]
    public float jumpHeight = 2.2f;
    public float jumpForwardMultiplier = 2f;
    public float landingSnapForce = 25f;

    [Header("Shoot")]
    public float shootCooldown = 0.5f;

    [Header("Ground Check")]
    public float groundCheckDistance = 1.2f;
    public LayerMask groundMask;

    [Header("Height Offset")]
    public float yOffset = 0f;

    [Header("Spine Aim")]
    public Transform spine;
    public Transform chest;

    public float maxLookUp = 40f;
    public float maxLookDown = -30f;
    public float spineSmooth = 8f;

    [Header("Right/Left Hand IK")]
    public Transform rightHandTarget;
    public Transform leftHandTarget;

    [Range(0f, 1f)] public float rightHandPositionWeight = 0f;
    [Range(0f, 1f)] public float rightHandRotationWeight = 0f;

    [Range(0f, 1f)] public float leftHandPositionWeight = 0f;
    [Range(0f, 1f)] public float leftHandRotationWeight = 0f;

    [Header("IK Smooth")]
    public float ikWeightSmooth = 10f;

    [Header("Right Hand IK Offset")]
    public float rightHandOffsetRight = 0f;
    public float rightHandOffsetUp = 0f;
    public float rightHandOffsetForward = 0f;

    [Header("Animator Layers")]
    public int upperBodyLayerIndex = 1;
    public float upperBodyLayerSmooth = 10f;

    private float currentUpperBodyWeight;

    private bool isJumping = false;
    private bool isShooting = false;

    private float speedVelocity;
    private float currentSpeed;

    private float moveXVelocity;
    private float moveZVelocity;

    private float verticalVelocity;

    private Vector3 currentMoveDirection;
    private Vector3 moveDirectionVelocity;

    private Vector3 jumpHorizontalVelocity;

    private float cameraYaw;
    private float cameraPitch;

    private float lastCameraYaw;
    private float currentTurnValue;
    private float turnVelocity;

    private NPCHealth npcHealth;
    private Rigidbody rb;

    private Quaternion spineStartRot;
    private Quaternion chestStartRot;

    private float currentSpinePitch;

    void Start()
    {
        npcHealth = GetComponent<NPCHealth>();
        rb = GetComponent<Rigidbody>();

        lastCameraYaw = cameraYaw;

        if (spine != null)
            spineStartRot = spine.localRotation;

        if (chest != null)
            chestStartRot = chest.localRotation;
    }

    void Update()
    {
        isAiming = Input.GetMouseButton(1);

        if (npcHealth != null && npcHealth.currentHealth <= 0)
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

        HandleUpperBodyLayer();
        HandleAimIK();
        HandleMovement();

        if (Input.GetKeyDown(KeyCode.Space) && !isJumping && IsGrounded())
            StartJump();

        if (Input.GetKeyDown(KeyCode.E) && !isShooting)
            StartCoroutine(Shoot());
    }

    void LateUpdate()
    {
        HandleSpineAim();
    }

    bool IsReloading()
    {
        return weapon != null && weapon.IsReloading;
    }

    void HandleUpperBodyLayer()
    {
        bool reloading = IsReloading();

        float targetWeight = (isAiming || reloading) ? 1f : 0f;

        currentUpperBodyWeight = Mathf.Lerp(
            currentUpperBodyWeight,
            targetWeight,
            Time.deltaTime * upperBodyLayerSmooth
        );

        animator.SetLayerWeight(upperBodyLayerIndex, currentUpperBodyWeight);
    }

    void HandleAimIK()
    {
        bool reloading = IsReloading();

        float targetRightHandPos = 0f;
        float targetRightHandRot = 0f;
        float targetLeftHandPos = 0f;
        float targetLeftHandRot = 0f;

        if (!reloading && isAiming)
        {
            targetRightHandPos = 1f;
            targetRightHandRot = 0.225f;
            targetLeftHandPos = 1f;
            targetLeftHandRot = 1f;
        }

        rightHandPositionWeight = Mathf.Lerp(rightHandPositionWeight, targetRightHandPos, Time.deltaTime * ikWeightSmooth);
        rightHandRotationWeight = Mathf.Lerp(rightHandRotationWeight, targetRightHandRot, Time.deltaTime * ikWeightSmooth);
        leftHandPositionWeight = Mathf.Lerp(leftHandPositionWeight, targetLeftHandPos, Time.deltaTime * ikWeightSmooth);
        leftHandRotationWeight = Mathf.Lerp(leftHandRotationWeight, targetLeftHandRot, Time.deltaTime * ikWeightSmooth);
    }

    public void SetCameraYaw(float yaw) => cameraYaw = yaw;
    public void SetCameraPitch(float pitch) => cameraPitch = pitch;

    bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.25f;
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundMask);
    }

    Vector3 GetInputDirection()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ += 1f;
        if (Input.GetKey(KeyCode.S)) moveZ -= 1f;
        if (Input.GetKey(KeyCode.A)) moveX -= 1f;
        if (Input.GetKey(KeyCode.D)) moveX += 1f;

        Vector3 camForward = Quaternion.Euler(0f, cameraYaw, 0f) * Vector3.forward;
        Vector3 camRight = Quaternion.Euler(0f, cameraYaw, 0f) * Vector3.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        if (!isAiming && moveZ < 0f)
        {
            camForward *= -1f;
            moveX = 0f;
            moveZ = 1f;
        }

        return (camForward * moveZ + camRight * moveX).normalized;
    }

    void StartJump()
    {
        isJumping = true;
        animator.SetTrigger("Jump");

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float speed = isRunning ? moveSpeed * runMultiplier : moveSpeed;

        Vector3 dir = GetInputDirection();

        jumpHorizontalVelocity = dir * speed * jumpForwardMultiplier;
        verticalVelocity = Mathf.Sqrt(jumpHeight * gravityForce * 2f);

        StartCoroutine(JumpCoroutine());
    }

    void HandleMovement()
    {
        float moveX = 0f;
        float moveZ = 0f;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKey(KeyCode.W)) moveZ += 1f;
        if (Input.GetKey(KeyCode.S)) moveZ -= 1f;
        if (Input.GetKey(KeyCode.A)) moveX -= 1f;
        if (Input.GetKey(KeyCode.D)) moveX += 1f;

        if (!isAiming && moveZ < 0f)
            moveX = 0f;

        bool isMoving = moveX != 0 || moveZ != 0;

        float targetSpeed = isMoving ? (isRunning ? 2f : 1f) : 0f;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, 0.1f);
        animator.SetFloat("Speed", currentSpeed);

        Vector3 targetDir = GetInputDirection();

        currentMoveDirection = Vector3.SmoothDamp(currentMoveDirection, targetDir, ref moveDirectionVelocity, movementSmoothTime);

        float speed = isRunning ? moveSpeed * runMultiplier : moveSpeed;

        if (!IsGrounded() || verticalVelocity > 0f)
            verticalVelocity -= gravityForce * Time.deltaTime;
        else if (!isJumping)
            verticalVelocity = -2f;

        Vector3 horizontal = currentMoveDirection * speed;

        Vector3 final = horizontal;
        final.y = verticalVelocity;

        transform.position += final * Time.deltaTime;

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            rb.angularVelocity = Vector3.zero;
        }

        if (isAiming)
        {
            Quaternion camRot = Quaternion.Euler(0f, cameraYaw, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, camRot, cameraTurnSmooth * Time.deltaTime);
        }
        else if (rotateToMovement && currentMoveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(currentMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleSpineAim()
    {
        if (spine == null || chest == null) return;

        float pitch = cameraPitch;

        if (pitch > 180f)
            pitch -= 360f;

        pitch = Mathf.Clamp(pitch, maxLookDown, maxLookUp);

        currentSpinePitch = Mathf.Lerp(currentSpinePitch, pitch, spineSmooth * Time.deltaTime);

        if (isAiming && !IsReloading())
        {
            spine.localRotation = spineStartRot * Quaternion.Euler(currentSpinePitch * 0.4f, 0f, 0f);
            chest.localRotation = chestStartRot * Quaternion.Euler(currentSpinePitch * 0.6f, 0f, 0f);
        }
        else
        {
            spine.localRotation = Quaternion.Slerp(spine.localRotation, spineStartRot, spineSmooth * Time.deltaTime);
            chest.localRotation = Quaternion.Slerp(chest.localRotation, chestStartRot, spineSmooth * Time.deltaTime);
        }
    }

    IEnumerator JumpCoroutine()
    {
        while (true)
        {
            if (npcHealth != null && npcHealth.currentHealth <= 0)
            {
                isJumping = false;
                yield break;
            }

            if (verticalVelocity <= 0f && IsGrounded())
                break;

            yield return null;
        }

        verticalVelocity = -2f;
        jumpHorizontalVelocity = Vector3.zero;
        isJumping = false;
    }

    IEnumerator Shoot()
    {
        isShooting = true;
        animator.SetTrigger("Shoot");
        yield return new WaitForSeconds(shootCooldown);
        isShooting = false;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null)
            return;

        bool reloading = IsReloading();

        float rwPos = (!reloading) ? rightHandPositionWeight : 0f;
        float rwRot = (!reloading) ? rightHandRotationWeight : 0f;
        float lwPos = (!reloading) ? leftHandPositionWeight : 0f;
        float lwRot = (!reloading) ? leftHandRotationWeight : 0f;

        if (rightHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rwPos);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rwRot);

            Vector3 finalRightPos =
                rightHandTarget.position +
                rightHandTarget.right * rightHandOffsetRight +
                rightHandTarget.up * rightHandOffsetUp +
                rightHandTarget.forward * rightHandOffsetForward;

            animator.SetIKPosition(AvatarIKGoal.RightHand, finalRightPos);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }

        if (leftHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, lwPos);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, lwRot);

            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }
    }
}
}