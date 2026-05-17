using UnityEngine;

public class TPSCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Player Controller")]
    public npc_move_anim playerController;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 2f, -4f);

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

    [Header("Collision")]
    public LayerMask levelLayer;
    public float collisionRadius = 0.2f;
    public float collisionOffset = 0.2f;

    private float yaw;
    private float pitch;

    private Vector3 positionVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleMouseInput();
        HandleZoom();
        MoveCamera();
    }

    void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);

        // Kommunikáció a playerrel
        if (playerController != null)
        {
            playerController.SetCameraYaw(yaw);
        }
    }

    void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (scroll != 0f)
        {
            offset.z += scroll * zoomSpeed * 0.05f;

            offset.z = Mathf.Clamp(
                offset.z,
                -maxDistance,
                -minDistance
            );
        }
    }

    void MoveCamera()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 desiredPosition =
            target.position +
            rotation * offset;

        Vector3 targetPosition =
            target.position + Vector3.up * 1.5f;

        Vector3 direction =
            desiredPosition - targetPosition;

        float distance = direction.magnitude;

        direction.Normalize();

        RaycastHit hit;

        if (Physics.SphereCast(
            targetPosition,
            collisionRadius,
            direction,
            out hit,
            distance,
            levelLayer
        ))
        {
            desiredPosition =
                hit.point -
                direction * collisionOffset;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref positionVelocity,
            positionSmoothTime
        );

        transform.LookAt(targetPosition);
    }
}