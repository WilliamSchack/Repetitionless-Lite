using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class RepetitionlessTerrain : MonoBehaviour
{
    [SerializeField] private Material _repetitionlessMaterial;
    public Material RepetitionlessMaterial { get { return _repetitionlessMaterial; } }

    private Terrain _terrain;
    public Terrain Terrain { get {
            if (_terrain == null)
                _terrain = GetComponent<Terrain>();
            
            return _terrain;
        }
    }

    public void UpdateTerrainMaterial(Material material)
    {
        Terrain.materialTemplate = material;
    }
}
