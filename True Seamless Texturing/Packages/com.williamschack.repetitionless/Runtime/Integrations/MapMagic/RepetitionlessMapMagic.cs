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

        private MapMagicObject _main;

        private void OnEnable()
        {
            // Sync material terrain layers in editor
            // Do this function on enable for all pinned tiles

            _main = GetComponent<MapMagicObject>();

            TerrainTile.OnTileApplied -= OnTileApplied;
            TerrainTile.OnTileApplied += OnTileApplied;

            foreach (TerrainTile tile in _main.tiles.pinned.Values) {
                SetupTile(tile);
            }
        }

        private void OnDisable()
        {
            TerrainTile.OnTileApplied -= OnTileApplied;
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
                    repetitionlessTerrain.UpdateMaterialTerrainTextures();
                    continue;
                }

                repetitionlessTerrain.UpdateMaterialTerrainTextures();
            }
        }
    }
}