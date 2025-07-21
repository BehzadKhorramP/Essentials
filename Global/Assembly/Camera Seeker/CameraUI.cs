using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using MadApper.Singleton;
using MadApper;
#if RP_ENABLED
using UnityEngine.Rendering.Universal;
#endif

namespace MadApper
{
    [RequireComponent(typeof(Camera))]
    public class CameraUI : PersistentSingleton<CameraUI>, IActiveableSystem
    {
        const string k_PrefabName = "Camera UI";
        public string i_PrefabName => k_PrefabName;


        Camera cam;
        Camera m_Cam
        {
            get
            {
                if (cam == null)
                    cam = GetComponent<Camera>();
                return cam;
            }
        }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            if (!IActiveableSystem.IsActive(k_PrefabName))
                return;


            MADUtility.TryLoadAndInstantiate<CameraUI>(k_PrefabName);
        }

        protected override void Awake()
        {
            base.Awake();

            new SceneChangeSubscriber.Builder()
                 .SetOnSceneChanged(OnSceneChanged)
                 .Build();
        }

        private void OnSceneChanged(string scene)
        {
            StopAllCoroutines();

            StartCoroutine(TryAttachToMainCamera());
        }

        IEnumerator TryAttachToMainCamera()
        {
            yield return null;

            if (!Application.isPlaying) yield break;

#if RP_ENABLED
            while (Camera.main == null)
                yield return null;

            var mainCam = Camera.main;

            var cameraData = mainCam.GetUniversalAdditionalCameraData();

            if (!cameraData.cameraStack.Contains(m_Cam))
                cameraData.cameraStack.Add(m_Cam);
#endif

        }
    }

}