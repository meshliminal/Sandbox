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

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null && useMovementCheck)
        {
            Debug.LogWarning("DamageDealer: Rigidbody hiányzik!");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        bool canDealDamage = true;

        // Mozgás ellenőrzés
        if (useMovementCheck)
        {
            if (rb != null)
            {
                canDealDamage = rb.linearVelocity.magnitude > movementThreshold;
            }
            else
            {
                canDealDamage = false;
            }
        }

        // Sebzés időköz ellenőrzés
        if (!canDealDamage || Time.time < lastDamageTime + damageInterval)
            return;

        // PLAYER sebzés
        PlayerHealth player = other.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damageAmount);
            lastDamageTime = Time.time;
            return;
        }

        // NPC sebzés
        NPCHealth npc = other.GetComponent<NPCHealth>();

        if (npc != null)
        {
            npc.TakeDamage(damageAmount);
            lastDamageTime = Time.time;
        }
    }
}