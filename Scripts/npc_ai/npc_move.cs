using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npc_move : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector3 inputDir = Vector3.zero;

        if (Input.GetKey(KeyCode.T))
            inputDir += Vector3.forward;

        if (Input.GetKey(KeyCode.G))
            inputDir += Vector3.back;

        if (Input.GetKey(KeyCode.F))
            inputDir += Vector3.left;

        if (Input.GetKey(KeyCode.H))
            inputDir += Vector3.right;

        bool isMoving = inputDir.magnitude > 0.1f;

        if (isMoving)
        {
            // MOZGÁS a karakter saját irányához képest
            Vector3 moveDir = transform.TransformDirection(inputDir);
            moveDir.y = 0;
            moveDir.Normalize();

            // Mozgatás
            transform.position += moveDir * moveSpeed * Time.deltaTime;

            // Forgás a mozgás irányába
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Animáció
            animator.SetBool("IsWalking", true);
            animator.SetBool("Idle", false);
        }
        else
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("Idle", true);
        }
    }
}