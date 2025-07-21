using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MadApper.Grid;

namespace MadApper.TileMap
{
    public class TileMapper : MonoBehaviour
    {
        [SerializeField] TileMapTemplate m_Template;

        List<GeneratedMap> generatedMaps = new();

        static Action<SingleArgs> addTileMapperSingle;
        static Action<CollectionArgs> addTileMapperCollection;

        private void Awake()
        {
            addTileMapperSingle += TryAddTileMapper;
            addTileMapperCollection += TryAddTileMapperCollection;
        }
        private void OnDestroy()
        {
            addTileMapperSingle -= TryAddTileMapper;
            addTileMapperCollection -= TryAddTileMapperCollection;
        }

        // may be hooked to a bridge class to SceneChangeSubscriberHelper
        public void z_OnReset()
        {
            for (int i = generatedMaps.Count - 1; i >= 0; i--)
            {
                var gMap = generatedMaps[i];
                Destroy(gMap.Parent.gameObject, .01f);
            }

            generatedMaps = new List<GeneratedMap>();
        }

        #region Public
        public static void AddTileMapper(SingleArgs args) => addTileMapperSingle.Invoke(args);
        public static void AddTileMapperCollection(CollectionArgs args) => addTileMapperCollection.Invoke(args);

        #endregion

        private async void TryAddTileMapper(SingleArgs args)
        {
            var ruleTile = args.RuleTileArgs.RuleTile;
            var nodePos = args.NodePos;
            var map = GetMap(ruleTileArgs: args.RuleTileArgs, gridArgs: args.GridArgs);

            await UniTask.DelayFrame(2);

            var pos = map.Tilemap.layoutGrid.WorldToCell(nodePos);
            map.Tilemap.SetTile(pos, ruleTile);
        }

        private async void TryAddTileMapperCollection(CollectionArgs args)
        {
            var ruleTile = args.RuleTileArgs.RuleTile;
            var map = GetMap(ruleTileArgs: args.RuleTileArgs, gridArgs: args.GridArgs);

            await UniTask.DelayFrame(2);

            foreach (var nodePos in args.NodePoses)
            {              
                var pos = map.Tilemap.layoutGrid.WorldToCell(nodePos);
                map.Tilemap.SetTile(pos, ruleTile);
            }

        }


        public void Scale(GridArgs args, GeneratedMap map)
        {
            var nodeSize = args.NodeSize;

            if (nodeSize == 0)
                nodeSize = 1;

            map.Tilemap.transform.localScale = Vector3.one * nodeSize;
        }


        GeneratedMap GetMap(RuleTileArgs ruleTileArgs, GridArgs gridArgs)
        {
            var uid = ruleTileArgs.UID;

            // in case it's not important to seperate different maps
            if (string.IsNullOrEmpty(uid))
                uid = ruleTileArgs.RuleTile.name;

            return GetMap(uid: uid, ruleTileArgs: ruleTileArgs, gridArgs: gridArgs);
        }

        GeneratedMap GetMap(string uid, RuleTileArgs ruleTileArgs, GridArgs gridArgs)
        {
            var map = generatedMaps.Find(x => x.UID == uid);

            if (map == null)
            {
                var gMap = CreateTileMap(ruleTileArgs, gridArgs, out Transform parent);

                map = new GeneratedMap()
                {
                    UID = uid,
                    RuleTile = ruleTileArgs.RuleTile,
                    Tilemap = gMap,
                    Parent = parent
                };

                generatedMaps.Add(map);
            }

            return map;
        }

        Tilemap CreateTileMap(RuleTileArgs ruleTileArgs, GridArgs gridArgs, out Transform parent)
        {
            var ruleTile = ruleTileArgs.RuleTile;
            var gridDimensions = gridArgs.GridDimensions;
            var nodeSize = gridArgs.NodeSize;

            if (nodeSize == 0)
                nodeSize = 1f;

            var template = Instantiate(m_Template, transform);
            parent = template.transform;

            template.Grid.name = $"{ruleTile.name}-Grid";
            template.TileMap.name = $"{ruleTile.name}-Tilemap";

            template.Grid.cellSize = Vector3.one * nodeSize;

            switch (gridArgs.GridCoord)
            {
                case GridCoord.XZ:
                    template.Grid.cellSwizzle = GridLayout.CellSwizzle.XZY;
                    template.TileMap.orientation = Tilemap.Orientation.XZ;     
                    template.Grid.transform.position += gridArgs.Offset; 
                    break;
                case GridCoord.XY:
                    template.Grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;
                    template.TileMap.orientation = Tilemap.Orientation.XY;
                    template.Grid.transform.position += gridArgs.Offset;
                    break;
            }
            return template.TileMap;
        }


        async void SetMaterial(TilemapRenderer renderer, Material mat)
        {
            await UniTask.DelayFrame(1);
            renderer.sharedMaterial = mat;
        }

        Vector3Int GetCellPos(Vector3 nPos, GridCoord gridCoord)
        {
            switch (gridCoord)
            {
                case GridCoord.XZ: return new Vector3Int(Mathf.FloorToInt(nPos.x + .05f), Mathf.FloorToInt(nPos.z + .05f), 0);
                case GridCoord.XY: return new Vector3Int(Mathf.FloorToInt(nPos.x + .05f), Mathf.FloorToInt(nPos.y + .05f), 0);
                default: return new Vector3Int(Mathf.FloorToInt(nPos.x + .05f), Mathf.FloorToInt(nPos.z + .05f), 0);
            }
        }



        public class GeneratedMap
        {
            public string UID;
            public RuleTile RuleTile;
            public Tilemap Tilemap;
            public Transform Parent;
        }
        public struct RuleTileArgs
        {
            public string UID;
            public RuleTile RuleTile;
        }
        public struct GridArgs
        {
            public GridCoord GridCoord;
            public Vector2Int GridDimensions;
            public Vector3 Offset;
            public float NodeSize;
        }
        public struct SingleArgs
        {
            public RuleTileArgs RuleTileArgs;
            public GridArgs GridArgs;

            public Vector3 NodePos;
        }
        public struct CollectionArgs
        {
            public RuleTileArgs RuleTileArgs;
            public GridArgs GridArgs;

            public IEnumerable<Vector3> NodePoses;
        }
    }
}
