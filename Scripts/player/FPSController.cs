using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    // Kamera beállítások
    public float mouseSensitivity = 100f;
    public float joySensitivity = 100f;
    public Transform playerCamera;
    float xRotation = 0f;
    //float xjoyRotation = 0f;

    // Xbox controller analóg karok
    private Vector2 rightStickInput;

    // Privát változók a mozgáshoz
    CharacterController controller;
    Vector3 velocity;
    bool isGrounded;

    // Input Action referencia
    public InputAction rightStickAction;
    public InputAction PlayerJump;
    public InputAction PlayerShoot; // Lövési akció


    void Start()
    {
        // Input action beállítása
        rightStickAction.Enable();
        PlayerJump.Enable();
        PlayerShoot.Enable(); // Lövési akció engedélyezése
        Cursor.lockState = CursorLockMode.Locked; // Az egérmutató rögzítése a képernyő közepére
        Cursor.visible = false; // Az egérmutató láthatatlanná tétele

        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Az új Input System használata a jobb analóg karhoz
        rightStickInput = rightStickAction.ReadValue<Vector2>();

        // Egér és kontroller bemenet kezelése
        HandleMouseLook();
        HandleMovement();
        HandleHeadbob();


    }

    void HandleMovement()
    {
        // Ellenőrizzük, hogy a játékos a földön van-e
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Mozgás inputok (WASD)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Futás és séta közti váltás
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);

        // Ugrás
        if (PlayerJump.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravitáció alkalmazása
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        // Egér input kezelése
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Kamera fel-le forgatása (X tengelyen korlátozva)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Kamera forgatás alkalmazása
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleHeadbob()
    {
        // Ha a játékos nem mozog, visszaállítjuk a kamera eredeti pozícióját
        if (controller.velocity.magnitude < 0.1f)
        {
            return;
        }
    }
	
	    //Erő alkalmazása a kisebb object-ek alrébb lökéséhez.
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Ellenőrizzük, hogy a player hozzáér-e egy bowling bábuhoz
        if (hit.collider.CompareTag("pin"))
        {
            Debug.Log("A player hozzáért egy bowling bábuhoz: " + hit.collider.name);

            // Hozzáférés a bábu Rigidbody komponenséhez
            Rigidbody pinRigidbody = hit.collider.attachedRigidbody;

            if (pinRigidbody != null)
            {
                // Erő hozzáadása a bábuhoz
                Vector3 pushDirection = hit.transform.position - transform.position;
                pushDirection.y = 0;  // Nem akarjuk, hogy felfelé irányuljon az erő
                pushDirection.Normalize();

                // Erő alkalmazása (itt az erő nagysága 5, ezt testre szabhatod)
                float pushForce = 0.5f;
                pinRigidbody.isKinematic = false;  // Gravitáció engedélyezése az ütközéskor
                pinRigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse);
            }

        }
		
		        // Ellenőrizzük, hogy a player hozzáér-e egy bowling bábuhoz
        if (hit.collider.CompareTag("pushdoor"))
        {
            Debug.Log("A player hozzáért egy bowling bábuhoz: " + hit.collider.name);

            // Hozzáférés a bábu Rigidbody komponenséhez
            Rigidbody pinRigidbody = hit.collider.attachedRigidbody;

            if (pinRigidbody != null)
            {
                // Erő hozzáadása a bábuhoz
                Vector3 pushDirection = hit.transform.position - transform.position;
                pushDirection.y = 0;  // Nem akarjuk, hogy felfelé irányuljon az erő
                pushDirection.Normalize();

                // Erő alkalmazása (itt az erő nagysága 5, ezt testre szabhatod)
                float pushForce = 0.1f;
                pinRigidbody.isKinematic = false;  // Gravitáció engedélyezése az ütközéskor
                pinRigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse);
            }

        }
    }
}