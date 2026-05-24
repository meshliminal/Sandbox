using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

using UnityEngine;

public class HeadFollow : MonoBehaviour
{
    public Transform target; // Célpont, amit a fej követni fog
    public Transform headBone; // A karakter "Head" bone-ja
    public float rotationSpeed = 5f; // Forgási sebesség
    public Vector2 pitchLimits = new Vector2(-20f, 20f); // Fel/le nézés határai (x: minimum, y: maximum)
    public Vector2 yawLimits = new Vector2(-45f, 45f); // Jobbra/balra nézés határai

    private Quaternion initialRotation;

    void Start()
    {
        if (headBone != null)
        {
            initialRotation = headBone.localRotation; // Fej alaphelyzetének mentése
        }
    }

    void LateUpdate()
    {
        if (headBone == null || target == null) return;

        // Cél irány kiszámítása
        Vector3 directionToTarget = target.position - headBone.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, transform.up);

        // Lokális szögek korlátozása
        Vector3 euler = (Quaternion.Inverse(transform.rotation) * targetRotation).eulerAngles;
        float pitch = Mathf.Clamp(NormalizeAngle(euler.x), pitchLimits.x, pitchLimits.y);
        float yaw = Mathf.Clamp(NormalizeAngle(euler.y), yawLimits.x, yawLimits.y);

        // Új forgatás beállítása
        Quaternion clampedRotation = Quaternion.Euler(pitch, yaw, 0f);
        headBone.localRotation = Quaternion.Slerp(headBone.localRotation, initialRotation * clampedRotation, Time.deltaTime * rotationSpeed);
    }

    // Szög normalizálása (0-360° helyett -180°-180°)
    private float NormalizeAngle(float angle)
    {
        angle = angle > 180f ? angle - 360f : angle;
        return angle;
    }
}