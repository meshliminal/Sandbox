using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyActivator : MonoBehaviour
{
    public Rigidbody selectedRigidbody;

    [Header("Force Settings")]
    public bool addForceAfterActivation = false;
    public Vector3 forceDirection = Vector3.forward;
    public float forceMagnitude = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && selectedRigidbody != null)
        {
            selectedRigidbody.isKinematic = false;

            if (addForceAfterActivation)
            {
                // Normáljuk az irányt, nehogy véletlenül nullás legyen az irányvektor
                Vector3 normalizedDirection = forceDirection.normalized;
                selectedRigidbody.AddForce(normalizedDirection * forceMagnitude, ForceMode.Impulse);
            }
        }
    }
}