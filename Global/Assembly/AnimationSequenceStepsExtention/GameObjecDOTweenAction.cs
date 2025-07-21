using BrunoMikoski.AnimationSequencer;
using DG.Tweening;
using System;
using UnityEngine;

namespace MadApper.Essentials
{
    public class GameObjecDOTweenAction : DOTweenActionBase
    {
        public override Type TargetComponentType => typeof(Transform);

        public override string DisplayName => "GameObject";

        [SerializeField]
        private bool active;
        public bool Active
        {
            get => active;
            set => active = value;
        }
        private GameObject gameObject;

        private bool previousActive;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (target == null) return null;

            gameObject = target;

            previousActive = target.gameObject.activeInHierarchy;

            float a = 0;

            var canvasTween = DOTween.To(() => a, x => a = x, 0, duration).OnComplete(() =>
            {
                target.gameObject.SetActive(active);                
            });

            canvasTween.SetTarget(target);

            return canvasTween;
        }

        public override void ResetToInitialState()
        {
            if (gameObject == null)
                return;

            gameObject.SetActive(previousActive);
        }
    }
}
