using UnityEngine;

public class FlybyCamera : MonoBehaviour
{
    public float speed = 5.0f; // Alap sebesség
    public float sensitivity = 2.0f; // Egér érzékenység

    private float rotationY = 0.0f; // Vízszintes forgás
    private float rotationX = 0.0f; // Függőleges forgás


    void Start()
    {
        Cursor.visible = false; // Elrejti az egérmutatót
        Cursor.lockState = CursorLockMode.Locked; // Zárolja az egérmutatót
    }

    void Update()
    {

        // Egér mozgás (mindig aktív)
        float mouseX = Input.GetAxis("Mouse X") * sensitivity; // Vízszintes mozgás
        rotationY += mouseX; // Vízszintes forgatás


            float mouseY = Input.GetAxis("Mouse Y") * sensitivity; // Függőleges mozgás
            rotationX -= mouseY; // Függőleges forgatás
            rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Korlátozza a függőleges forgatást


        // Forgatás alkalmazása
        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);

        // Mozgás
        float moveX = Input.GetAxis("Horizontal") * speed * Time.deltaTime; // Balra-jobbra
        float moveZ = Input.GetAxis("Vertical") * speed * Time.deltaTime; // Előre-hátra
        float moveY = 0f;

        // Felfelé és lefelé mozgás
        if (Input.GetKey(KeyCode.LeftShift)) // Shift lenyomva
        {
            moveY = speed * Time.deltaTime; // Felfelé
        }
        else if (Input.GetKey(KeyCode.LeftControl)) // Ctrl lenyomva
        {
            moveY = -speed * Time.deltaTime; // Lefelé
        }

        // Mozgás alkalmazása
        Vector3 move = transform.right * moveX + transform.forward * moveZ + transform.up * moveY;
        transform.position += move;
    }

    void OnDisable()
    {
        // Visszaállítja az egérmutatót, amikor a script letiltódik
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}