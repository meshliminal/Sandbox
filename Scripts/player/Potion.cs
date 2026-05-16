using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public int healAmount = 50; // Mennyit gyógyít
    public bool isConsumed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isConsumed)
            return;

        // PLAYER gyógyítás
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.RefillHealth(healAmount);

            isConsumed = true;
            Destroy(gameObject);
            return;
        }

        // NPC gyógyítás
        NPCHealth npcHealth = other.GetComponent<NPCHealth>();

        if (npcHealth != null)
        {
            HealNPC(npcHealth);

            isConsumed = true;
            Destroy(gameObject);
        }
    }

    private void HealNPC(NPCHealth npcHealth)
    {
        npcHealth.currentHealth += healAmount;

        // Ne mehessen max fölé
        if (npcHealth.currentHealth > npcHealth.maxHealth)
        {
            npcHealth.currentHealth = npcHealth.maxHealth;
        }

        Debug.Log($"{npcHealth.gameObject.name} healed to {npcHealth.currentHealth}");
    }
}