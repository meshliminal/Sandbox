using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObject : MonoBehaviour
{
    public Transform player;            // J�t�kos poz�ci�ja
    public Transform objectHolder;      // T�rgy helye, miut�n felvett�k
    public float pickupRange = 3f;      // Felvehet� t�rgy t�vols�ga
    public LayerMask pickupLayer;       // R�teg, amelyre az interakt�v t�rgyak vannak be�ll�tva
    public float pullBackForce = 10f;   // Er�, amivel a t�rgyat visszah�zzuk az objektumtart�hoz

    private GameObject currentObject = null;  // Felvett t�rgy
    private Rigidbody currentObjectRb = null;

    void Update()
    {
        // Felv�tel �s elenged�s
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentObject == null)
            {
                TryPickupObject();
            }
            else
            {
                DropObject();
            }
        }

        // Ha van felvett t�rgy, folyamatosan korrig�ljuk a poz�ci�j�t
        if (currentObject != null)
        {
            MaintainObjectPosition();
        }
    }

    void TryPickupObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupLayer))
        {
            Debug.Log("T�rgy megtal�lva!");

            if (hit.collider != null)
            {
                currentObject = hit.collider.gameObject;
                currentObjectRb = currentObject.GetComponent<Rigidbody>();

                if (currentObjectRb != null)
                {
                    currentObjectRb.useGravity = false;
                    currentObjectRb.linearVelocity = Vector3.zero;
                    currentObjectRb.angularVelocity = Vector3.zero; // Forg�st is null�zzuk
                }

                currentObject.transform.SetParent(objectHolder);
                currentObject.transform.localPosition = Vector3.zero;
            }
        }
    }

    void DropObject()
    {
        if (currentObjectRb != null)
        {
            currentObjectRb.useGravity = true;
        }

        currentObject.transform.SetParent(null);
        currentObject = null;
        currentObjectRb = null;
    }

    void MaintainObjectPosition()
    {
        // Ha van felvett t�rgy, mindig az objektumtart� poz�ci�j�ba h�zzuk
        Vector3 desiredPosition = objectHolder.position;
        Vector3 currentPosition = currentObject.transform.position;

        if (currentObjectRb != null)
        {
            Vector3 pullDirection = desiredPosition - currentPosition;
            currentObjectRb.linearVelocity = pullDirection * pullBackForce;
            currentObjectRb.angularVelocity = Vector3.zero;
        }
    }
}