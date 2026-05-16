using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Vector3 startingPosition; // Kezdõpozíció
    public GameObject characterController; // Karakter controller
    public GameObject target; // Ha meg van adva, ide fog teleportálni

    void Start()
    {
        // A kezdõpozíció beállítása a karakter controller jelenlegi pozíciójára
        startingPosition = characterController.transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        // Ellenõrizzük, hogy a Player tagú objektum lépett-e be
        if (other.CompareTag("Player"))
        {
            ResetPlayerPosition();
        }
    }

    void ResetPlayerPosition()
    {
        Vector3 respawnPosition;

        // Ha meg van adva célpont, akkor azt használjuk
        if (target != null)
        {
            respawnPosition = target.transform.position;
        }
        else
        {
            respawnPosition = startingPosition;
        }

        // Karakter controller pozíciójának beállítása
        if (characterController != null)
        {
            //characterController.enabled = false; // Ideiglenesen kikapcsoljuk
            characterController.transform.position = respawnPosition;
            //characterController.enabled = true; // Visszakapcsoljuk
        }
    }
}
