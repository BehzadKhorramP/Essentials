#if ANIMATIONSEQ_ENABLED
using BrunoMikoski.AnimationSequencer;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Essentials
{
    public class VirtualDOTweenAction : DOTweenActionBase
    {
        public override Type TargetComponentType => typeof(MonoBehaviour);

        public override string DisplayName => "DOVirtual";

        [SerializeField]
        private float from, to;

        [SerializeField]
        UnityEventDelayList<float> onUpadate;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            var f = from;
            var t = to;

            var tween = DOTween.To(() => f, x =>
            {
                f = x;
                onUpadate?.Invoke(f);
            }, t, duration);

            tween.SetTarget(this);

            return tween;
        }
        public override void ResetToInitialState()
        {

        }
    }
}

#endif