using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npc_controller : MonoBehaviour
{
    [Header("Character")]
    public GameObject character;
    public Animator animator;

    [Header("Ragdoll")]
    public Rigidbody[] ragdollRigidbodies;
    public Collider[] ragdollColliders;

    [Header("Main Collider")]
    public CapsuleCollider mainCapsuleCollider;

    [Header("Hitbox Layer Settings")]
    public int hitboxLayer;
    public Collider[] hitboxColliders;

    [Header("Player Settings")]
    public bool isPlayer = false; // Ha ez player, akkor nem kapcsoljuk ki a fõ collidert

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
            if (rb != null)
            {
                rb.isKinematic = !enable;
            }
        }

        // Ragdoll colliderek kezelése
        foreach (var col in ragdollColliders)
        {
            if (col != null)
            {
                // A main colliderhez ne nyúljon itt
                if (mainCapsuleCollider != null && col == mainCapsuleCollider)
                    continue;

                col.enabled = enable;
            }
        }

        // HITBOX colliderek kikapcsolása halál után
        foreach (var hitboxCol in hitboxColliders)
        {
            if (hitboxCol != null)
            {
                hitboxCol.enabled = !enable;
            }
        }

        // Main capsule collider kezelése
        if (mainCapsuleCollider != null)
        {
            // NPC esetén kikapcsoljuk
            // Player esetén bekapcsolva marad
            if (!isPlayer)
            {
                mainCapsuleCollider.enabled = !enable;
            }
            else
            {
                mainCapsuleCollider.enabled = true;
            }
        }

        // Animator ki/be
        if (animator != null)
        {
            animator.enabled = !enable;
        }
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