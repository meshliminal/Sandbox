using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Ne felejtsd el hozzáadni az UI namespace-t

public class CubeController : MonoBehaviour
{





public GameObject cube; // Kocka GameObject referencia
    public GameObject outerCube; // Külsõ kocka GameObject referencia
    public Camera playerCamera; // Játékos kamera referencia
    public float moveSpeed = 5f; // Sebesség
    public Image cursorImage; // A kurzor kép referencia

    private bool isColliding = false; // Jelzi, hogy ütközünk-e a kockával
    private bool isPushing = false;
    void Start()
    {
        // Kezdjük a kurzor kép elrejtésével
        if (cursorImage != null)
        {
            cursorImage.enabled = false;
        }

    }

    void Update()
    {



        // Ellenõrizzük, hogy a kocka referencia érvényes-e
        if (cube != null && isColliding)
        {
            // Képzeljük el a kamera elõre nézõ irányát
            Vector3 forward = playerCamera.transform.forward;
            forward.y = 0; // Az Y irányt figyelmen kívül hagyjuk
            forward.Normalize();

            // Irányok beállítása a W, A, S, D gombok alapján
            Vector3 right = playerCamera.transform.right;
            right.y = 0; // Az Y irányt figyelmen kívül hagyjuk
            right.Normalize();

            // A mozgás iránya a W, A, S, D gombok alapján
            float moveVertical = Input.GetAxis("Vertical");
            float moveHorizontal = Input.GetAxis("Horizontal");
            Vector3 moveDirection = (forward * moveVertical) + (right * moveHorizontal);

            // Kocka mozgása, ha az 'E' billentyût nyomjuk és tolják a kockát
            if (Input.GetKey(KeyCode.E))
            {
                isPushing = true; // A kocka tolják
                cube.transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

            }
            else if (!Input.GetKey(KeyCode.E) && isPushing)
            {
                isPushing = false; // A kocka nem tolják
                // Leállítjuk a hangot, ha nem tolják a kockát

            }


        }
    }

    void OnTriggerStay(Collider other)
    {

        // A kamera irányába küldünk egy ray-t
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward); // A ray a kamera pozíciójából és irányából indul
        RaycastHit hit;

        // Ellenõrizzük, hogy a játékos ütközik-e
        if (other.CompareTag("Player"))
        {
            isColliding = true; // Ütközés történt
            Debug.Log("Ütközés");

            // A kurzor kép megjelenítése
            if (cursorImage != null)
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore))
                {
                    // Ellenõrizzük, hogy az objektum neve "trigger_cube"
                    if (hit.collider.gameObject.name == "drag")
                    {
                        // Kiíratjuk az objektum nevét
                        Debug.Log("Hit object: " + hit.collider.gameObject.name); cursorImage.enabled = true;
                    }
                    else
                    {
                        Debug.Log("no raycast: " + hit.collider.gameObject.name); cursorImage.enabled = false;
                    }
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Ellenõrizzük, hogy a játékos elhagyja-e a trigger területet
        if (other.CompareTag("Player"))
        {
            isColliding = false; // Ütközés véget ért
            Debug.Log("Ütközés nem");

            // A kurzor kép elrejtése
            if (cursorImage != null)
            {
                cursorImage.enabled = false;
            }
        }
    }
}