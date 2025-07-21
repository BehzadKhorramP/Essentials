using System.Collections.Generic;
using UnityEngine;

namespace BEH
{
    public abstract class BoundaryArranger : MonoBehaviour
    {
        public enum Coord { XY, XZ }

        public Transform m_Parent;

        [SerializeField] protected Coord m_Coord;
        [SerializeField] protected Transform m_LeftBound;
        [SerializeField] protected Transform m_RightBound;
        [SerializeField] protected Transform m_TopBound;
        [SerializeField] protected Transform m_BottomBound;
        [SerializeField] protected float m_Spacing = .5f;
        [SerializeField] protected float m_MaxDivider = 1f;

        public abstract void Execute(List<Item> items, out List<Vector3> poses);

        public struct Item
        {
            public float Width;
            public float Height;
        }
    }
}
