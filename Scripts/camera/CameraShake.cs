using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float duration = 0.5f;  // Mennyi ideig tartson a rázkódás
    public float magnitude = 0.3f; // A rázkódás erőssége

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    private void OnEnable()
    {
        // Eredeti pozíció mentése bekapcsoláskor
        originalPosition = transform.localPosition;
    }

    public void Shake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition = originalPosition; // visszaállítás, ha előző shake félbeszakadt
        }

        shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
}
