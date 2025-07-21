using BEH;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Beardy
{
    [ExecuteAlways]
    public class DiamondGrid_Modifier : MonoBehaviour
    {
        [Space(10)] public Beardy.GridLayoutGroup m_DiamondGridLayout;

        [Space(10)] public float m_TopOffset;

        [Tooltip("-1 means fixed cellsize")]
        [Space(10)] public float m_MaxSize;

        [Space(10)] public List<ConstraintBand> m_ConstraintBands;
        [Space(10)] public List<SizeBand> m_SizeBands;

        [Space(10)] public RectTransform m_SyncRect;

        [Space(10)] public Vector2 m_SyncExtra;

        [Space(10)][SerializeField] AxisConstraint m_AxisConstraint;


        [Flags]
        public enum AxisConstraint
        {
            None = 0x0,
            X = 0x2,
            Y = 0x4,           
            W = 0x8
        }

        RectTransform rect;
        RectTransform m_Rect
        {
            get
            {
                if (rect == null && m_DiamondGridLayout != null)
                    rect = m_DiamondGridLayout.GetComponent<RectTransform>();
                return rect;
            }
        }


        int count;


#if UNITY_EDITOR

        private void Update()
        {
            if (Application.isPlaying)
                return;

            if (m_DiamondGridLayout == null)
                return;

            var count = GetCount();

            if (count <= 0)
                return;

            RefreshGrid(count);
        }
#endif



        internal void RefreshGrid(int count)
        {
            if (m_DiamondGridLayout == null)
                return;

            if (this.count == count)
                return;

            if (count <= 0)
                return;

            RefreshConstraintBand(count);

            var bandMaxSize = GetMaxSizeBand(count);

            var colLimit = m_DiamondGridLayout.constraintCount;

            var colCount = Mathf.Min(colLimit, count);

            var rowCount = count / colCount;
            var rowMudolu = count % colCount;

            rowCount = rowMudolu != 0 ? rowCount + 1 : rowCount;

            var newOfsset = ((count - 1) / colLimit * (m_TopOffset / 2f)) + m_TopOffset;

            if (m_Rect != null)
            {
                m_Rect.anchoredPosition = new Vector2(0, -newOfsset);

                if (m_MaxSize != -1)
                {
                    var rectSize = m_Rect.rect.size;

                    var size = 100f;

                    var gap = m_DiamondGridLayout.spacing;

                    var gapX = colCount * gap.x;
                    var gapY = rowCount * gap.y;

                    var sizeX = (rectSize.x - gapX) / colCount;
                    var sizeY = (rectSize.y - gapY) / rowCount;

                    sizeX = Mathf.Min(sizeX, rectSize.x);
                    sizeY = Mathf.Min(sizeY, rectSize.y);

                    size = Mathf.Min(sizeX, sizeY);

                    if (bandMaxSize > 0)
                        size = Mathf.Min(size, bandMaxSize);
                    else if (m_MaxSize > 0)
                        size = Mathf.Min(size, m_MaxSize);

                    m_DiamondGridLayout.cellSize = Vector2.one * size;
                }
            }

            this.count = count;

            RefreshSync(colCount, rowCount);
        }


        int GetCount()
        {
            var count = 0;

            foreach (Transform item in m_DiamondGridLayout.transform)
            {
                if (item.gameObject.activeInHierarchy)
                    count++;
            }

            return count;
        }

        void RefreshConstraintBand(int count)
        {
            if (m_ConstraintBands == null || m_ConstraintBands.Count <= 0)
                return;

            var constraint = m_ConstraintBands[0].Constraint;

            foreach (var item in m_ConstraintBands)
            {
                if (item.Band <= count)
                    constraint = item.Constraint;
                else
                    break;
            }

            m_DiamondGridLayout.constraintCount = constraint;
        }


        float GetMaxSizeBand(int count)
        {
            if (m_SizeBands == null || m_SizeBands.Count <= 0)
                return 0f;

            var res = m_SizeBands[0].MaxSize;

            foreach (var item in m_SizeBands)
            {
                if (item.Band <= count)
                    res = item.MaxSize;
                else
                    break;
            }

            return res;
        }


        void RefreshSync(int col, int row)
        {
            if (m_SyncRect == null)
                return;

            var size = m_DiamondGridLayout.cellSize;
            var space = m_DiamondGridLayout.spacing;

            Vector2 rectSize = m_SyncRect.sizeDelta;

            // Apply calculations based on the constraint
            if (m_AxisConstraint == AxisConstraint.None || m_AxisConstraint.HasFlag(AxisConstraint.X))
            {
                rectSize.x = (size.x + space.x) * col;
            }

            if (m_AxisConstraint == AxisConstraint.None || m_AxisConstraint.HasFlag(AxisConstraint.Y))
            {
                rectSize.y = (size.y + space.y) * row;
            }

            // Add the extra size sync
            rectSize += m_SyncExtra;


        //    var rectSize = new Vector2((size.x + space.x) * col, (size.y + space.y) * row) + m_SyncExtra;

            m_SyncRect.sizeDelta = rectSize;
        }


        public void Refresh()
        {
            this.count = 0;

            var c = GetCount();

            RefreshGrid(c);
        }

        #region Targeted Action

        [Button]
        [ContextMenu("REFRESH")]
        public void z_Refresh()
        {
            Refresh();
        }

        #endregion




        [Serializable]
        public class ConstraintBand
        {
            [Space(10)] public int Band;
            [Space(10)] public int Constraint;
        }


        [Serializable]
        public class SizeBand
        {
            [Space(10)] public int Band;
            [Space(10)] public float MaxSize;
        }

    }
}
