using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_6000_0_OR_NEWER
using Unity.Cinemachine;
#else
using Cinemachine;
#endif

namespace MadApper.Cinemachine
{
    public class CinemachineUtils : MonoBehaviour
    {
#if UNITY_6000_0_OR_NEWER
        [AutoGetOrAdd][SerializeField] CinemachineCamera virtualCamera;
#else
        [AutoGetOrAdd][SerializeField] CinemachineVirtualCamera m_VirtualCamera;
#endif

        public void z_UpdateFOV(float value)
        {
#if UNITY_6000_0_OR_NEWER
            virtualCamera.Lens.FieldOfView = value;
#else
            m_VirtualCamera.m_Lens.FieldOfView = value;
#endif
        }

        public void z_UpdateOrthosize(float value)
        {
#if UNITY_6000_0_OR_NEWER
            virtualCamera.Lens.OrthographicSize = value;
#else
            m_VirtualCamera.m_Lens.OrthographicSize = value;
#endif
        }
    }
}
