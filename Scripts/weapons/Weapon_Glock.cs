using UnityEngine;

public class Weapon_Glock : MonoBehaviour
{
    [Header("Weapon")]
    public float bulletRange = 50f;

    // Brutál gyors fire rate
    public float fireRate = 0.005f;

    [Header("Bullet")]
    public GameObject bulletPrefab;

    // Lassabb bullet hogy látszódjon
    public float bulletSpeed = 80f;

    [Header("References")]
    public Transform firePoint;
    public TPSCamera tpsCamera;

    [Header("Effects")]
    public GameObject decalPrefab;
    public float decalDuration = 2f;

    public GameObject casingPrefab;
    public Transform casingEjectPoint;

    public GameObject debrisPrefab;
    public GameObject sparkPrefab;
    public GameObject smokePrefab;

    public float smokeDuration = 3f;

    [Header("Animation")]
    public Animator animator;

    private float nextFireTime = 0f;

    void Update()
    {
        if (
            Input.GetButton("Fire1") &&
            Time.time >= nextFireTime
        )
        {
            nextFireTime =
                Time.time + fireRate;

            ShootRaycast();
        }
    }

    void ShootRaycast()
    {
        bool aiming =
            tpsCamera != null &&
            tpsCamera.IsAiming();

        float sprayFactor =
            aiming
            ? 0.0005f
            : 0.004f;

        Vector3 sprayOffset =
            new Vector3(
                Random.Range(
                    -sprayFactor,
                    sprayFactor
                ),

                Random.Range(
                    -sprayFactor,
                    sprayFactor
                ),

                Random.Range(
                    -sprayFactor,
                    sprayFactor
                )
            );

        Ray ray =
            new Ray(
                Camera.main.transform.position,

                Camera.main.transform.forward +
                sprayOffset
            );

        Debug.DrawRay(
            ray.origin,
            ray.direction * bulletRange,
            Color.red,
            0.2f
        );

        RaycastHit hit;

        int layerMask =
            ~LayerMask.GetMask(
                "Trigger",
                "disabled_ragdoll",
                "npc_controller"
            );

        Vector3 bulletDirection =
            Camera.main.transform.forward;

        if (
            Physics.Raycast(
                ray,
                out hit,
                bulletRange,
                layerMask
            )
        )
        {
            bulletDirection =
                (
                    hit.point -
                    firePoint.position
                ).normalized;

            HandleHit(
                hit,
                bulletDirection
            );
        }

        ShootBullet(
            bulletDirection
        );

        EjectCasing();
    }

    void ShootBullet(
        Vector3 direction
    )
    {
        GameObject bullet =
            Instantiate(
                bulletPrefab,
                firePoint.position,

                Quaternion.LookRotation(
                    direction
                ) *
                Quaternion.Euler(
                    90,
                    0,
                    0
                )
            );

        Rigidbody bulletRb =
            bullet.GetComponent<Rigidbody>();

        if (bulletRb != null)
        {
            bulletRb.linearVelocity =
                direction.normalized *
                bulletSpeed;
        }

        Destroy(
            bullet,
            3f
        );
    }

    void HandleHit(
        RaycastHit hit,
        Vector3 bulletDirection
    )
    {
        int damage = 60;

        if (
            hit.collider.gameObject.name
            .ToLower() == "head"
        )
        {
            damage = 100;
        }

        // NPC HIT
        if (
            hit.collider.CompareTag(
                "npc_controller"
            )
        )
        {
            Transform currentParent =
                hit.collider.transform;

            while (
                currentParent.parent != null
            )
            {
                currentParent =
                    currentParent.parent;
            }

            GameObject rootParentObject =
                currentParent.gameObject;

            // NPC sebzés
            ApplyDamageToNPC(
                rootParentObject,
                damage
            );

            // Ragdoll stabilizálás
            Rigidbody[] allRigidbodies =
                rootParentObject
                .GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in allRigidbodies)
            {
                if (rb == null)
                    continue;

                // Ne repüljön el
                rb.linearVelocity *= 0.15f;
                rb.angularVelocity *= 0.15f;

                // Stabilabb fizika
                rb.maxAngularVelocity = 10f;
                rb.maxDepenetrationVelocity = 1f;

                rb.interpolation =
                    RigidbodyInterpolation.Interpolate;

                rb.collisionDetectionMode =
                    CollisionDetectionMode.ContinuousDynamic;
            }

            SpawnSparks(
                hit.point,
                hit.normal
            );

            // NPC-re NINCS force
            return;
        }

        // NEM NPC object
        Rigidbody hitRb =
            hit.collider
            .GetComponent<Rigidbody>();

        if (hitRb != null)
        {
            hitRb.AddForce(
                bulletDirection * 2f,
                ForceMode.Impulse
            );
        }
        else
        {
            SpawnSmoke(
                hit.point,
                hit.normal
            );

            SpawnSparks(
                hit.point,
                hit.normal
            );

            SpawnDebris(
                hit.point,
                hit.normal
            );
        }
    }

    void ApplyDamageToNPC(
        GameObject npc,
        int damage
    )
    {
        NPCHealth npcHealth =
            npc.GetComponent<NPCHealth>();

        if (npcHealth != null)
        {
            npcHealth.TakeDamage(
                damage
            );
        }
    }

    void SpawnDebris(
        Vector3 hitPoint,
        Vector3 hitNormal
    )
    {
        float offset = 0.01f;

        Vector3 debrisPosition =
            hitPoint +
            hitNormal * offset;

        GameObject debris =
            Instantiate(
                debrisPrefab,
                debrisPosition,

                Quaternion.LookRotation(
                    hitNormal
                )
            );

        Destroy(
            debris,
            5f
        );
    }

    void SpawnSparks(
        Vector3 hitPoint,
        Vector3 hitNormal
    )
    {
        float offset = 0.01f;

        Vector3 sparkPosition =
            hitPoint +
            hitNormal * offset;

        GameObject sparks =
            Instantiate(
                sparkPrefab,
                sparkPosition,

                Quaternion.LookRotation(
                    hitNormal
                )
            );

        Destroy(
            sparks,
            2f
        );
    }

    void SpawnSmoke(
        Vector3 hitPoint,
        Vector3 hitNormal
    )
    {
        float offset = 0.01f;

        Vector3 smokePosition =
            hitPoint +
            hitNormal * offset;

        GameObject smoke =
            Instantiate(
                smokePrefab,
                smokePosition,

                Quaternion.LookRotation(
                    hitNormal
                )
            );

        Destroy(
            smoke,
            smokeDuration
        );
    }

    void EjectCasing()
    {
        GameObject casing =
            Instantiate(
                casingPrefab,
                casingEjectPoint.position,
                casingEjectPoint.rotation
            );

        Rigidbody casingRb =
            casing.GetComponent<Rigidbody>();

        if (casingRb != null)
        {
            Vector3 ejectDir =
                casingEjectPoint.right +

                new Vector3(
                    Random.Range(
                        -0.2f,
                        0.2f
                    ),

                    Random.Range(
                        0.1f,
                        0.3f
                    ),

                    Random.Range(
                        -0.2f,
                        0.2f
                    )
                );

            casingRb.AddForce(
                ejectDir * 2f,
                ForceMode.Impulse
            );

            casingRb.AddTorque(
                Random.insideUnitSphere *
                5f,
                ForceMode.Impulse
            );
        }

        Destroy(
            casing,
            5f
        );
    }
}