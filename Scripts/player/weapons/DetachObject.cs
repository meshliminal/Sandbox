using UnityEngine;

namespace sandbox
{
public class DetachObject : MonoBehaviour
{
    public GameObject objectToDetach; // Ezt a GameObjectet szeretnéd leválasztani.

    void Update()
    {
        // Ha lenyomod az "F" gombot, leválasztjuk az objektumot.
        if (Input.GetKeyDown(KeyCode.F) && objectToDetach != null)
        {
            Detach();
        }
    }

    void Detach()
    {
        // A szülő-gyerek kapcsolat megszüntetése.
        objectToDetach.transform.parent = null;

        // Ellenőrizzük, hogy van-e Rigidbody komponens.
        Rigidbody rb = objectToDetach.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // Fizikailag szabad mozgást engedélyezünk.

            // Az összes gyorsulás és erő nullázása.
            rb.linearVelocity = Vector3.zero; // Sebesség nullázása.
            rb.angularVelocity = Vector3.zero; // Forgási sebesség nullázása.

            // Erők nullázása, ha szükséges (alap Unity fizikában erre nincs külön API, de velocity reset elég szokott lenni).
        }

        Debug.Log($"{objectToDetach.name} sikeresen leválasztva, minden gyorsulás és erő nullázva!");
    }
}
}