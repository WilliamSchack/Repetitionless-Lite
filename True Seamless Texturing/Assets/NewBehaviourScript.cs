using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SeamlessMaterial.Compression;

public class NewBehaviourScript : MonoBehaviour
{
    private void Start()
    {
        bool[] bools = new bool[] {
            true,
            true,
            true,
            true,
            false,
            false
        };

        Debug.Log(BooleanCompression.CompressValues(bools));
    }
}
