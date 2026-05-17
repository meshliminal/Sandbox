using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f; // Maximális életerõ
    public float currentHealth;

    public GameObject deadBody;
    public GameObject deadUi;

    private bool isDead = false; // Halál állapot nyomon követése

    private Rigidbody playerRb;
    private Collider[] playerColliders;

    void Start()
    {
        currentHealth = maxHealth; // Kezdetben teljes élet

        deadBody.SetActive(false);

        playerRb = GetComponent<Rigidbody>();
        playerColliders = GetComponentsInChildren<Collider>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return; // Ne vegyen kárt, ha már halott

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Current Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player has died!");
        isDead = true;

        // PLAYER MOZGÁS LEÁLLÍTÁS
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.isKinematic = true;
        }

        // COLLIDEREK KIKAPCSOLÁSA
        foreach (Collider col in playerColliders)
        {
            col.enabled = false;
        }

        // Dead body aktiválás
        if (deadBody != null)
        {
            // Pozíció és rotáció másolása
            Vector3 offset = new Vector3(0, -0.5f, 0);

            deadBody.transform.position = transform.position + offset;
            deadBody.transform.rotation = transform.rotation;

            deadBody.SetActive(true);

            Rigidbody[] rigidbodies =
                deadBody.GetComponentsInChildren<Rigidbody>();

            // Nullázás mielõtt aktiváljuk
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = true;

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            StartCoroutine(DisableKinematicAfterDelay());
        }

        if (deadUi != null)
        {
            deadUi.SetActive(true);
        }

        // Élõ karakter kikapcsolása
        gameObject.SetActive(false);
    }

    public void RefillHealth(float amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log("Health refilled by " + amount);
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    private IEnumerator DisableKinematicAfterDelay()
    {
        yield return new WaitForFixedUpdate();

        Rigidbody[] rigidbodies =
            deadBody.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;

            // EXTRA BIZTONSÁG
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}