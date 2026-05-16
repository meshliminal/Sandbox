using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerGrow : MonoBehaviour
{
    public GameObject objectToGrow; // Az objektum, amit növelni szeretnél
    public Vector3 targetScale = new Vector3(1f, 1f, 1f); // Cél skála
    public float growSpeed = 1f; // Növekedés sebessége
    private Vector3 initialScale; // Kezdeti skála
    private Vector3 initialPosition; // Kezdeti pozíció
    private bool hasStartedGrowing = false; // Növekedési folyamat állapota

    private void Start()
    {
        if (objectToGrow != null)
        {
            // Az objektum kezdeti skálája és pozíciója
            initialScale = new Vector3(0f, 0f, 0f);
            initialPosition = objectToGrow.transform.position;
            objectToGrow.transform.localScale = initialScale;
        }
        else
        {
            Debug.LogWarning("Nincs kijelölt objektum a növekedéshez!");
        }
    }

    private void Update()
    {
        // Ellenõrizzük, hogy a G gombot lenyomták-e, és ha igen, elindítjuk a növekedést
        if (Input.GetKeyDown(KeyCode.G) && objectToGrow != null)
        {
            hasStartedGrowing = true; // Növekedés elindítása
        }

        // Csak akkor növekedjen, ha a növekedés elindult
        if (hasStartedGrowing && objectToGrow.transform.localScale != targetScale)
        {
            // Növekedés interpolálása
            objectToGrow.transform.localScale = Vector3.Lerp(objectToGrow.transform.localScale, targetScale, growSpeed * Time.deltaTime);

            // Új pozíció beállítása, hogy az objektum alja a földön maradjon
            float scaleY = objectToGrow.transform.localScale.y;
            objectToGrow.transform.position = initialPosition + new Vector3(0, scaleY / 2f, 0);

            // Ellenõrzés, hogy elértük-e a cél méretet
            if (Vector3.Distance(objectToGrow.transform.localScale, targetScale) < 0.01f)
            {
                objectToGrow.transform.localScale = targetScale;
                objectToGrow.transform.position = initialPosition + new Vector3(0, targetScale.y / 2f, 0);
                hasStartedGrowing = false; // Kikapcsoljuk a növekedést, ha elérte a cél méretet
            }
        }
    }

    // Trigger esemény kezelése
    private void OnTriggerEnter(Collider other)
    {
        // Ellenõrizzük, hogy egy objektum belépett a triggerbe, és elindítjuk a növekedést
        if (other.CompareTag("drag") && objectToGrow != null) // Feltételezzük, hogy a játékos tag-je "Player"
        {
            hasStartedGrowing = true; // Növekedés elindítása a trigger által
            Debug.Log("Növekedés elindítása a trigger által");
        }
    }


    // Trigger esemény kezelése
    private void OnTriggerExit(Collider other)
    {
        // Ellenõrizzük, hogy egy objektum belépett a triggerbe, és elindítjuk a növekedést
        if (other.CompareTag("drag") && objectToGrow != null) // Feltételezzük, hogy a játékos tag-je "Player"
        {
            hasStartedGrowing = false;
            objectToGrow.transform.localScale = new Vector3(0f, 0f, 0f);
        }
    }

}