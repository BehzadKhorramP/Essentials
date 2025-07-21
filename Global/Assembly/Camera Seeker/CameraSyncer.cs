
using UnityEditor;
using UnityEngine;

namespace MadApper
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraSyncer : MonoBehaviour
    {
        public Camera m_SelfCam;

        public Camera m_TargetCam;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (m_SelfCam != null)
                return;

            m_SelfCam = GetComponent<Camera>();

            EditorUtility.SetDirty(this);
        }
#endif

        private void Start()
        {
            if (!Application.isPlaying)
                return;

            if (m_TargetCam == null)
                enabled = false;
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (m_SelfCam == null || m_TargetCam == null)
                return;
#endif
            m_SelfCam.fieldOfView = m_TargetCam.fieldOfView;
            m_SelfCam.orthographicSize = m_TargetCam.orthographicSize;
        }
    }
}
