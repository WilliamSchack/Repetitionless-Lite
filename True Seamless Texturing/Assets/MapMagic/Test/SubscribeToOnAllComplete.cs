using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubscribeToOnAllComplete : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        MapMagic.Terrains.TerrainTile.OnAllComplete += mmo => Debug.Log("All Complete");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
