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
        public GameObject sparkPrefab;
        public GameObject casingPrefab;
        public Transform casingEjectPoint;

        public Animator animator;

        [Header("Debug")]
        public LineRenderer debugLine;
        public float debugLineDuration = 0.05f;

        [Header("Target Marker")]
        public GameObject targetMarker;

        private float nextFireTime = 0f;

        void Update()
        {
            UpdateAimPoint();

            if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                ShootRaycast();
            }
        }

        void UpdateAimPoint()
        {
            if (targetMarker == null) return;

            Ray baseRay =
                tpsCamera != null
                ? tpsCamera.GetAimRay()
                : Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            int layerMask =
                ~LayerMask.GetMask("Trigger", "disabled_ragdoll", "npc_controller");

            Vector3 point;

            if (Physics.Raycast(baseRay, out RaycastHit hit, bulletRange, layerMask))
            {
                point = hit.point;
            }
            else
            {
                point = baseRay.origin + baseRay.direction * bulletRange;
            }

            targetMarker.transform.position = point;
        }

        void ShootRaycast()
        {
            bool aiming = tpsCamera != null && tpsCamera.IsAiming();
            float sprayFactor = aiming ? 0.0005f : 0.004f;

            Ray baseRay =
                tpsCamera != null
                ? tpsCamera.GetAimRay()
                : Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            Vector3 dir = baseRay.direction;

            dir += new Vector3(
                Random.Range(-sprayFactor, sprayFactor),
                Random.Range(-sprayFactor, sprayFactor),
                Random.Range(-sprayFactor, sprayFactor)
            ) * 0.5f;

            Ray ray = new Ray(baseRay.origin, dir.normalized);

            int layerMask =
                ~LayerMask.GetMask("Trigger", "disabled_ragdoll", "npc_controller");

            Vector3 bulletDirection = ray.direction;

            if (Physics.Raycast(ray, out RaycastHit hit, bulletRange, layerMask))
            {
                bulletDirection =
                    (hit.point - firePoint.position).normalized;

                HandleHit(hit, bulletDirection);
            }

            ShootBullet(bulletDirection);
            EjectCasing();

            if (animator != null)
                animator.SetTrigger("Shoot");

            if (debugLine != null)
                StartCoroutine(DrawDebugRay(firePoint.position, bulletDirection * bulletRange));
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

            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            if (rb != null)
                rb.linearVelocity = direction.normalized * bulletSpeed;

            Destroy(bullet, 3f);
        }

        void HandleHit(RaycastHit hit, Vector3 bulletDirection)
        {
            int damage = 60;

            if (hit.collider.gameObject.name.ToLower() == "head")
                damage = 100;

            // NPC HIT
            if (hit.collider.CompareTag("npc_controller"))
            {
                Transform root = hit.collider.transform;

                while (root.parent != null)
                    root = root.parent;

                GameObject rootObject = root.gameObject;

                // DAMAGE
                NPCHealth hp = rootObject.GetComponent<NPCHealth>();

                if (hp != null)
                    hp.TakeDamage(damage);

                // ELSZÁLLÁS ELLENI VÉDELEM
                Rigidbody[] allRigidbodies =
                    rootObject.GetComponentsInChildren<Rigidbody>();

                foreach (Rigidbody body in allRigidbodies)
                {
                    if (body == null)
                        continue;

                    // Lassítás
                    body.linearVelocity *= 0.15f;
                    body.angularVelocity *= 0.15f;

                    // Stabil fizika
                    body.maxAngularVelocity = 10f;
                    body.maxDepenetrationVelocity = 1f;

                    body.interpolation =
                        RigidbodyInterpolation.Interpolate;

                    body.collisionDetectionMode =
                        CollisionDetectionMode.ContinuousDynamic;
                }

                // EFFECT
                if (sparkPrefab != null)
                {
                    GameObject obj = Instantiate(
                        sparkPrefab,
                        hit.point + hit.normal * 0.01f,
                        Quaternion.LookRotation(hit.normal)
                    );

                    Destroy(obj, 2f);
                }

                // NPC-re NINCS FORCE
                return;
            }

            // NORMAL OBJECT
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(
                    bulletDirection * 1f,
                    ForceMode.Impulse
                );
            }
            else
            {
                if (sparkPrefab != null)
                {
                    GameObject obj = Instantiate(
                        sparkPrefab,
                        hit.point + hit.normal * 0.01f,
                        Quaternion.LookRotation(hit.normal)
                    );

                    Destroy(obj, 2f);
                }
            }
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