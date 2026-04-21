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

        void OnEnable()
        {
            // Sync material terrain layers in editor
            // Do this function on enable for all pinned tiles

            _main = GetComponent<MapMagicObject>();

            TerrainTile.OnTileFinalized += (TerrainTile tile, TileData data, StopToken stop) => { EditorApplication.delayCall += () => {
                if (tile == null)
                    return;

                Terrain mainTerrain = tile.main.terrain;
                mainTerrain.drawInstanced = false;

                RepetitionlessTerrain terrain;
                mainTerrain.TryGetComponent(out terrain);
                
                if (terrain == null) {
                    terrain = mainTerrain.gameObject.AddComponent<RepetitionlessTerrain>();

                    terrain.UpdateTerrainMaterial(_mat);
                    terrain.UpdateMaterialTerrainTextures();
                    return;
                }

                terrain.UpdateMaterialTerrainTextures();
            }; };
        }
    }
}