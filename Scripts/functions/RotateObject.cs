using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 100, 0);  // Forgás sebessége (fok/másodperc)

    void Update()
    {
        // Forgatja az objektumot a megadott tengelyek körül
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
