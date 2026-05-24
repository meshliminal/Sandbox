using UnityEngine;

namespace sandbox
{
public class TPSCamera : MonoBehaviour
{
    [Header("Targets")]
    public Transform playerTarget;

    [Header("Weapon")]
    public Weapon_Glock weapon;

    [Header("Player Controller")]
    public tps_player playerController;

    [Header("Crosshair")]
    public GameObject crosshairObject;

    [Header("Normal Offset")]
    public Vector3 normalOffset =
        new Vector3(0f, 1.8f, -5.5f);

    [Header("Aim Offset")]
    public Vector3 aimOffset =
        new Vector3(0.4f, 1.45f, -2f);

    [Header("Aim")]
    public float aimTargetDistance = 100f;

    [Header("Zoom")]
    public float zoomSpeed = 1f;
    public float minDistance = 1.5f;
    public float maxDistance = 12f;

    [Header("Rotation")]
    public float mouseSensitivity = 2.2f;
    public float rotationSmoothSpeed = 14f;
    public float minYAngle = -35f;
    public float maxYAngle = 75f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.12f;
    public float aimSmoothSpeed = 10f;

    [Header("FOV")]
    public float normalFov = 65f;
    public float aimFov = 38f;
    public float fovSmooth = 10f;

    [Header("Collision")]
    public LayerMask levelLayer;
    public float collisionRadius = 0.25f;
    public float collisionOffset = 0.2f;

    private float yaw;
    private float pitch;

    private float currentYaw;
    private float currentPitch;

    private Vector3 currentOffset;
    private Vector3 positionVelocity;

    private bool isAiming;

    private Camera cam;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam = Camera.main;

        Vector3 angles = transform.eulerAngles;

        yaw = angles.y;
        pitch = angles.x;

        currentYaw = yaw;
        currentPitch = pitch;

        currentOffset = normalOffset;

        if (crosshairObject != null)
        {
            crosshairObject.SetActive(false);
        }
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
        isAiming = Input.GetMouseButton(1);

        if (crosshairObject != null)
            crosshairObject.SetActive(isAiming);

        Vector3 targetOffset =
            isAiming ? aimOffset : normalOffset;

        currentOffset =
            Vector3.Lerp(
                currentOffset,
                targetOffset,
                Time.deltaTime * aimSmoothSpeed
            );
    }

    void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);

        currentYaw = Mathf.LerpAngle(currentYaw, yaw, Time.deltaTime * rotationSmoothSpeed);
        currentPitch = Mathf.Lerp(currentPitch, pitch, Time.deltaTime * rotationSmoothSpeed);

        if (playerController != null)
        {
            playerController.SetCameraYaw(currentYaw);
            playerController.SetCameraPitch(currentPitch);
        }
    }

    void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (scroll != 0f)
        {
            normalOffset.z += scroll * zoomSpeed;
            normalOffset.z = Mathf.Clamp(normalOffset.z, -maxDistance, -minDistance);

            aimOffset.z += scroll * zoomSpeed;
            aimOffset.z = Mathf.Clamp(aimOffset.z, -6f, -1f);
        }
    }

    void HandleFOV()
    {
        if (cam == null) return;

        float targetFov = isAiming ? aimFov : normalFov;

        cam.fieldOfView =
            Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * fovSmooth);
    }

    void MoveCamera()
    {
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

        Vector3 pivotPoint = playerTarget.position + Vector3.up * 1.45f;

        Vector3 desiredPosition = pivotPoint + rotation * currentOffset;

        Vector3 direction = desiredPosition - pivotPoint;
        float distance = direction.magnitude;
        direction.Normalize();

        if (Physics.SphereCast(pivotPoint, collisionRadius, direction, out RaycastHit hit, distance, levelLayer))
        {
            desiredPosition = hit.point - direction * collisionOffset;
        }

        transform.position =
            Vector3.SmoothDamp(transform.position, desiredPosition, ref positionVelocity, positionSmoothTime);

        transform.rotation =
            Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSmoothSpeed);
    }

    // ✅ EZ AZ ÚJ FIX
    public Ray GetAimRay()
    {
        return cam.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)
        );
    }

    public bool IsAiming() => isAiming;

    public float GetYaw() => currentYaw;

    public float GetPitch() => currentPitch;
}
}