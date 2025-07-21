using UnityEngine;

namespace MadApper.Grid
{
    public static class GridExtentions
    {
        public static Vector3 GetCenterOffsetPosition(int width, int height, GridCoord gridCoord = GridCoord.XY, float nodeSize = 1f)
        {
            var centerWidth = (width - 1) * nodeSize / 2f;
            var centerHeight = (height - 1) * nodeSize / -2f;

            switch (gridCoord)
            {
                case GridCoord.XZ: return new Vector3(centerWidth, 0, centerHeight);
                case GridCoord.XY: return new Vector3(centerWidth, centerHeight, 0);
            }

            return default;
        }
        public static Vector3 GetNodePositionAt(int column, int row, Vector3 centerOffset, GridCoord gridCoord = GridCoord.XY, float nodeSize = 1f)
        {
            switch (gridCoord)
            {
                case GridCoord.XZ: return (new Vector3(column, 0, -row) * nodeSize) - centerOffset;
                case GridCoord.XY: return (new Vector3(column, -row, 0) * nodeSize) - centerOffset;
            }

            return default;
        }
        public static Vector3 GetNextRowVector(GridCoord gridCoord = GridCoord.XY, float nodeSize = 1f)
        {
            switch (gridCoord)
            {
                case GridCoord.XZ: return (new Vector3(0, 0, -1) * nodeSize);
                case GridCoord.XY: return (new Vector3(0, -1, 0) * nodeSize);
            }

            return default;
        }
        public static Vector3 GetNextColumnVector(GridCoord gridCoord = GridCoord.XY, float nodeSize = 1f)
        {
            switch (gridCoord)
            {
                case GridCoord.XZ: return (new Vector3(1, 0, 0) * nodeSize);
                case GridCoord.XY: return (new Vector3(1, 0, 0) * nodeSize);
            }

            return default;
        }
    }
}
