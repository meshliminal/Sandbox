using System.Collections;
using UnityEngine;

namespace sandbox
{
    public class Weapon_Glock : MonoBehaviour
    {
        [Header("Weapon")]
        public float bulletRange = 50f;
        public float fireRate = 0.08f;

        [Header("Ammo")]
        public int magazineSize = 12;
        public int currentAmmo = 12;

        [Header("Reload")]
        public float reloadTime = 1.5f;
        public KeyCode reloadKey = KeyCode.R;

        [Header("Bullet")]
        public GameObject bulletPrefab;
        public float bulletSpeed = 80f;

        [Header("References")]
        public Transform firePoint;
        public TPSCamera tpsCamera;

        [Header("IK")]
        public Transform IK_righthand;
        public float ikDistance = 2.5f;
        public float ikSmooth = 20f;

        [Header("IK Weight")]
        public float ikBlendSpeed = 10f;

        [Header("Effects")]
        public GameObject sparkPrefab;
        public GameObject bloodPrefab;
        public GameObject casingPrefab;
        public Transform casingEjectPoint;

        public Animator animator;

        [Header("Debug")]
        public LineRenderer debugLine;
        public float debugLineDuration = 0.05f;

        [Header("Target Marker")]
        public GameObject targetMarker;

        [Header("Stabilization")]
        public float shootOriginOffset = 0.3f;

        private float nextFireTime = 0f;

        private bool isReloading = false;
		public bool IsReloading => isReloading;
		
		
        void Start()
        {
            currentAmmo = magazineSize;
        }

        void Update()
        {
            UpdateAimPoint();

            if (
                Input.GetKeyDown(reloadKey) &&
                !isReloading
            )
            {
                StartCoroutine(ReloadRoutine());
            }

            if (isReloading)
                return;

            if (
                Input.GetButton("Fire1") &&
                Time.time >= nextFireTime
            )
            {
                if (currentAmmo <= 0)
                {
                    StartCoroutine(ReloadRoutine());
                    return;
                }

                nextFireTime = Time.time + fireRate;
                currentAmmo--;

                ShootRaycast();
            }
        }

        void LateUpdate()
        {
            UpdateIKRightHand();
        }

        IEnumerator ReloadRoutine()
        {
            isReloading = true;

            if (animator != null)
            {
                animator.SetBool("Reloading", true);
                animator.SetTrigger("Reload");
            }

            yield return new WaitForSeconds(reloadTime);

            currentAmmo = magazineSize;

            if (animator != null)
            {
                animator.SetBool("Reloading", false);
            }

            isReloading = false;
        }

        void UpdateIKRightHand()
        {
            if (IK_righthand == null)
                return;

            Transform root = transform.root;

            // RELOAD MODE: IK idle pozíció (de weight marad aktív)
            if (isReloading)
            {
                Vector3 idlePos =
                    root.position +
                    Vector3.up * 1.2f +
                    root.forward * 0.3f;

                IK_righthand.position = Vector3.Lerp(
                    IK_righthand.position,
                    idlePos,
                    Time.deltaTime * ikSmooth
                );

                IK_righthand.rotation = Quaternion.Lerp(
                    IK_righthand.rotation,
                    root.rotation,
                    Time.deltaTime * ikSmooth
                );

                return;
            }

            // NORMAL AIM IK
            Ray baseRay =
                tpsCamera != null
                ? tpsCamera.GetAimRay()
                : Camera.main.ViewportPointToRay(
                    new Vector3(0.5f, 0.5f, 0f)
                );

            Vector3 camDir = baseRay.direction.normalized;

            Vector3 characterForward =
                Vector3.ProjectOnPlane(root.forward, Vector3.up).normalized;

            Vector3 flatDir =
                Vector3.ProjectOnPlane(camDir, Vector3.up).normalized;

            if (flatDir.sqrMagnitude < 0.001f)
                flatDir = characterForward;

            float angle =
                Vector3.SignedAngle(characterForward, flatDir, Vector3.up);

            angle = Mathf.Clamp(angle, -85f, 85f);

            Vector3 orbitDir =
                Quaternion.AngleAxis(angle, Vector3.up) * characterForward;

            float vertical = Mathf.Clamp(camDir.y, -0.7f, 0.7f);

            Vector3 finalDir = (orbitDir + Vector3.up * vertical).normalized;

            Vector3 targetPos =
                root.position +
                Vector3.up * 1.4f +
                finalDir * ikDistance;

            Quaternion targetRot =
                Quaternion.LookRotation(finalDir);

            IK_righthand.position = Vector3.Lerp(
                IK_righthand.position,
                targetPos,
                Time.deltaTime * ikSmooth
            );

            IK_righthand.rotation = Quaternion.Lerp(
                IK_righthand.rotation,
                targetRot,
                Time.deltaTime * ikSmooth
            );
        }

