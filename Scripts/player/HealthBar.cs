using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public NPCHealth npcHealth; // NPC health script
    public Image healthFill;

    private float smoothSpeed = 10f;

    void Update()
    {
        if (npcHealth != null && healthFill != null)
        {
            float targetFill = npcHealth.GetHealthPercentage();

            healthFill.fillAmount = Mathf.Lerp(
                healthFill.fillAmount,
                targetFill,
                smoothSpeed * Time.deltaTime
            );
        }
    }
}