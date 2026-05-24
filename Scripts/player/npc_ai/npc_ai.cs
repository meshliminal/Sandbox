using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npc_ai : MonoBehaviour
{
    public Transform player; // A játékos transformja
    public float followSpeed = 3f; // Követés sebessége
    public float rotationSpeed = 5f; // Forgatás sebessége
    public float attackRange = 1f; // Támadási távolság
    public float attackCooldown = 1f; // Támadás utáni hûtési idõ
    public float maxFollowDistance = 10f; // Maximális követési távolság
    public float minDistanceBetweenNPCs = 2f; // Minimális távolság két NPC között
    public float gravity = 9.8f; // Gravitáció erõssége
    public LayerMask groundLayer; // A föld rétege

    private bool isAttacking = false; // Támadás folyamatban
    private bool isFollowing = true; // NPC követi a játékost
    private bool isGrounded; // Ellenõrzi, hogy a földön van-e
    private Animator animator; // Animációkhoz
    private Vector3 velocity; // Sebességvektor gravitációhoz

    public npc_controller ragdollController; // Ragdoll vezérlõ
    private NPCHealth npcHealth; // Életerõ script
    private float randomOffset; // Véletlenszerû mozgás offsetje
    private float randomRotationSpeed; // Véletlenszerû forgatási sebesség
    private List<npc_ai> allNPCs; // Az összes NPC referencia

    private int currentAttackPhase = 0;
    private int maxAttackPhases = 2; // Példa: három különbözõ támadás



    void Start()
    {
        animator = GetComponent<Animator>();
        npcHealth = GetComponent<NPCHealth>();
        allNPCs = new List<npc_ai>(FindObjectsOfType<npc_ai>());

        // Véletlenszerû offset és forgatási sebesség
        randomOffset = Random.Range(0f, 1f);
        randomRotationSpeed = Random.Range(2f, 5f);

        // Hibaellenõrzés
        if (animator == null) Debug.LogError("Az NPC nem rendelkezik Animator komponenssel!");
        if (npcHealth == null) Debug.LogError("Az NPC nem rendelkezik NPCHealth komponenssel!");
		
		
    }

    void Update()
    {
        if (npcHealth != null && npcHealth.currentHealth <= 0)
        {
            HandleDeath();
            return;
        }

        if (isFollowing) FollowPlayer();

        if (Vector3.Distance(transform.position, player.position) <= attackRange && !isAttacking)
        {
            StartCoroutine(Attack());
			
					if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f || currentAttackPhase == 0)
        {
            currentAttackPhase++;
            if (currentAttackPhase > maxAttackPhases)
            {
                currentAttackPhase = 1; // Újraindul az elsõ támadás
            }

            animator.SetFloat("Blend", currentAttackPhase);
            animator.SetTrigger("Attack");
        }
		
		
			
			
            isFollowing = false;
        }
        else if (!isAttacking && Vector3.Distance(transform.position, player.position) > attackRange)
        {
            isFollowing = true;
        }


    }

    void FollowPlayer()
    {
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 direction = targetPosition - transform.position;

        // Ha túl messze van a játékos, állítsuk le a követést
        if (direction.magnitude > maxFollowDistance)
        {
            isFollowing = false;
			
			
			animator.SetBool("IsWalking", false); 
			animator.SetBool("Idle", true);
			
			
			
            return;
        } else {
			            isFollowing = true;
				animator.SetBool("IsWalking", true); 
				animator.SetBool("Idle", false);
		}



        // Ellenõrizzük más NPC-k távolságát
        foreach (npc_ai otherNPC in allNPCs)
        {
            if (otherNPC != this)
            {
                float distanceToOtherNPC = Vector3.Distance(transform.position, otherNPC.transform.position);
                if (distanceToOtherNPC < minDistanceBetweenNPCs)
                {
                    Vector3 separationDirection = (transform.position - otherNPC.transform.position).normalized;
                    transform.position += separationDirection * followSpeed * Time.deltaTime;
                    return;
                }
            }
        }

        // Mozgás
        transform.position += direction.normalized * followSpeed * Time.deltaTime;

        // Forgatás
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, randomRotationSpeed * Time.deltaTime);
        }

        if (animator != null) 
			animator.SetBool("IsWalking", true);
    }



    IEnumerator Attack()
    {
        isAttacking = true;

        if (animator != null) animator.SetBool("IsWalking", false); 
		
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;

        if (Vector3.Distance(transform.position, player.position) > attackRange) isFollowing = true;
    }

    void HandleDeath()
    {
        if (ragdollController != null) ragdollController.ActivateRagdoll();
        if (animator != null) animator.enabled = false;


		
    }
}
