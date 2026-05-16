using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public Transform target; // A követendő objektum (pl. másik GameObject Transform-ja)
    public Vector3 offset;   // Opcionális eltolás

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}