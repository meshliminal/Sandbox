using System.Collections;
using UnityEngine;

public class DartShooter : MonoBehaviour
{
    public GameObject arrowPrefab; // Nyíl prefab
    public Transform shootPoint; // Kilövési pont
    public float shootForce = 10f; // Lövési erő
    public float fireRate = 1.5f; // Lövési időköz

    private Coroutine shootingCoroutine; // Coroutine referencia

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Játékos aktiválja
        {
            // Ha a coroutine már fut, ne indítsuk el újra
            if (shootingCoroutine == null)
            {
                shootingCoroutine = StartCoroutine(ShootDarts());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // Ha kilép, leáll
        {
            //StopShooting();
        }
    }

    private void StopShooting()
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine); // Megállítjuk a lövés coroutine-t
            shootingCoroutine = null;
        }
    }

    IEnumerator ShootDarts()
    {
        while (true) // Folyamatosan lövünk
        {
            Shoot();
            yield return new WaitForSeconds(fireRate); // Várakozás a következő lövésig
        }
    }

    void Shoot()
    {
        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, shootPoint.rotation);

        // Apply a controlled rotation to the arrow
        // Rotate the arrow by a fixed value (not random), to simulate a directional shot
        float rotationX = 90f; // Tilt the arrow slightly up (adjustable)
        float rotationY = 0f;  // You can adjust this if you want the arrow to tilt sideways

        // Rotate the arrow around its own axes (in world space)
        arrow.transform.Rotate(rotationX, rotationY, 0f);

        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shootPoint.forward * shootForce;
        }

        // Nyíl automatikus eltüntetése 5 másodperc után
        Destroy(arrow, 5f); // 5 másodperc után törlés
    }
}
