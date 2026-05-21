using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npc_controller : MonoBehaviour
{
    public GameObject character; // A kezelendõ karakter gyökér GameObject-je
    public Animator animator;    // Az Animator, amely az animációkat kezeli

    public Rigidbody[] ragdollRigidbodies; // A karakterben található összes Rigidbody
    public Collider[] ragdollColliders;    // A karakterben található összes Collider

    void Start()
    {
        // Keressük a Rigidbody-kat és Collider-eket a karakter gyerekobjektumai között
        ragdollRigidbodies = character.GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = character.GetComponentsInChildren<Collider>(true);

        // Alapértelmezett állapot: ragdoll kikapcsolva
        EnableRagdoll(false);
    }

    void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }

    public void EnableRagdoll(bool enable)
    {
        // Rigidbody-k kinematikus állapotának kezelése
        foreach (var rb in ragdollRigidbodies)
        {
            // rb.isKinematic = !enable;
        }

        // Collider-ek engedélyezése/kikapcsolása
        foreach (var col in ragdollColliders)
        {
            //col.enabled = enable;
        }

        // Animator állapota
        animator.enabled = !enable;
    }

    public void ActivateRagdoll()
    {
        //Debug.Log("Ragdoll Activated"); 
        EnableRagdoll(true); 
    }

    public void DeactivateRagdoll()
    {
        EnableRagdoll(false);
    }
}