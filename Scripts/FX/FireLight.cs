using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireLight : MonoBehaviour
{
    public Light fireLight; // A lámpa objektum
    public float minIntensity = 0.8f;
    public float maxIntensity = 2.0f;
    public float flickerSpeed = 0.1f;

    private void Start()
    {
        if (fireLight == null)
        {
            fireLight = GetComponent<Light>();
        }

        // Tűz sárgás szín
        fireLight.color = new Color(1.0f, 0.6f, 0.0f); // Narancssárga-sárga árnyalat
    }

    private void Update()
    {
        // Véletlenszerű villogás
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0.0f);
        fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}