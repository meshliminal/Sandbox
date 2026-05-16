using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideOnTrigger : MonoBehaviour
{
    public GameObject objectToHide; // A GameObject, amit el akarunk rejteni

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Csak a "Player" tagekkel működik
        {
            if (objectToHide != null)
            {
                objectToHide.SetActive(false); // Elrejti az objektumot
            }
        }
    }
}