        void UpdateAimPoint()
        {
            if (targetMarker == null || isReloading)
                return;

            Ray baseRay =
                tpsCamera != null
                ? tpsCamera.GetAimRay()
                : Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            int layerMask = ~LayerMask.GetMask("Trigger", "ragdoll", "npc_controller");

            Vector3 point;

            if (Physics.Raycast(baseRay, out RaycastHit hit, bulletRange, layerMask))
                point = hit.point;
            else
                point = baseRay.origin + baseRay.direction * bulletRange;

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

            dir.Normalize();

            Ray ray = new Ray(baseRay.origin, dir);

            int layerMask = ~LayerMask.GetMask("Trigger", "ragdoll", "npc_controller");

            Vector3 bulletDirection = dir;

            Vector3 stableShootOrigin =
                baseRay.origin + baseRay.direction * shootOriginOffset;

            if (Physics.Raycast(ray, out RaycastHit hit, bulletRange, layerMask))
            {
                bulletDirection = (hit.point - stableShootOrigin).normalized;
                HandleHit(hit, bulletDirection);
            }

            ShootBullet(bulletDirection);
            EjectCasing();

            if (animator != null)
                animator.SetTrigger("Shoot");

            if (debugLine != null)
                StartCoroutine(DrawDebugRay(stableShootOrigin, bulletDirection * bulletRange));
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
            Vector3 spawnPos = firePoint != null
                ? firePoint.position + firePoint.forward * 0.05f
                : transform.position;

            GameObject bullet = Instantiate(
                bulletPrefab,
                spawnPos,
                Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0)
            );

            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.linearVelocity = direction.normalized * bulletSpeed;
            }

            Destroy(bullet, 3f);
        }

        void HandleHit(RaycastHit hit, Vector3 bulletDirection)
        {
            int damage = 60;

            if (hit.collider.gameObject.name.ToLower() == "head")
                damage = 100;

            if (hit.collider.CompareTag("npc_controller"))
            {
                Transform root = hit.collider.transform;
                while (root.parent != null) root = root.parent;

                NPCHealth hp = root.GetComponent<NPCHealth>();
                if (hp != null) hp.TakeDamage(damage);

                Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>();

                foreach (var body in bodies)
                {
                    body.linearVelocity *= 0.15f;
                    body.angularVelocity *= 0.15f;
                }

                if (bloodPrefab != null)
                {
                    GameObject blood = Instantiate(
                        bloodPrefab,
                        hit.point + hit.normal * 0.01f,
                        Quaternion.LookRotation(hit.normal)
                    );

                    Destroy(blood, 3f);
                }

                return;
            }

            if (hit.rigidbody != null)
            {
                Rigidbody rb = hit.rigidbody;

                float massFactor = Mathf.Clamp(10f / Mathf.Max(rb.mass, 0.1f), 0.7f, 3f);

                rb.AddForce(bulletDirection * 10f * massFactor, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * 2f * massFactor, ForceMode.Impulse);
            }
            else if (sparkPrefab != null)
            {
                GameObject obj = Instantiate(
                    sparkPrefab,
                    hit.point + hit.normal * 0.01f,
                    Quaternion.LookRotation(hit.normal)
                );

                Destroy(obj, 2f);
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