using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Glock : MonoBehaviour
{
    public float bulletRange = 50f;
    public float fireRate = 0.15f;
    public float raycastForce = 15f;
    public float reloadTime = 2f;
    public int magazineSize = 17;

    public Transform firePoint;

    public GameObject decalPrefab;
    public float decalDuration = 2f;
    public float randomRotationRange = 10f;

    public GameObject casingPrefab;
    public Transform casingEjectPoint;

    public GameObject debrisPrefab;
    public GameObject sparkPrefab;
    public float debrisForce = 2f;

    public GameObject smokePrefab;
    public float smokeDuration = 3f;

    public GameObject bulletPrefab;
    public float bulletSpeed = 1000f;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextFireTime = 0f;

    public Animator animator;


    void Start()
    {
        currentAmmo = magazineSize;
    }


    void Update()
    {
        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
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
        float sprayFactor = 0.01f;

        Vector3 sprayOffset = new Vector3(
            Random.Range(-sprayFactor, sprayFactor),
            Random.Range(-sprayFactor, sprayFactor),
            Random.Range(-sprayFactor, sprayFactor)
        );

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward + sprayOffset);
        RaycastHit hit;

        int layerMask = ~LayerMask.GetMask("Trigger");

        Vector3 bulletDirection = Camera.main.transform.forward;

        if (Physics.Raycast(ray, out hit, bulletRange, layerMask))
        {
            Debug.Log("Eltalált objektum: " + hit.collider.gameObject.name);

            bulletDirection = (hit.point - firePoint.position).normalized;

            HandleHit(hit, bulletDirection);
        }

        ShootBullet(bulletDirection);
        EjectCasing();

        currentAmmo--;
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

        Destroy(bullet, bulletRange / bulletSpeed);
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

            GameObject rootParentObject = currentParent.gameObject;

            ApplyDamageToNPC(rootParentObject, damage);

            SpawnSparks(hit.point, hit.normal);
        }
        else
        {
            Rigidbody hitRb = hit.collider.GetComponent<Rigidbody>();

            if (hitRb != null)
            {
                hitRb.AddForce(bulletDirection * raycastForce, ForceMode.Impulse);
            }
            else
            {
                //ShowDecal(hit.point, hit.normal, hit.collider);
                SpawnSmoke(hit.point, hit.normal);
                SpawnSparks(hit.point, hit.normal);
                SpawnDebris(hit.point, hit.normal);
            }
        }
    }


    void ApplyDamageToNPC(GameObject npc, int damage)
    {
        var npcHealth = npc.GetComponent<NPCHealth>();

        if (npcHealth != null)
        {
            npcHealth.TakeDamage(damage);
        }
        else
        {
            Debug.LogError("NPCHealth hiányzik!");
        }
    }


    void ShowDecal(Vector3 hitPoint, Vector3 hitNormal, Collider hitCollider)
    {
        float offset = 0.01f;
        Vector3 decalPosition = hitPoint + hitNormal * offset;

        float randomRoll = Random.Range(0f, 360f);
        Quaternion decalRotation = Quaternion.LookRotation(hitNormal) * Quaternion.Euler(180f, 0f, randomRoll);

        GameObject decal = Instantiate(decalPrefab, decalPosition, decalRotation);
        Destroy(decal, decalDuration);
    }


    void SpawnDebris(Vector3 hitPoint, Vector3 hitNormal)
    {
        float offset = 0.01f;
        Vector3 debrisPosition = hitPoint + hitNormal * offset;

        GameObject debris = Instantiate(debrisPrefab, debrisPosition, Quaternion.LookRotation(hitNormal));
        Destroy(debris, 5f);
    }


    void SpawnSparks(Vector3 hitPoint, Vector3 hitNormal)
    {
        float offset = 0.01f;
        Vector3 sparkPosition = hitPoint + hitNormal * offset;

        GameObject sparks = Instantiate(sparkPrefab, sparkPosition, Quaternion.LookRotation(hitNormal));
        Destroy(sparks, 2f);
    }


    void SpawnSmoke(Vector3 hitPoint, Vector3 hitNormal)
    {
        float offset = 0.01f;
        Vector3 smokePosition = hitPoint + hitNormal * offset;

        GameObject smoke = Instantiate(smokePrefab, smokePosition, Quaternion.LookRotation(hitNormal));
        Destroy(smoke, smokeDuration);
    }


    void EjectCasing()
    {
        GameObject casing = Instantiate(casingPrefab, casingEjectPoint.position, casingEjectPoint.rotation);
        Rigidbody casingRb = casing.GetComponent<Rigidbody>();

        Vector3 ejectDir = casingEjectPoint.right + new Vector3(
            Random.Range(-0.2f, 0.2f),
            Random.Range(0.1f, 0.3f),
            Random.Range(-0.2f, 0.2f)
        );

        casingRb.AddForce(ejectDir * 2f, ForceMode.Impulse);
        casingRb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

        Destroy(casing, 5f);
    }


    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;
    }
}
