#if ANIMATIONSEQ_ENABLED
using BrunoMikoski.AnimationSequencer;
using DG.Tweening;
using System;
using UnityEngine;

namespace BEH
{
    [Serializable]
    public class CameraDOTweenAction : DOTweenActionBase
    {
        public override Type TargetComponentType => typeof(Camera);

        public override string DisplayName => "Camera";

        [SerializeField]
        private float orthoSize;
        public float OrthoSize
        {
            get => orthoSize;
            set => orthoSize = value;
        }
        [SerializeField]
        private float fov;
        public float FOV
        {
            get => fov;
            set => fov = value;
        }

        private Camera camera;
        private float previousOrthoSize;
        private float previousFOV;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (camera == null)
            {
                camera = target.GetComponent<Camera>();

                if (camera == null)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component");
                    return null;
                }
            }

            var isOrtho = camera.orthographic;

            previousOrthoSize = camera.orthographicSize;
            previousFOV = camera.fieldOfView;

            if (isOrtho)
            {
                var tween = DOTween.To(() => camera.orthographicSize, x => camera.orthographicSize = x, orthoSize, duration);
                tween.SetTarget(camera);
                return tween;
            }
            else
            {
                var tween = DOTween.To(() => camera.fieldOfView, x => camera.fieldOfView = x, fov, duration);
                tween.SetTarget(camera);
                return tween;
            }
          
        }
        public override void ResetToInitialState()
        {
            if (camera == null)
                return;

            camera.orthographicSize = previousOrthoSize;
            camera.fieldOfView = previousFOV;
        }

    }
}

#endif