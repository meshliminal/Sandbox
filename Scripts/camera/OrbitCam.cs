using UnityEngine;

public class OrbitCam : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Distance")]
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 15f;

    [Header("Rotation")]
    public float xSpeed = 200f;
    public float ySpeed = 120f;

    public float yMinLimit = -40f;
    public float yMaxLimit = 80f;

    float x;
    float y;

    [Header("Collision")]
    public LayerMask collisionMask;
    public float collisionOffset = 0.2f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        if (target == null)
        {
            Debug.LogError("OrbitCamera: No target assigned!");
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        // Mouse input
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;

            y = ClampAngle(y, yMinLimit, yMaxLimit);
        }

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * 5f;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Rotation
        Quaternion rotation = Quaternion.Euler(y, x, 0);

        // Desired position
        Vector3 negDistance = new Vector3(0, 0, -distance);
        Vector3 position = rotation * negDistance + target.position;

        // Collision check (IMPORTANT for Level layer issue)
        Vector3 direction = (position - target.position).normalized;
        float targetDistance = distance;

        if (Physics.Raycast(target.position, direction, out RaycastHit hit, distance, collisionMask))
        {
            targetDistance = hit.distance - collisionOffset;
        }

        targetDistance = Mathf.Clamp(targetDistance, minDistance, distance);

        Vector3 finalPosition = target.position + (rotation * new Vector3(0, 0, -targetDistance));

        transform.rotation = rotation;
        transform.position = finalPosition;
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}