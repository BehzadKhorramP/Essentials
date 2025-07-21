using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;

namespace MadApper
{
    public class CameraSeeker : MonoBehaviour
    {
        public CameraSO m_CamSO;

        public Canvas[] m_Canvas;
        public Canvas m_SelfCanvas;

        public Camera FoundCamera;

        public static Action<CameraSeeker> onTryGetSetCamera;

        bool isSet;
        private void OnEnable()
        {
            isSet = false;

            TryGetSetCamera();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (m_SelfCanvas == null)
            {
                if (transform.TryGetComponent(out m_SelfCanvas))
                {
                    this.TrySetDirty();
                }
            }
        }
        void TryGeSetEditor()
        {
            if (m_CamSO == null || m_Canvas == null)
                return;

            var targets = FindObjectsOfType<CameraTarget>();

            foreach (var item in targets)
            {
                if (item.IsTarget(this))
                {
                    SetCamera(item);
                    return;
                }
            }
        }

        [ContextMenu("All Cameras Seeking")]
        public void AllCamerasSeek()
        {
            var seekers = FindObjectsOfType<CameraSeeker>();

            foreach (var item in seekers)
                item.TryGeSetEditor();
        }
#endif



        // Via Events
        public void z_TryGetSetCamera()
        {
            TryGetSetCamera();
        }

        void TryGetSetCamera()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (m_CamSO == null)
                return;

            StartCoroutine(TryRoutine());

            IEnumerator TryRoutine()
            {
                while (isSet == false)
                {
                    onTryGetSetCamera?.Invoke(this);
                    yield return null;
                }
            }
        }

        bool CanvasesHaveCamera()
        {
            if (m_Canvas == null)
                return true;

            foreach (var item in m_Canvas)
            {
                if (item.worldCamera == null)
                    return false;
            }
            return true;
        }

        public async UniTask<Camera> GetCamera()
        {
            if (FoundCamera != null)
                return FoundCamera;

            TryGetSetCamera();

            await UniTask.DelayFrame(1);

            return FoundCamera != null ? FoundCamera : Camera.main;
        }

        internal void SetCamera(CameraTarget cameraTarget)
        {
            FoundCamera = cameraTarget.m_Cam;

            if (m_Canvas != null)
            {
                isSet = true;

                if (transform.TryGetComponent(out Canvas selfCanvas))
                {
                    selfCanvas.worldCamera = cameraTarget.m_Cam;
                }

                foreach (var item in m_Canvas)
                {
                    item.worldCamera = cameraTarget.m_Cam;
                }
            }

        }
    }

}