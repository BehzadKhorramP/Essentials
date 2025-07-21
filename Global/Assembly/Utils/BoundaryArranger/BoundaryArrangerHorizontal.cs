using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BEH
{
    public class BoundaryArrangerHorizontal : BoundaryArranger
    {
        public override void Execute(List<Item> items, out List<Vector3> poses)
        {
            var maxWidth = (m_RightBound.transform.position - m_LeftBound.transform.position).magnitude;
            var maxHeight = (m_TopBound.transform.position - m_BottomBound.transform.position).magnitude;

            var count = items.Count;

            var totalSpacingX = (count - 1) * m_Spacing;           
            var totalObjectsWidth = items.Sum(x => x.Width);
            var tallestObjectsHeight = items.Max(x => x.Height);     

            var divideX = maxWidth / (totalObjectsWidth + totalSpacingX);
            var divideY = maxHeight / tallestObjectsHeight;

            var divide = Mathf.Min(divideY, divideX);

            if (divide > m_MaxDivider)
                divide = m_MaxDivider;

            m_Parent.localScale = Vector3.one * divide;

            poses = new List<Vector3>();

            var refPos = m_Parent.transform.position;

            var currentX = refPos.x - (totalObjectsWidth  + totalSpacingX) * divide / 2f;

            foreach (var item in items)
            {
                var pos = refPos;
                var scaledWidth = item.Width * divide;
                var scaledSpacing = m_Spacing * divide;

                pos.x = currentX + scaledWidth / 2f;    
                poses.Add(pos);
                currentX += scaledWidth + scaledSpacing;
            }
        }
    }
}
