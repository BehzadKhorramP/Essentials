using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadApper.TileMap
{
    [CreateAssetMenu(fileName = "ScaleableRuleTile", menuName = "Tiles/ScaleableRuleTile")]
    public class RuleTileScaleable : RuleTile
    {
        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instantiatedGameObject)
        {
            var res = base.StartUp(position, tilemap, instantiatedGameObject);

            if (instantiatedGameObject != null)
            {
                var grid = tilemap.GetComponent<Tilemap>().layoutGrid;

                if (grid != null)
                {                   
                    var modScale = instantiatedGameObject.transform.localScale;

                    instantiatedGameObject.transform.localScale = new Vector3(
                       modScale.x * grid.cellSize.x,
                       modScale.y * grid.cellSize.y,
                       modScale.z * grid.cellSize.z
                    );
                }
            }

            return res;
        }
    }
}
