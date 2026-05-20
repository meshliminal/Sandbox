using System.Collections;
using UnityEngine;

public class Weapon_Glock : MonoBehaviour
{
    [Header("Weapon")]
    public float bulletRange = 50f;
    public float fireRate = 0.06f;
    public float raycastForce = 15f;
    public float reloadTime = 2f;
    public int magazineSize = 30;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 1000f;

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
    public float debrisForce = 2f;

    [Header("Animation")]
    public Animator animator;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextFireTime = 0f;

    void Start()
    {
        currentAmmo = magazineSize;
    }

    void Update()
    {
        if (isReloading)
            return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // AUTO FIRE

        if (Input.GetButton("Fire1") &&
            Time.time >= nextFireTime)
        {
            nextFireTime =
                Time.time + fireRate;

            ShootRaycast();

            // animator.SetTrigger("Shooting");
        }

        if (Input.GetButtonDown("Reload"))
        {
            StartCoroutine(Reload());
        }
    }

    void ShootRaycast()
    {
        bool aiming =
            tpsCamera != null &&
            tpsCamera.IsAiming();

        // M4/TPS spray

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

        Ray ray = new Ray(
            Camera.main.transform.position,

            Camera.main.transform.forward +
            sprayOffset
        );

        RaycastHit hit;

        int layerMask =
            ~LayerMask.GetMask(
                "Trigger"
            );

        Vector3 bulletDirection =
            Camera.main.transform.forward;

        if (Physics.Raycast(
            ray,
            out hit,
            bulletRange,
            layerMask
        ))
        {
            Debug.Log(
                "Eltalált objektum: " +
                hit.collider.gameObject.name
            );

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

        currentAmmo--;
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
            bulletRange /
            bulletSpeed
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

            ApplyDamageToNPC(
                rootParentObject,
                damage
            );

            SpawnSparks(
                hit.point,
                hit.normal
            );
        }
        else
        {
            Rigidbody hitRb =
                hit.collider
                .GetComponent<Rigidbody>();

            if (hitRb != null)
            {
                hitRb.AddForce(
                    bulletDirection *
                    raycastForce,

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
        else
        {
            Debug.LogError(
                "NPCHealth hiányzik!"
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

        Destroy(
            casing,
            5f
        );
    }

    IEnumerator Reload()
    {
        isReloading = true;

        yield return
            new WaitForSeconds(
                reloadTime
            );

        currentAmmo =
            magazineSize;

        isReloading = false;
    }
}