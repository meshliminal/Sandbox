using UnityEngine;
using System.Collections.Generic;

public class DuplicateOnTrigger : MonoBehaviour
{
    [Header("Settings")]
    public GameObject objectToDuplicate;
    public bool buttonControl = false;
    public Transform spawnPoint;

    public static List<GameObject> spawnedClones = new List<GameObject>();

    private bool isPlayerInTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerInTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerInTrigger = false;
    }

    private void Update()
    {
        if (!isPlayerInTrigger)
            return;

        if (buttonControl)
        {
            if (Input.GetKeyDown(KeyCode.E))
                DuplicateObject();
        }
        else
        {
            DuplicateObject();
        }
    }

    private void DuplicateObject()
    {
        if (objectToDuplicate == null || spawnPoint == null)
            return;

        GameObject clone = Instantiate(
            objectToDuplicate,
            spawnPoint.position,
            spawnPoint.rotation
        );

        spawnedClones.Add(clone);
    }
}