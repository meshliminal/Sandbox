using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake_Trigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
		FindObjectOfType<CameraShake>().Shake();
    }
}
