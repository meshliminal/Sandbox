using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public GameObject objectToDetach;

    private npc_controller npcController;

    void Start()
    {
        currentHealth = maxHealth;
        npcController = GetComponent<npc_controller>();

        if (npcController == null)
        {
            Debug.LogError("npc_controller script is missing on this NPC!");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log($"{gameObject.name} took {damage} damage, current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} has died!");

        if (objectToDetach != null)
        {
            DetachObject();
        }

        if (npcController != null)
        {
            npcController.ActivateRagdoll();
        }
    }

    private void DetachObject()
    {
        objectToDetach.transform.parent = null;

        Rigidbody rb = objectToDetach.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"{objectToDetach.name} sikeresen leválasztva!");
    }
}