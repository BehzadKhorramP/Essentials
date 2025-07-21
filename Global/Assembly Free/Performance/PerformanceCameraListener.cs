using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MadApper
{
    public class PerformanceCameraListener : MonoBehaviour
    {
        [SerializeField][AutoGetOrAdd] Camera m_Camera;


        public void z_OnDeactiveShadows()
        {
            if (m_Camera != null && m_Camera.TryGetComponent(out UniversalAdditionalCameraData cameraData))
            {
                cameraData.renderShadows = false;
            }
        }
    }
}
