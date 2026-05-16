using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public GameObject loadingScreen;  // A loading képernyõ UI
    public Image loadingImage;        // A loading kép, amely az animációt reprezentálja (Sprite, ami "kitöltõdik")
    public GameObject roomToActivate; // A szoba fõobjektuma
    private GameObject[] roomParts;   // A szoba részei

    private void Start()
    {
        // A szoba részeit mentjük el
        roomParts = GetRoomParts(roomToActivate);

        // Kezdetben a szoba legyen inaktív
        roomToActivate.SetActive(false);

        // Kezdetben állítsd a fillAmount-ot 0-ra
        if (loadingImage != null)
        {
            loadingImage.fillAmount = 0f;
        }
    }

    public void LoadRoom()
    {
        StartCoroutine(ActivateRoomStepByStep());
    }

    private IEnumerator ActivateRoomStepByStep()
    {
        // Aktiváld a loading képernyõt
        loadingScreen.SetActive(true);

        // Számítsd ki, hogy egy rész aktiválása mennyi progress-t jelent
        int totalParts = roomParts.Length;
        float progressPerStep = 1f / totalParts;

        // Lépésenként aktiváljuk a szoba részeit
        for (int i = 0; i < totalParts; i++)
        {
            roomParts[i].SetActive(true);

            // Ha van loading kép, változtassuk a fillAmount értékét
            if (loadingImage != null)
            {
                // Frissítsük a fillAmount-ot a progressz alapján
                loadingImage.fillAmount = (i + 1) * progressPerStep;
            }

            // Várj egy keretet, hogy vizuális feedback legyen
            yield return null;
        }

        // A szoba teljesen aktív
        roomToActivate.SetActive(true);

        // Elrejted a loading képernyõt
        loadingScreen.SetActive(false);
    }

    // Helper függvény a szoba részeinek listázásához
    private GameObject[] GetRoomParts(GameObject room)
    {
        return room.GetComponentsInChildren<Transform>(true) // Minden gyerek objektumot visszaad
            .Where(t => t.gameObject != room)               // Kivéve a root objektumot
            .Select(t => t.gameObject)                      // Csak a GameObject-ek kellenek
            .ToArray();
    }
}