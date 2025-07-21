using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper
{
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class CameraTarget : MonoBehaviour
    {
        public CameraSO m_CamSO;

        Camera cam;
        public Camera m_Cam
        {
            get
            {
                if (cam == null)
                    cam = GetComponent<Camera>();
                return cam;
            }
        }

        public static Action<string, Action<CameraTarget>> tryGetCameraTarget;

        private void OnEnable()
        {
            CameraSeeker.onTryGetSetCamera += OnTryGetSetCamera;
            tryGetCameraTarget += TryGetCamera;
        }

      

        private void OnDisable()
        {
            CameraSeeker.onTryGetSetCamera -= OnTryGetSetCamera;
            tryGetCameraTarget -= TryGetCamera;
        }

      
        public bool IsTarget(string id) => id.Equals(m_CamSO.m_ID);
        public bool IsTarget(CameraSeeker obj) => obj.m_CamSO.m_ID.Equals(m_CamSO.m_ID);

        private void OnTryGetSetCamera(CameraSeeker obj)
        {
            if (!IsTarget(obj))
                return;

            obj.SetCamera(this);
        }

        private void TryGetCamera(string id, Action<CameraTarget> arg2)
        {
            if (!IsTarget(id))
                return;

            arg2?.Invoke(this);
        }
    }

}