using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MadApper.Grid;

namespace MadApper.TileMap
{


    public class TileMapperTest : SerializedMonoBehaviour
    {
        [SerializeField] string m_UID = "GridTile";
        [SerializeField] GridCoord m_GridCoord = GridCoord.XY;
        [SerializeField] Vector3 m_Offset;
       
        [SerializeField] RuleTile m_RuleTile;
        [SerializeField] RuleTile m_RuleTileBorder;
      

        [Space(10), TableMatrix(SquareCells = true)]
        public bool[,] m_Matrix;

        private void Start()
        {
            Create();
        }


        [TitleGroup("Test", order: -1)]
        [PropertySpace(SpaceAfter = 20)]
        public void Create()
        {
            var width = m_Matrix.GetLength(0);
            var height = m_Matrix.GetLength(1);
            var gridDimensions = new Vector2Int(width, height);
                      
            var nodePoses = new List<Vector3>(width * height);
            var gapPoses = new List<Vector3>();
        
            var centerOffset = GridExtentions.GetCenterOffsetPosition(width, height, gridCoord: m_GridCoord);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    var exists = m_Matrix[col, row];
                    var pos = GridExtentions.GetNodePositionAt(column: col, row: row, centerOffset: centerOffset, gridCoord: m_GridCoord);                                     

                    if (exists)
                        nodePoses.Add(pos);
                    else
                        gapPoses.Add(pos);
                }
            }


            #region Nodes

            var ruleTileArgs = new TileMapper.RuleTileArgs
            {
                UID = m_UID,
                RuleTile = m_RuleTile,
            };
            var gridArgs = new TileMapper.GridArgs
            {
                GridCoord = m_GridCoord,
                GridDimensions = gridDimensions,
                Offset = m_Offset
            };
            var collectionArgs = new TileMapper.CollectionArgs
            {
                RuleTileArgs = ruleTileArgs,
                GridArgs = gridArgs,
                NodePoses = nodePoses
            };

            TileMapper.AddTileMapperCollection(collectionArgs);

            #endregion
                    

            #region Border

            if (m_RuleTileBorder != null)
            {
                var borderPoses = GetBorderNodes(width: width, height: height, centerOffset: centerOffset);

                borderPoses.AddRange(gapPoses);

                var borderCollectionArgs = new TileMapper.CollectionArgs
                {
                    RuleTileArgs = new TileMapper.RuleTileArgs
                    {
                        UID = m_UID,
                        RuleTile = m_RuleTileBorder,
                    },
                    GridArgs = gridArgs,
                    NodePoses = borderPoses
                };


                TileMapper.AddTileMapperCollection(borderCollectionArgs);
            }          

            #endregion
        }

        public List<Vector3> GetBorderNodes(int width, int height, Vector3 centerOffset,  float nodeSize = 1f)
        {
            List<Vector3> borderPoses = new List<Vector3>();

            var nextRowVector = GridExtentions.GetNextRowVector(gridCoord: m_GridCoord, nodeSize: nodeSize);
            var nextColumnVector = GridExtentions.GetNextColumnVector(gridCoord: m_GridCoord, nodeSize: nodeSize);


            for (int col = -1; col < width + 1; col++)
            {
                var topRowPos = GridExtentions.GetNodePositionAt(column: col, row: 0, centerOffset: centerOffset, gridCoord: m_GridCoord);
                topRowPos -= nextRowVector;

                var bottomRowPos = GridExtentions.GetNodePositionAt(column: col, row: height - 1, centerOffset: centerOffset, gridCoord: m_GridCoord);
                bottomRowPos += nextRowVector;

                if (!borderPoses.Contains(topRowPos))
                    borderPoses.Add(topRowPos);

                if (!borderPoses.Contains(bottomRowPos))
                    borderPoses.Add(bottomRowPos);
            }

            for (int row = -1; row < height + 1; row++)
            {
                var leftColPos = GridExtentions.GetNodePositionAt(column: 0, row: row, centerOffset: centerOffset, gridCoord: m_GridCoord);
                leftColPos -= nextColumnVector;

                var rightColPos = GridExtentions.GetNodePositionAt(column: width - 1, row: row, centerOffset: centerOffset, gridCoord: m_GridCoord);
                rightColPos += nextColumnVector;

                if (!borderPoses.Contains(leftColPos))
                    borderPoses.Add(leftColPos);

                if (!borderPoses.Contains(rightColPos))
                    borderPoses.Add(rightColPos);
            }

            return borderPoses;
        }


    }
}
