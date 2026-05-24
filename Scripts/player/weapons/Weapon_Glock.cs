using System.Collections;
using UnityEngine;

namespace sandbox
{
public class Weapon_Glock : MonoBehaviour
{
    [Header("Weapon")]
    public float bulletRange = 50f;

    public float fireRate = 0.08f;

    [Header("Bullet")]
    public GameObject bulletPrefab;
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

    [Header("Debug")]
    public LineRenderer debugLine;
    public float debugLineDuration = 0.05f;

    private float nextFireTime = 0f;

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            ShootRaycast();
        }
    }

    void ShootRaycast()
    {
        bool aiming =
            tpsCamera != null &&
            tpsCamera.IsAiming();

        float sprayFactor =
            aiming ? 0.0005f : 0.004f;

        Vector3 sprayOffset = new Vector3(
            Random.Range(-sprayFactor, sprayFactor),
            Random.Range(-sprayFactor, sprayFactor),
            Random.Range(-sprayFactor, sprayFactor)
        );

        Vector3 rayDir =
            (Camera.main.transform.forward + sprayOffset).normalized;

        Ray ray = new Ray(
            Camera.main.transform.position,
            rayDir
        );

        // 🔴 DEBUG LINE
        if (debugLine != null)
        {
            StartCoroutine(DrawDebugRay(ray.origin, rayDir * bulletRange));
        }

        Debug.DrawRay(ray.origin, ray.direction * bulletRange, Color.red, 0.2f);

        RaycastHit hit;

        int layerMask =
            ~LayerMask.GetMask(
                "Trigger",
                "disabled_ragdoll",
                "npc_controller"
            );

        Vector3 bulletDirection = Camera.main.transform.forward;

        if (Physics.Raycast(ray, out hit, bulletRange, layerMask))
        {
            bulletDirection = (hit.point - firePoint.position).normalized;

            HandleHit(hit, bulletDirection);
        }

        ShootBullet(bulletDirection);
        EjectCasing();

        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
    }

    IEnumerator DrawDebugRay(Vector3 start, Vector3 end)
    {
        debugLine.enabled = true;

        debugLine.SetPosition(0, start);
        debugLine.SetPosition(1, start + end);

        yield return new WaitForSeconds(debugLineDuration);

        debugLine.enabled = false;
    }

    void ShootBullet(Vector3 direction)
    {
        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0)
        );

        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction.normalized * bulletSpeed;
        }

        Destroy(bullet, 3f);
    }

    void HandleHit(RaycastHit hit, Vector3 bulletDirection)
    {
        int damage = 60;

        if (hit.collider.gameObject.name.ToLower() == "head")
        {
            damage = 100;
        }

        if (hit.collider.CompareTag("npc_controller"))
        {
            Transform currentParent = hit.collider.transform;

            while (currentParent.parent != null)
                currentParent = currentParent.parent;

            GameObject root = currentParent.gameObject;

            ApplyDamageToNPC(root, damage);

            Rigidbody[] allRigidbodies = root.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in allRigidbodies)
            {
                if (rb == null) continue;

                rb.linearVelocity *= 0.15f;
                rb.angularVelocity *= 0.15f;

                rb.maxAngularVelocity = 10f;
                rb.maxDepenetrationVelocity = 1f;

                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            Vector3 flatForceDirection = new Vector3(
                bulletDirection.x,
                0f,
                bulletDirection.z
            ).normalized;

            foreach (Rigidbody rb in allRigidbodies)
            {
                if (rb == null) continue;

                rb.AddForce(flatForceDirection * 2f, ForceMode.Impulse);
            }

            SpawnSparks(hit.point, hit.normal);
            return;
        }

        Rigidbody hitRb = hit.collider.GetComponent<Rigidbody>();

        if (hitRb != null)
        {
            hitRb.AddForce(bulletDirection * 2f, ForceMode.Impulse);
        }
        else
        {
            SpawnSmoke(hit.point, hit.normal);
            SpawnSparks(hit.point, hit.normal);
            SpawnDebris(hit.point, hit.normal);
        }
    }

    void ApplyDamageToNPC(GameObject npc, int damage)
    {
        NPCHealth hp = npc.GetComponent<NPCHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
        }
    }

    void SpawnDebris(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (debrisPrefab == null) return;

        GameObject obj = Instantiate(
            debrisPrefab,
            hitPoint + hitNormal * 0.01f,
            Quaternion.LookRotation(hitNormal)
        );

        Destroy(obj, 5f);
    }

    void SpawnSparks(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (sparkPrefab == null) return;

        GameObject obj = Instantiate(
            sparkPrefab,
            hitPoint + hitNormal * 0.01f,
            Quaternion.LookRotation(hitNormal)
        );

        Destroy(obj, 2f);
    }

    void SpawnSmoke(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (smokePrefab == null) return;

        GameObject obj = Instantiate(
            smokePrefab,
            hitPoint + hitNormal * 0.01f,
            Quaternion.LookRotation(hitNormal)
        );

        Destroy(obj, smokeDuration);
    }

    void EjectCasing()
    {
        if (casingPrefab == null || casingEjectPoint == null)
            return;

        GameObject casing = Instantiate(
            casingPrefab,
            casingEjectPoint.position,
            casingEjectPoint.rotation
        );

        Rigidbody rb = casing.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 ejectDir =
                casingEjectPoint.right +
                new Vector3(
                    Random.Range(-0.2f, 0.2f),
                    Random.Range(0.1f, 0.3f),
                    Random.Range(-0.2f, 0.2f)
                );

            rb.AddForce(ejectDir * 2f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }

        Destroy(casing, 5f);
    }
}
}