using UnityEngine;

public class TPSCamera : MonoBehaviour
{
    [Header("Targets")]
    public Transform playerTarget;

    [Header("Weapon")]
    public Weapon_Glock weapon;

    [Header("Player Controller")]
    public npc_move_anim playerController;

    [Header("Normal Offset")]
    public Vector3 normalOffset =
        new Vector3(0f, 2f, -4f);

    [Header("Aim Offset")]
    public Vector3 aimOffset =
        new Vector3(0.45f, 1.95f, -2.6f);

    [Header("Aim")]
    public float aimTargetDistance = 50f;

    [Header("Zoom")]
    public float zoomSpeed = 0.5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;

    [Header("Rotation")]
    public float mouseSensitivity = 3f;
    public float minYAngle = -30f;
    public float maxYAngle = 70f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.05f;
    public float aimSmoothSpeed = 8f;

    [Header("FOV")]
    public float normalFov = 60f;
    public float aimFov = 50f;
    public float fovSmooth = 8f;

    [Header("Collision")]
    public LayerMask levelLayer;
    public float collisionRadius = 0.2f;
    public float collisionOffset = 0.2f;

    private float yaw;
    private float pitch;

    private Vector3 currentOffset;
    private Vector3 positionVelocity;

    private bool isAiming;

    private Camera cam;

    void Start()
    {
        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;

        cam = Camera.main;

        Vector3 angles =
            transform.eulerAngles;

        yaw = angles.y;
        pitch = angles.x;

        currentOffset =
            normalOffset;
    }

    void LateUpdate()
    {
        if (playerTarget == null)
            return;

        HandleAim();
        HandleMouseInput();
        HandleZoom();
        HandleFOV();
        MoveCamera();
    }

    void HandleAim()
    {
        isAiming =
            Input.GetMouseButton(1);

        Vector3 targetOffset =
            isAiming
            ? aimOffset
            : normalOffset;

        currentOffset =
            Vector3.Lerp(
                currentOffset,
                targetOffset,
                Time.deltaTime *
                aimSmoothSpeed
            );
    }

    void HandleMouseInput()
    {
        float mouseX =
            Input.GetAxis("Mouse X") *
            mouseSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y") *
            mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(
            pitch,
            minYAngle,
            maxYAngle
        );

        if (playerController != null)
        {
            playerController
                .SetCameraYaw(yaw);
        }
    }

    void HandleZoom()
    {
        float scroll =
            Input.mouseScrollDelta.y;

        if (scroll != 0f)
        {
            normalOffset.z +=
                scroll * zoomSpeed;

            aimOffset.z +=
                scroll * zoomSpeed;

            normalOffset.z =
                Mathf.Clamp(
                    normalOffset.z,
                    -maxDistance,
                    -minDistance
                );

            aimOffset.z =
                Mathf.Clamp(
                    aimOffset.z,
                    -maxDistance,
                    -minDistance + 0.5f
                );
        }
    }

    void HandleFOV()
    {
        if (cam == null)
            return;

        float targetFov =
            isAiming
            ? aimFov
            : normalFov;

        cam.fieldOfView =
            Mathf.Lerp(
                cam.fieldOfView,
                targetFov,
                Time.deltaTime *
                fovSmooth
            );
    }

    void MoveCamera()
    {
        Quaternion rotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );

        // PLAYER körüli orbit marad

        Vector3 desiredPosition =
            playerTarget.position +
            rotation * currentOffset;

        // COLLISION

        Vector3 pivotPoint =
            playerTarget.position +
            Vector3.up * 1.6f;

        Vector3 direction =
            desiredPosition -
            pivotPoint;

        float distance =
            direction.magnitude;

        direction.Normalize();

        RaycastHit collisionHit;

        if (Physics.SphereCast(
            pivotPoint,
            collisionRadius,
            direction,
            out collisionHit,
            distance,
            levelLayer
        ))
        {
            desiredPosition =
                collisionHit.point -
                direction *
                collisionOffset;
        }

        // SMOOTH POSITION

        transform.position =
            Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                positionSmoothTime
            );

        // FONTOS:
        // Orbit rotáció marad aim közben is

        transform.rotation =
            rotation;
    }

    public bool IsAiming()
    {
        return isAiming;
    }

    public float GetYaw()
    {
        return yaw;
    }
}