using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npc_controller : MonoBehaviour
{
    public GameObject character; // A kezelendõ karakter gyökér GameObject-je
    public Animator animator;    // Az Animator, amely az animációkat kezeli

    public Rigidbody[] ragdollRigidbodies; // A karakterben található összes Rigidbody
    public Collider[] ragdollColliders;    // A karakterben található összes Collider

    [Header("Main Collider")]
    public CapsuleCollider mainCapsuleCollider;

    [Header("Hitbox Layer Settings")]
    public int hitboxLayer; // a hitbox layer indexe
    public Collider[] hitboxColliders; // csak a hitbox layeren lévõ colliderek

    void Start()
    {
        // Rigidbody-k és Collider-ek keresése
        ragdollRigidbodies = character.GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = character.GetComponentsInChildren<Collider>(true);

        // Hitbox layeren lévõ colliderek összegyûjtése
        List<Collider> hitboxList = new List<Collider>();

        foreach (var col in ragdollColliders)
        {
            if (col != null && col.gameObject.layer == hitboxLayer)
            {
                hitboxList.Add(col);
            }
        }

        hitboxColliders = hitboxList.ToArray();

        // Main collider automatikus keresése
        if (mainCapsuleCollider == null)
        {
            mainCapsuleCollider = GetComponent<CapsuleCollider>();
        }

        // Alapállapot
        EnableRagdoll(false);
    }

    void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }

    public void EnableRagdoll(bool enable)
    {
        // Rigidbody kezelés
        foreach (var rb in ragdollRigidbodies)
        {
            // rb.isKinematic = !enable;
        }

        // Összes collider kezelése (ragdoll logika)
        foreach (var col in ragdollColliders)
        {
            // col.enabled = enable;
        }

        // HITBOX LAYER COLLIDEREK KIKAPCSOLÁSA HALÁL UTÁN
        foreach (var hitboxCol in hitboxColliders)
        {
            if (hitboxCol != null)
            {
                hitboxCol.enabled = !enable;
            }
        }

        // Main capsule collider kezelése
        // PLAYER TAG esetén NE kapcsoljuk ki
        if (mainCapsuleCollider != null)
        {
            if (!CompareTag("Player"))
            {
                mainCapsuleCollider.enabled = !enable;
            }
        }

        // Animator ki/be
        animator.enabled = !enable;
    }

    public void ActivateRagdoll()
    {
        EnableRagdoll(true);
    }

    public void DeactivateRagdoll()
    {
        EnableRagdoll(false);
    }
}