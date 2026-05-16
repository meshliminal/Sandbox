using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LaserPointer : MonoBehaviour
{
    public Camera playerCamera; // Játékos kamerája
    public GameObject laserDecalPrefab; // Decal prefab a lézerponthoz
    private GameObject laserDecalInstance; // Aktív lézerdecal
    public LayerMask collisionLayers; // Layer mask a Raycast számára

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        // Létrehozza a lézer pontot, de láthatatlanná teszi alapból
        laserDecalInstance = Instantiate(laserDecalPrefab);
        laserDecalInstance.SetActive(false);
    }

    void Update()
    {
        RaycastHit hit;
        Vector3 rayOrigin = playerCamera.transform.position; // Ray kezdõpontja
        Vector3 rayDirection = playerCamera.transform.forward; // Ray iránya

        // Raycast a kiválasztott layerekre, trigger colliderek nélkül
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, Mathf.Infinity, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            // Lézer pont pozicionálása és aktiválása
            laserDecalInstance.SetActive(true);
            laserDecalInstance.transform.position = hit.point + hit.normal * 0.01f;

            // A lézer decal mindig a kamera felé néz
            laserDecalInstance.transform.rotation = Quaternion.LookRotation(playerCamera.transform.position - hit.point);

            // Lézer színének beállítása pirosra
            laserDecalInstance.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            // Ha nincs találat, kikapcsolja a lézer pontot
            laserDecalInstance.SetActive(false);
        }
    }
}