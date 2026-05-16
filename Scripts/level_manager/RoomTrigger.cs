using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public LoadingManager loadingManager; // Hivatkozás a LoadingManagerre
    public string playerTag = "Player";   // Csak a Player objektum indíthatja el

    private void OnTriggerEnter(Collider other)
    {
        // Csak a megfelelõ tagû objektum esetén értesítjük a LoadingManagert
        if (other.CompareTag(playerTag) && loadingManager != null)
        {
            loadingManager.LoadRoom();
        }
    }
}

