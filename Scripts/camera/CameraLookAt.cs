using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLookAt : MonoBehaviour
{
    public Transform target; // Az objektum, amit nézni kell.
    public float smoothTime = 1.0f; // Mennyi idõ alatt érje el teljesen a célpontot.
    private float lerpProgress = 0.0f; // Az interpoláció állapota (0.0 - kezdet, 1.0 - vége).

    void Update()
    {
        if (target != null)
        {
            // Lassan növeljük az interpoláció progressz értékét.
            lerpProgress += Time.deltaTime / smoothTime;
            lerpProgress = Mathf.Clamp01(lerpProgress); // Biztosítjuk, hogy 0 és 1 között maradjon.

            // A célpont irányába fordulás smooth interpolációval.
            Vector3 direction = target.position - transform.position; // Irány a cél felé.
            Quaternion targetRotation = Quaternion.LookRotation(direction); // Célt nézõ forgatás.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lerpProgress); // Smooth átmenet.
        }
    }
}