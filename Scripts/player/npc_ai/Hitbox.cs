using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public string bodyPartName; // Pl. "Head", "Chest", stb.
    public float damageMultiplier = 1.0f; // Fejre pl. nagyobb szorzó

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log($"Hit on {bodyPartName}!");

            // Sebzés kiszámítása és konvertálása egész számra
            int damage = Mathf.RoundToInt(collision.relativeVelocity.magnitude * damageMultiplier);

            // Keressük meg a karakter fő szkriptjét, ami kezeli a sebzést
            NPCHealth characterHealth = GetComponentInParent<NPCHealth>();
            if (characterHealth != null)
            {
                characterHealth.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("Character health component not found!");
            }

            // Golyó eltávolítása, ha szükséges
            Destroy(collision.gameObject);
        }
    }
}