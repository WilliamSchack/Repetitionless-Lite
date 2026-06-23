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
        [SerializeField] private Material _mainMaterial;
        public Material MainMaterial => _mainMaterial;

        public bool AutoSaveTextures = true;

        private Material _defaultTerrainMaterial;

        private MapMagicObject _mapMagicObject;

        private void OnEnable()
        {
            // Sync material terrain layers in editor
            // Do this function on enable for all pinned tiles

            _mapMagicObject = GetComponent<MapMagicObject>();
            _defaultTerrainMaterial = DefaultTerrainMaterial();

            TerrainTile.OnTileApplied -= OnTileApplied;
            TerrainTile.OnTileApplied += OnTileApplied;

            SetupAllTiles();
        }

        private void OnDisable()
        {
            TerrainTile.OnTileApplied -= OnTileApplied;

#if UNITY_EDITOR
            DisableAllTiles();
#endif
        }

#if UNITY_EDITOR
        private void OnDestroy()
        {
            // OnDisable handles the rest and is called before this
            RemoveAllTiles();
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

        private void SetupAllTiles()
        {
            foreach (TerrainTile tile in _mapMagicObject.tiles.grid.Values) {
                SetupTile(tile);
            }
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
                }

                repetitionlessTerrain.enabled = true;

                if (_mainMaterial == null)
                    continue;

                if (repetitionlessTerrain.MainMaterial == null) {
                    repetitionlessTerrain.UpdateTerrainMaterial(_mainMaterial);
                }

                repetitionlessTerrain.UpdateMaterialTerrainTextures();
                repetitionlessTerrain.AssignMaterialInstance();
            }
        }

        private void DisableAllTiles()
        {
            // Disable all repetitionless terrains and reset materials
            foreach (TerrainTile tile in _mapMagicObject.tiles.grid.Values) {
                DisableTile(tile);
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

        public void RemoveAllTilesMaterials()
        {
            // Remove all repetitionless terrain components
            foreach (TerrainTile tile in _mapMagicObject.tiles.grid.Values) {
                RemoveTileMaterials(tile);
            }
        }

        private void RemoveTileMaterials(TerrainTile tile)
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

                repetitionlessTerrain.UpdateTerrainMaterial(null, true);
                terrain.materialTemplate = _defaultTerrainMaterial;
            }
        }

        private void RemoveAllTiles()
        {
            // Remove all repetitionless terrain components
            foreach (TerrainTile tile in _mapMagicObject.tiles.grid.Values) {
                RemoveTile(tile);
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

        public void UpdateMaterialTerrainTextures()
        {
            // Call the same function on each RepetitionlessTerrain
            foreach (TerrainTile tile in _mapMagicObject.tiles.grid.Values) {
                if (tile == null)
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
                        
                    repetitionlessTerrain.UpdateMaterialTerrainTextures();
                }
            }
        }

        public void UpdateTerrainMaterials(Material material, bool assignMaterial = true)
        {
            // Call the same function on each RepetitionlessTerrain
            foreach (TerrainTile tile in _mapMagicObject.tiles.grid.Values) {
                if (tile == null)
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
                        
                    repetitionlessTerrain.UpdateTerrainMaterial(material, assignMaterial);
                }
            }
        }

#if UNITY_EDITOR
        public void CheckAndUpdateMaterials()
        {
            // Check one terrain, if its material has changed, we can assume all terrains have and reapply for all terrains
            if(_mapMagicObject.tiles.grid.Count == 0 || _mainMaterial == null)
                return;

            // Get the first tile out of the grid
            TerrainTile checkingTile = null;
            foreach (var kvp in _mapMagicObject.tiles.grid) {
                checkingTile = kvp.Value;
                break;
            }
            if (checkingTile == null) return; // Shouldnt happen

            Terrain mainTerrain = checkingTile.main.terrain;
            RepetitionlessTerrain repetitionlessTerrain;
            mainTerrain.gameObject.TryGetComponent(out repetitionlessTerrain);
            if (repetitionlessTerrain == null) return;

            if (mainTerrain.materialTemplate == repetitionlessTerrain.MaterialInstance)
                return;

            // Update material for all terrains
            foreach (TerrainTile tile in _mapMagicObject.tiles.grid.Values) {
                SetupTile(tile);
            }
        }
#endif
    }
}