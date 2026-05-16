using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookUpAndReset : MonoBehaviour
{
    void Start()
    {
        // Leválasztjuk ideiglenesen a parentet, hogy a forgatás abszolút legyen
        Transform originalParent = transform.parent;
        transform.parent = null;

        // Beállítjuk, hogy az objektum felfelé "nézzen"
        // Alapvetően 'up' irányba nézzen a forward irány
        transform.rotation = Quaternion.LookRotation(Vector3.up);

        // Visszaállítjuk az eredeti parentet
        transform.parent = originalParent;
    }
}
