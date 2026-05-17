using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 1;
    public float damageInterval = 0.05f;

    [Header("Movement Check")]
    public bool useMovementCheck = false;
    public float movementThreshold = 0.1f;

    private float lastDamageTime;
    private Rigidbody rb;

    // Health referenciák
    private PlayerHealth playerHealth;
    private NPCHealth npcHealth;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        playerHealth = GetComponent<PlayerHealth>();
        npcHealth = GetComponent<NPCHealth>();

        if (rb == null && useMovementCheck)
        {
            Debug.LogWarning("DamageDealer: Rigidbody hiányzik!");
        }
    }

    private void FixedUpdate()
    {
        // Ha meghalt → velocity nullázás
        if (IsDead() && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    bool IsDead()
    {
        if (playerHealth != null &&
            playerHealth.currentHealth <= 0)
            return true;

        if (npcHealth != null &&
            npcHealth.currentHealth <= 0)
            return true;

        return false;
    }

    private void OnTriggerStay(Collider other)
    {
        // Halott objektum ne sebezzen
        if (IsDead())
            return;

        bool canDealDamage = true;

        // Mozgás ellenőrzés
        if (useMovementCheck)
        {
            if (rb != null)
            {
                // Csak vízszintes mozgás számítson
                Vector3 horizontalVelocity = rb.linearVelocity;
                horizontalVelocity.y = 0f;

                canDealDamage =
                    horizontalVelocity.magnitude >
                    movementThreshold;
            }
            else
            {
                canDealDamage = false;
            }
        }

        // Sebzés időköz ellenőrzés
        if (!canDealDamage ||
            Time.time < lastDamageTime + damageInterval)
            return;

        // PLAYER sebzés
        PlayerHealth player =
            other.GetComponent<PlayerHealth>();

        if (player != null &&
            player.currentHealth > 0)
        {
            player.TakeDamage(damageAmount);
            lastDamageTime = Time.time;
            return;
        }

        // NPC sebzés
        NPCHealth npc =
            other.GetComponent<NPCHealth>();

        if (npc != null &&
            npc.currentHealth > 0)
        {
            npc.TakeDamage(damageAmount);
            lastDamageTime = Time.time;
        }
    }
}