using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Essentials
{

    public class GOUtils : MonoBehaviour
    {
        #region Destroyer

        public void z_Destroy(float delay)
        {
            Destroy(gameObject, delay);
        }

        #endregion


        #region Transform

        [FoldoutGroup("Rotations")][SerializeField] Vector3[] m_RandomRotations;

        public void z_RandomRotation()
        {
            transform.localRotation = Quaternion.Euler(m_RandomRotations.GetRandom());
        }
        public void z_MoveToPosition(Transform target)
        {
            transform.position = target.position;
        }

        [FoldoutGroup("Scale")][SerializeField] DotweenScale m_DotweenScale;
        public void z_ScaleToSize(Transform target)
        {
            m_DotweenScale.Execute(target);
        }

        #endregion
    }
}
