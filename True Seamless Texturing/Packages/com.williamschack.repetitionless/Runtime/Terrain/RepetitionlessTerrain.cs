using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

// Update material textures to terrain render textures whenever they are removed
// Honestly the best way i found to do since writing them to textures would be sluggish
// Plus the terrain is already handling these may aswell use them

// Updates textures when:
// OnEnable
// Scene saved

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
public class RepetitionlessTerrain : MonoBehaviour
{
    private const int CONTROL_TEXTURE_COUNT = 8;

    [SerializeField] private Material _repetitionlessMaterial;
    public Material RepetitionlessMaterial { get { return _repetitionlessMaterial; } }

    // Not used in this file but used by the editor in a SerializedProperty
    // Disabling warnings here to prevent unused variable warning
#pragma warning disable 0414
    [SerializeField] private bool _autoSaveTextures = true;
#pragma warning restore 0414

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
        Terrain.materialTemplate = material;
    }

    // Update control and holes textures
    public void UpdateMaterialTerrainTextures()
    {
        // this == null to prevent error on end of build
        if (_repetitionlessMaterial == null || this == null)
            return;

        UpdateControlTextures();
        UpdateHolesTexture();

#if UNITY_EDITOR
        SetDirty();
#endif
    }

    public void UpdateControlTextures()
    {
        int controlTextureCount = Mathf.CeilToInt(_terrainData.alphamapLayers / 4.0f);
        for (int i = 0; i < CONTROL_TEXTURE_COUNT; i++) {
            Texture2D controlTexture = controlTextureCount > i ? _terrainData.alphamapTextures[i] : null;
            _repetitionlessMaterial.SetTexture($"_Control{i}", controlTexture);
        }
    }

    public void UpdateHolesTexture()
    {
        _repetitionlessMaterial.SetTexture("_TerrainHoles", _terrainData.holesTexture);
    }
}
