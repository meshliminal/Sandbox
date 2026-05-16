using UnityEngine;
using System.Collections;

public class DuplicateOnTrigger : MonoBehaviour
{
    [Header("Settings")]
    public GameObject objectToDuplicate; // A tárgy vagy NPC, amit másolni szeretnél
    public bool buttonControl = false;  // Ha igaz, csak az E gombra mûködik
    public Transform spawnPoint;        // Hol jelenjen meg az új példány
    public bool allowMultipleActivations = true; // Többször aktiválható-e
    public float activationDelay = 0.2f; // Aktiválások közötti várakozási idõ

    private bool isPlayerInTrigger = false;
    private bool isCoroutineRunning = false;

    private static int cloneCount = 0; // Globális számláló a klónokhoz

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
        }
    }

    private void Update()
    {
        if (isPlayerInTrigger && !isCoroutineRunning)
        {
            if (buttonControl && Input.GetKeyDown(KeyCode.E))
            {
                DuplicateObject();
            }
            else if (!buttonControl)
            {
                DuplicateObject();
            }
        }
    }

    private IEnumerator DuplicateWithDelay()
    {
        isCoroutineRunning = true; // Jelzi, hogy a coroutine fut
        if (objectToDuplicate != null && spawnPoint != null)
        {
            DuplicateObject();
            yield return new WaitForSeconds(activationDelay); // Várakozás a delay idejéig
        }
        else
        {
            Debug.LogWarning("ObjectToDuplicate vagy SpawnPoint nincs beállítva!");
        }

        if (allowMultipleActivations)
        {
            isCoroutineRunning = false; // Ha több aktiválás megengedett, újra lehet indítani
        }
    }

    private void DuplicateObject()
    {
        GameObject clone = Instantiate(objectToDuplicate, spawnPoint.position, spawnPoint.rotation);
        clone.name = objectToDuplicate.name + "_Clone_" + ++cloneCount; // Név hozzáadása számozással
    }
}