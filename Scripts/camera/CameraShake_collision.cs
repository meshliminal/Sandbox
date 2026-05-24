using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake_collision : MonoBehaviour
{
    public string levelLayer = "level"; // A Layer neve
    public float minSpeed = 1.0f; // Minimális sebesség a rázáshoz

    private Rigidbody rb;
    private CameraShake cameraShake;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraShake = FindObjectOfType<CameraShake>();
        if (rb == null)
        {
            Debug.LogWarning("Nincs Rigidbody komponens csatolva erre az objektumra!");
        }
        if (cameraShake == null)
        {
            Debug.LogWarning("Nem található CameraShake komponens a jelenetben!");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(levelLayer))
        {
            Debug.Log("A labda ütközött a Level réteggel!");

            if (rb != null)
            {
                float speed = rb.linearVelocity.magnitude;
                if (speed > minSpeed)
                {
                    cameraShake?.Shake();
                }
                else
                {
                    Debug.Log("Sebesség túl alacsony, nem rázunk. Sebesség: " + speed);
                }
            }
        }
    }
}