using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f; // Maximális életerõ
    private float currentHealth;

    public GameObject deadBody;
    public GameObject deadUi;
    private bool isDead = false; // Halál állapot nyomon követése
    



void Start()
    {
        currentHealth = maxHealth; // Kezdetben teljes élet
        deadBody.SetActive(false);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return; // Ne vegyen kárt, ha már halott
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Nem mehet 0 alá vagy max fölé

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
                // Ha be van állítva a deadBody, frissítjük a pozícióját és rotációját
        if (deadBody != null)
        {
            // Pozíció frissítése (játékos alatt marad)
            Vector3 offset = new Vector3(0, -0.5f, 0); // Példa offset: felfelé és elõre
            deadBody.transform.position = transform.position + offset;

            // Rotáció frissítése (játékos rotációját másolja)
            deadBody.transform.rotation = transform.rotation;

            ActivateWithDelay();
            deadUi.SetActive(true);
        }
    }

    public void RefillHealth(float amount)
    {
        currentHealth += amount; // Hozzáadjuk a gyógyítást
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth; // Ne lépje túl a maximális életerõt
        }
        //healthBar.UpdateHealthBar(currentHealth / maxHealth); // Életcsík frissítése
        Debug.Log("Health refilled by " + amount);
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth; // Életcsíkhoz százalék
    }

    public void ActivateWithDelay()
    {
        deadBody.SetActive(true); // Aktiváljuk a testet
        StartCoroutine(DisableKinematicAfterDelay(0.05f)); // 1 másodperc várakozás után kikapcsoljuk az isKinematic-et
    }

    private IEnumerator DisableKinematicAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Rigidbody[] rigidbodies = deadBody.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false; // Kikapcsoljuk az isKinematic-et
        }
    }
}