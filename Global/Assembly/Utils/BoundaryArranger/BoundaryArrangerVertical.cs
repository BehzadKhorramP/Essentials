using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BEH
{
    public class BoundaryArrangerVertical : BoundaryArranger
    {
        public override void Execute(List<Item> items, out List<Vector3> poses)
        {
            var maxWidth = (m_RightBound.transform.position - m_LeftBound.transform.position).magnitude;
            var maxHeight = (m_TopBound.transform.position - m_BottomBound.transform.position).magnitude;

            var count = items.Count;

            var totalSpacingY = (count - 1) * m_Spacing;
            var totalObjectsHeight = items.Sum(x => x.Height);
            var broadestObjectsWidth = items.Max(x => x.Width);

            var divideX = maxWidth / broadestObjectsWidth;
            var divideY = maxHeight / (totalObjectsHeight + totalSpacingY);

            var divide = Mathf.Min(divideY, divideX);

            if (divide > m_MaxDivider)
                divide = m_MaxDivider;

            m_Parent.localScale = Vector3.one * divide;

            poses = new List<Vector3>();

            var refPos = m_Parent.transform.position;

            float currentY = 0f;
         

            switch (m_Coord)
            {
                case Coord.XY:
                    currentY = refPos.y - (totalObjectsHeight + totalSpacingY) * divide / 2f;

                    foreach (var item in items)
                    {
                        var pos = refPos;
                        var scaledWidth = item.Height * divide;
                        var scaledSpacing = m_Spacing * divide;

                        pos.y = currentY + scaledWidth / 2f;
                        poses.Add(pos);
                        currentY += scaledWidth + scaledSpacing;
                    }

                    break;

                case Coord.XZ:
                    currentY = refPos.z - (totalObjectsHeight + totalSpacingY) * divide / 2f;

                    foreach (var item in items)
                    {
                        var pos = refPos;
                        var scaledWidth = item.Height * divide;
                        var scaledSpacing = m_Spacing * divide;

                        pos.z = currentY + scaledWidth / 2f;
                        poses.Add(pos);
                        currentY += scaledWidth + scaledSpacing;
                    }

                    break;
            }


        }
    }
}
