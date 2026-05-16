using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSlow : MonoBehaviour
{
    public float slowMotionFactor = 0.5f; // Lassítás mértéke
    public float slowMotionDuration = 2f; // Lassítás időtartama

    private bool isSlowing = false;
    private float originalTimeScale;

    void Update()
    {
        // Az időlassítás aktiválása egy gomb lenyomásával
        if (Input.GetKeyDown(KeyCode.T) && !isSlowing)
        {
            StartCoroutine(SlowTime());
        }
    }

    IEnumerator SlowTime()
    {
        isSlowing = true;
        originalTimeScale = Time.timeScale;
        Time.timeScale = slowMotionFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // A FixedUpdate megfelelő működéséhez

        yield return new WaitForSecondsRealtime(slowMotionDuration); // Valós időben vár

        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = 0.02f; // Visszaáll az eredeti FixedUpdate idő
        isSlowing = false;
    }
}