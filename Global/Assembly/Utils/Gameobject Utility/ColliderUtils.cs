using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Essentials
{
    [ExecuteAlways]
    public class ColliderUtils : MonoBehaviour
    {
        [SerializeField][AutoGetOrAdd] BoxCollider m_Collider;
        [SerializeField] BoxCollider targetCollider;
        [SerializeField] Transform m_Target;
        [SerializeField] AxisConstraints syncAxes = AxisConstraints.XYZ;

#if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying) return;

            if ((m_Target == null && targetCollider == null) || m_Collider == null) return;

            Vector3 currentSize = m_Collider.size;
            Vector3 targetScale = targetCollider != null ? targetCollider.size : m_Target.localScale;
            Vector3 newSize = currentSize;

            if (syncAxes.HasFlag(AxisConstraints.X)) newSize.x = targetScale.x;
            if (syncAxes.HasFlag(AxisConstraints.Y)) newSize.y = targetScale.y;
            if (syncAxes.HasFlag(AxisConstraints.Z)) newSize.z = targetScale.z;

            if (newSize != currentSize)
            {
                m_Collider.size = newSize;
                this.TrySetDirty();
            }
        }

#endif

    }
}
