using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

using UnityEngine;

public class BowlingBallShooter : MonoBehaviour
{
    public GameObject bowlingBallPrefab; // A golyó prefab
    public Transform shootPoint; // A hely, ahonnan a golyót lövik
    public float maxShootingTime = 1f; // Maximális lövési idő
    public float maxForce = 1000f; // Maximális erő

    private float currentShootingTime = 0f; // Jelenlegi lövési idő

    void Update()
    {
        // Ellenőrizd, hogy az egér bal gombját nyomják-e
        if (Input.GetMouseButtonDown(1)) // Bal egérgomb lenyomására
        {
            // Növeld a lövési időt
            currentShootingTime += Time.deltaTime;

            // Korlátozd a lövési időt
            if (currentShootingTime > maxShootingTime)
            {
                currentShootingTime = maxShootingTime;
            }
        }

        // Ellenőrizd, hogy elengedték-e az egér bal gombját
        if (Input.GetMouseButtonUp(1)) // 0 a bal gomb
        {
            Shoot(); // Hívja meg a lövési függvényt
            currentShootingTime = 0f; // Állítsd vissza a lövési időt
        }
    }

    void Shoot()
    {
        // Hozz létre egy új golyót a prefab alapján
        GameObject ball = Instantiate(bowlingBallPrefab, shootPoint.position, Quaternion.identity);

        // Szerezd meg a Rigidbody-t
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        // Számítsd ki az erőt a lövési idő alapján
        float force = currentShootingTime / maxShootingTime * maxForce;

        // Lődd el a golyót
        rb.AddForce(shootPoint.forward * force, ForceMode.Impulse);
    }
}