using UnityEngine;

public class DoorController : MonoBehaviour
{
    public GameObject targetObject;
    public float rotationSpeed = 30f;
    public float rotationAngle = 90f;
    public bool isReverse = false;
    public bool shouldOpen = true;
    public bool triggerOnce = false;
    public bool allowButtonControl = false; // ÚJ: lehetõvé teszi az E gombos vezérlést

    public AudioSource audioSource;

    private bool isMoving = false;
    private bool hasTriggered = false;
    private Quaternion targetRotation;

    private bool hasPendingAction = false;
    private float pendingAngle = 0f;

    private bool isPlayerInTrigger = false; // ÚJ: figyeljük, bent van-e a játékos

    private void Update()
    {
        // Ajtó forgatása
        if (isMoving)
        {
            targetObject.transform.rotation = Quaternion.RotateTowards(
                targetObject.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(targetObject.transform.rotation, targetRotation) < 0.1f)
            {
                targetObject.transform.rotation = targetRotation;
                isMoving = false;
                if (audioSource != null) audioSource.Stop();

                if (hasPendingAction)
                {
                    hasPendingAction = false;
                    StartRotation(pendingAngle);
                }
            }
        }

        // E gomb kezelése csak ha bent van a játékos a triggerben
        if (allowButtonControl && isPlayerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            float desiredAngle = shouldOpen ? rotationAngle : -rotationAngle;

            if (!isMoving)
            {
                StartRotation(desiredAngle);
            }
            else
            {
                hasPendingAction = true;
                pendingAngle = desiredAngle;
            }

            if (triggerOnce)
            {
                hasTriggered = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;

            if (!allowButtonControl && (!triggerOnce || !hasTriggered))
            {
                float desiredAngle = shouldOpen ? rotationAngle : -rotationAngle;

                if (!isMoving)
                {
                    StartRotation(desiredAngle);
                }
                else
                {
                    hasPendingAction = true;
                    pendingAngle = desiredAngle;
                }

                if (triggerOnce)
                {
                    hasTriggered = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
        }
    }

    private void StartRotation(float angle)
    {
        float direction = isReverse ? -1f : 1f;
        Vector3 eulerRotation = new Vector3(0, angle * direction, 0);
        targetRotation = Quaternion.Euler(targetObject.transform.eulerAngles + eulerRotation);
        isMoving = true;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.Play();
        }
    }
}
