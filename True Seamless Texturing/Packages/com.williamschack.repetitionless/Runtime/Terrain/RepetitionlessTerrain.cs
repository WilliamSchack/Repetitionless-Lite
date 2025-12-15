using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

// Set the control and holes render textures to the material directly
// Better than trying to fiddle with saving the textures
// Also allows me to instance the material and have different textures for different terrains

// Updates textures when:
// OnEnable (Runtime & Editor)
// Scene saved
// Undo/Redo

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
public class RepetitionlessTerrain : MonoBehaviour
{
    private const int CONTROL_TEXTURE_COUNT = 8;

    [SerializeField] private Material _mainMaterial;
    public Material MainMaterial { get { return _mainMaterial; } }
    
    private Material _materialInstance;
    public Material MaterialInstance { get { return _materialInstance; } }

    public bool AutoSaveTextures = true;

    private Terrain _terrain;
    public Terrain Terrain { get {
            if (_terrain == null)
                _terrain = GetComponent<Terrain>();
            
            return _terrain;
        }
    }

    private TerrainData _terrainData {
        get {
            return Terrain.terrainData;
        }
    }

    [SerializeField] public RepetitionlessTerrain ParentTerrain;

#if UNITY_EDITOR
    // Set this as dirty so saving after doing nothing will still update the textures
    private void SetDirty()
    {
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(Terrain);
        EditorUtility.SetDirty(_terrainData);
    }

    private void OnSceneSaved(Scene scene)
    {
        UpdateMaterialTerrainTextures();
    }
#endif

    void OnEnable()
    {
#if UNITY_EDITOR
        EditorSceneManager.sceneSaved -= OnSceneSaved;
        EditorSceneManager.sceneSaved += OnSceneSaved;

        // Set dirty and update textures on next editor frame
        // For some reason doing it immediately doesnt work
        EditorApplication.delayCall += () => {
            SetDirty();
            UpdateMaterialTerrainTextures();
        };
#else
        UpdateMaterialTerrainTextures();
#endif
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        EditorSceneManager.sceneSaved -= OnSceneSaved;
#endif
    }

    public void UpdateTerrainMaterial(Material material)
    {
        _mainMaterial = material;

        // Use an instance to support multiple terrain objects
        if (_materialInstance != null)
            DestroyImmediate(_materialInstance);

        _materialInstance = new Material(_mainMaterial);
        _materialInstance.name += " (Instance)";
        _materialInstance.CopyPropertiesFromMaterial(_mainMaterial);

        _terrain.materialTemplate = _materialInstance;
    }

    // Update control and holes textures
    public void UpdateMaterialTerrainTextures()
    {
        // this == null to prevent error on end of build
        if (_mainMaterial == null || this == null)
            return;

        if (_materialInstance == null || _terrain.materialTemplate == null)
            UpdateTerrainMaterial(_mainMaterial);

        // Control textures 2-8 are not exposed in the shader graph
        // May aswell also set holes while we are here
        UpdateLayersCount();
        UpdateControlTextures();
        UpdateHolesTexture();

#if UNITY_EDITOR
        SetDirty();
#endif
    }

    public void UpdateLayersCount()
    {
        _materialInstance.SetFloat("_LayersCount", _terrainData.alphamapLayers);
    }

    public void UpdateControlTextures()
    {
        int controlTextureCount = Mathf.CeilToInt(_terrainData.alphamapLayers / 4.0f);
        for (int i = 0; i < CONTROL_TEXTURE_COUNT; i++) {
            Texture2D controlTexture = controlTextureCount > i ? _terrainData.alphamapTextures[i] : null;
            _materialInstance.SetTexture($"_Control{i}", controlTexture);
        }
    }

    public void UpdateHolesTexture()
    {
        _materialInstance.SetTexture("_TerrainHoles", _terrainData.holesTexture);
    }

    public void SetupNewTerrainNeighbour(Terrain newNeighbour)
    {
        RepetitionlessTerrain repetitionlessTerrain;
        newNeighbour.TryGetComponent<RepetitionlessTerrain>(out repetitionlessTerrain);

        // Already setup
        if (repetitionlessTerrain != null && repetitionlessTerrain.ParentTerrain == this)
            return;

        // Create terrain component
        if (repetitionlessTerrain == null)
            repetitionlessTerrain = newNeighbour.gameObject.AddComponent<RepetitionlessTerrain>();

        // Set parent
        RepetitionlessTerrain newParent = this;
        if (ParentTerrain != null)
            newParent = ParentTerrain;
        
        repetitionlessTerrain.ParentTerrain = newParent;

        // Set terrain layers and material
        repetitionlessTerrain.Terrain.terrainData.terrainLayers = _terrainData.terrainLayers;
        repetitionlessTerrain.AutoSaveTextures = false;
        repetitionlessTerrain.UpdateTerrainMaterial(_mainMaterial);
    }
}