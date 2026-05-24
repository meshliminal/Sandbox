using UnityEngine;
using System.Collections;

public class LeverRotation : MonoBehaviour
{
    public GameObject lever;
    public float rotationDuration = 1f;
    public float waitBeforeReset = 2f;

    private bool isPlayerInside = false;
    private bool isRotating = false;

    private void Start()
    {
        if (lever != null)
        {
            Vector3 currentEuler = lever.transform.rotation.eulerAngles;
            // Kezdeti állapot: lefele, Z = -45f
            lever.transform.rotation = Quaternion.Euler(currentEuler.x, currentEuler.y, -45f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    private void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E) && !isRotating)
        {
            StartCoroutine(RotateLeverSequence());
        }
    }

    private IEnumerator RotateLeverSequence()
    {
        isRotating = true;

        Vector3 currentEuler = lever.transform.rotation.eulerAngles;

        Quaternion startRotation = lever.transform.rotation;
        Quaternion upRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, 45f);
        Quaternion downRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, -45f);
        float elapsed = 0f;

        // Forgatás felfele
        while (elapsed < rotationDuration)
        {
            lever.transform.rotation = Quaternion.Slerp(startRotation, upRotation, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        lever.transform.rotation = upRotation;

        // Várakozás
        yield return new WaitForSeconds(waitBeforeReset);

        // Visszaforgatás lefele
        elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            lever.transform.rotation = Quaternion.Slerp(upRotation, downRotation, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        lever.transform.rotation = downRotation;

        isRotating = false;
    }
}
