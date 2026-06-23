#if MAPMAGIC2
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using MapMagic.Core;
using MapMagic.Products;
using MapMagic.Terrains;

namespace Repetitionless.Runtime.Integrations.MapMagic
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MapMagicObject))]
    public class RepetitionlessMapMagic : MonoBehaviour
    {
        [SerializeField] private Material _mat;
        private Material _defaultTerrainMaterial;

        private MapMagicObject _main;

        private void OnEnable()
        {
            // Sync material terrain layers in editor
            // Do this function on enable for all pinned tiles

            _main = GetComponent<MapMagicObject>();
            _defaultTerrainMaterial = DefaultTerrainMaterial();

            TerrainTile.OnTileApplied -= OnTileApplied;
            TerrainTile.OnTileApplied += OnTileApplied;

            foreach (TerrainTile tile in _main.tiles.pinned.Values) {
                SetupTile(tile);
            }
        }

        private void OnDisable()
        {
            TerrainTile.OnTileApplied -= OnTileApplied;

#if UNITY_EDITOR
            // Disable all repetitionless terrains and reset materials
            foreach (TerrainTile tile in _main.tiles.pinned.Values) {
                DisableTile(tile);
            }
#endif
        }

#if UNITY_EDITOR
        private void OnDestroy()
        {
            // Remove all repetitionless terrain components
            // OnDisable handles the rest and is called before this
            foreach (TerrainTile tile in _main.tiles.pinned.Values) {
                RemoveTile(tile);
            }
        }
#endif

        private Material DefaultTerrainMaterial()
        {
            // PROPERLY IMPLEMENT ME, TEST BIRP, URP, HDRP

            return new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
        }

        // If terrain layers are not the same, sync up if in the editor

        private void OnTileApplied(TerrainTile tile, TileData data, StopToken stopToken)
        {
            SetupTile(tile);
        }

        private void SetupTile(TerrainTile tile)
        {
            if (tile == null || !tile.transform.IsChildOf(transform))
                    return;

            Terrain[] terrains = {
                tile.main.terrain,
                tile.draft.terrain
            };

            foreach (Terrain terrain in terrains) {
                //terrain.drawInstanced = false;

                RepetitionlessTerrain repetitionlessTerrain;
                terrain.TryGetComponent(out repetitionlessTerrain);
                
                if (repetitionlessTerrain == null) {
                    repetitionlessTerrain = terrain.gameObject.AddComponent<RepetitionlessTerrain>();

                    repetitionlessTerrain.UpdateTerrainMaterial(_mat);
                }

                repetitionlessTerrain.enabled = true;
                repetitionlessTerrain.UpdateMaterialTerrainTextures();
                repetitionlessTerrain.AssignMaterialInstance();
            }
        }

        private void DisableTile(TerrainTile tile)
        {
            if (tile == null || !tile.transform.IsChildOf(transform))
                    return;

            Terrain[] terrains = {
                tile.main.terrain,
                tile.draft.terrain
            };

            foreach (Terrain terrain in terrains) {
                terrain.materialTemplate = _defaultTerrainMaterial;

                RepetitionlessTerrain repetitionlessTerrain;
                terrain.gameObject.TryGetComponent(out repetitionlessTerrain);

                if (repetitionlessTerrain == null)
                    continue;

                repetitionlessTerrain.enabled = false;
            }
        }

        private void RemoveTile(TerrainTile tile)
        {
            if (tile == null || !tile.transform.IsChildOf(transform))
                    return;

            Terrain[] terrains = {
                tile.main.terrain,
                tile.draft.terrain
            };

            foreach (Terrain terrain in terrains) {
                RepetitionlessTerrain repetitionlessTerrain;
                terrain.gameObject.TryGetComponent(out repetitionlessTerrain);

                if (repetitionlessTerrain == null)
                    continue;

                DestroyImmediate(repetitionlessTerrain);
            }
        }
    }
}

#endif