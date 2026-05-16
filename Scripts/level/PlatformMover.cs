using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    public GameObject platform;  // A platform GameObject
    public GameObject target;    // A cél GameObject (üres objektum)
    public float speed = 2f;     // Mozgás sebessége

    public bool returnAfterDelay = false; // Opció: Visszatérjen-e az eredeti helyére
    public float returnDelay = 3f;        // Mennyi idő után térjen vissza

    public bool allowButtonControl = false; // Opció: lehessen-e gombbal indítani (E)

    private bool isMoving = false;          // Épp előre mozog
    private bool isReturning = false;       // Épp visszatér
    private bool isInMotionCycle = false;   // Teljes mozgásciklus folyamatban van-e
    private bool isPlayerInTrigger = false; // A játékos a triggerben van-e

    private Vector3 startPosition;          // Eredeti pozíció

    private void Start()
    {
        startPosition = platform.transform.position;
    }

    private void Update()
    {
        if (isMoving)
        {
            MovePlatformTowardsTarget();
        }

        if (allowButtonControl && isPlayerInTrigger)
        {
            if (Input.GetKeyDown(KeyCode.E) && !isMoving && !isInMotionCycle && !isReturning)
            {
                StartMoving();
            }
        }
    }

    // Platform mozgatása a cél felé
    public void MovePlatformTowardsTarget()
    {
        if (platform != null && target != null)
        {
            platform.transform.position = Vector3.MoveTowards(platform.transform.position, target.transform.position, speed * Time.deltaTime);

            if (Vector3.Distance(platform.transform.position, target.transform.position) < 0.01f)
            {
                isMoving = false;

                if (returnAfterDelay)
                {
                    StartCoroutine(ReturnToStartPosition());
                }
                else
                {
                    isInMotionCycle = false;
                }
            }
        }
    }

    // Platform visszahúzása az eredeti helyére
    private IEnumerator ReturnToStartPosition()
    {
        isReturning = true;
        yield return new WaitForSeconds(returnDelay);

        while (Vector3.Distance(platform.transform.position, startPosition) > 0.01f)
        {
            platform.transform.position = Vector3.MoveTowards(platform.transform.position, startPosition, speed * Time.deltaTime);
            yield return null;
        }

        isReturning = false;
        isInMotionCycle = false;
    }

    // Platform mozgásának elindítása
    public void StartMoving()
    {
        if (isInMotionCycle || isReturning) return;

        isInMotionCycle = true;
        isMoving = true;
    }

    // Ha a játékos belép a triggerbe
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;

            if (!allowButtonControl && !isInMotionCycle && !isReturning)
            {
                StartMoving();
            }
        }
    }

    // Ha a játékos kilép a triggerből
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
        }
    }
}
