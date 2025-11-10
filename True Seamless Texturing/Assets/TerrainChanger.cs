using UnityEngine;

public class TerrainChanger : MonoBehaviour
{
    [SerializeField] public Terrain _terrain;
    [SerializeField] public TerrainData _newData;

    [ContextMenu("CHANGE")]
    void Change()
    {
        _terrain.terrainData = _newData;
    }
}
