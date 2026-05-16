using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    public GameObject platform;  // A platform GameObject referencia
    public float loweredPosition = -0.5f;  // Az elmozdulás mértéke
    public float moveSpeed = 0.5f;  // A mozgás sebessége (egység/másodperc)
    private Vector3 originalPosition;  // Eredeti helyzet
    private bool isPlayerOnPlatform = false;

    void Start()
    {
        if (platform != null)
        {
            originalPosition = platform.transform.position;  // Eredeti hely eltárolása
        }
        else
        {
            Debug.LogError("A platform GameObject nincs hozzárendelve!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Ha a játékos lép a trigger zónába
        //if (other.CompareTag("Player"))
        //{
            isPlayerOnPlatform = true;
        //}
    }

    void OnTriggerExit(Collider other)
    {
        // Ha a játékos elhagyja a trigger zónát
        //if (other.CompareTag("Player"))
        //{
            isPlayerOnPlatform = false;
        //}
    }

    void FixedUpdate()
    {
        if (platform != null)
        {
            Vector3 targetPosition = isPlayerOnPlatform ? originalPosition + new Vector3(0, loweredPosition, 0) : originalPosition;
            platform.transform.position = Vector3.MoveTowards(platform.transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        }
    }
}
