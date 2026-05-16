using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScarabBehavior : MonoBehaviour
{
    public Transform player;
    public float baseSpeed = 2.0f;
    public float randomSpeedRange = 1.0f;
    public float damageRadius = 1.0f;
    public int damage = 10;
    public List<ScarabBehavior> allScarabs; // Minden scarabeusz a hordában
    public float cohesionStrength = 1.0f; // Összetartás erõssége (erõsebb kohézió)
    public float separationDistance = 1.5f; // Túl közeli távolságok elkerülése
    public float alignmentStrength = 1.0f; // Irányultság erõssége
    public float turnSpeed = 3.0f; // A scarabeuszok fordulási sebessége (lassú fordulás)

    private float speed;
    private bool isDealingDamage = false;
    private Vector3 randomOffset;
    private bool isLookingForPlayer = true;
    private float searchRadius = 10f;

    void Start()
    {
        speed = baseSpeed + Random.Range(-randomSpeedRange, randomSpeedRange);
        randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f)); // Célpont eltolás csökkentése
    }

    void Update()
    {
        if (isLookingForPlayer)
        {
            MoveRandomly();
            CheckForPlayer();
        }
        else
        {
            // A játékos pozíciója körül egy kis eltolás
            Vector3 targetPosition = player.position + randomOffset;

            // Kohéziós, szétválasztási és irányultsági erõk alkalmazása
            Vector3 cohesion = CalculateCohesion();
            Vector3 separation = CalculateSeparation();
            Vector3 alignment = CalculateAlignment();

            // A mozgás iránya a következõ számítások alapján
            Vector3 direction = (targetPosition - transform.position).normalized + cohesion + separation + alignment;
            transform.position += direction * speed * Time.deltaTime;

            // A scarabeuszok fokozatosan forduljanak a célpont felé
            Vector3 targetDirection = (targetPosition - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

            // Ellenõrizzük, hogy elérte-e a sebzés távolságát
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= damageRadius && !isDealingDamage)
            {
                StartCoroutine(DealDamage());
            }
        }
    }

    void MoveRandomly()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        transform.position += randomDirection * speed * Time.deltaTime;
    }

    void CheckForPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= searchRadius)
        {
            isLookingForPlayer = false; // Elkezdhet követni a játékost
        }
    }

    private Vector3 CalculateCohesion()
    {
        // Minden scarabeusz próbál egymáshoz közel maradni
        Vector3 centerOfMass = Vector3.zero;
        int count = 0;

        foreach (ScarabBehavior scarab in allScarabs)
        {
            if (scarab != this)
            {
                centerOfMass += scarab.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            centerOfMass /= count;
            return (centerOfMass - transform.position) * cohesionStrength;
        }

        return Vector3.zero;
    }

    private Vector3 CalculateSeparation()
    {
        // Ha a scarabeusz túl közel van egy másikhoz, távolodjon el
        Vector3 separation = Vector3.zero;
        foreach (ScarabBehavior scarab in allScarabs)
        {
            if (scarab != this)
            {
                float distance = Vector3.Distance(transform.position, scarab.transform.position);
                if (distance < separationDistance)
                {
                    separation += (transform.position - scarab.transform.position) / distance;
                }
            }
        }

        return separation;
    }

    private Vector3 CalculateAlignment()
    {
        // A scarabeuszok irányultságának egységesítése
        Vector3 averageHeading = Vector3.zero;
        int count = 0;

        foreach (ScarabBehavior scarab in allScarabs)
        {
            if (scarab != this)
            {
                averageHeading += scarab.transform.forward;
                count++;
            }
        }

        if (count > 0)
        {
            averageHeading /= count;
            return (averageHeading - transform.forward) * alignmentStrength;
        }

        return Vector3.zero;
    }

    private IEnumerator DealDamage()
    {
        isDealingDamage = true;
        Debug.Log("Scarab causes damage!");

        yield return new WaitForSeconds(1.0f);
        isDealingDamage = false;
    }
}