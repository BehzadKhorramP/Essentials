using DG.Tweening;
using MadApper;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BEH
{
    public class HintIndicator : MonoBehaviour
    {
        public Transform Obj;

        [SerializeField][ReadOnly][AutoGetInChildren] CanvasGroup m_CG;

        Sequence sq;

        private void Awake()
        {
            FadeOut(0);
        }
        public void Move(MoveArgs args)
        {
            if (args.Poses == null || args.Poses.Count == 0)
                return;

            if (sq != null)
                sq.Kill();

            sq = DOTween.Sequence();

            var startPos = args.Poses[0];
            var duration = args.Duration;

            sq.Append(Obj
                .DOMove(startPos, 0)
                .SetEase(Ease.Linear)
                .OnUpdate(() => args.OnUpdated?.Invoke(this, Obj.position)));

            sq.AppendCallback(() => { FadeIn(.1f); });

            if (args.WaitAtStart > 0)
                sq.Append(DOVirtual.DelayedCall(args.WaitAtStart, null));

            sq.AppendCallback(() => args.OnStarted?.Invoke(this));

            if (args.Poses.Count > 1)
            {
                for (int i = 1; i < args.Poses.Count; i++)
                {
                    var pos = args.Poses[i];

                    sq.Append(Obj
                        .DOMove(pos, duration)
                        .SetEase(Ease.Linear)
                        .OnUpdate(() => args.OnUpdated?.Invoke(this, Obj.position)));
                }
            }

            sq.AppendCallback(() => args.OnReached?.Invoke(this));

            if (args.WaitAtDestination > 0)
                sq.Append(DOVirtual.DelayedCall(args.WaitAtDestination, null));

            sq.OnComplete(() => args.OnCompleted?.Invoke(this));

            sq.SetLoops(-1, LoopType.Restart);
        }



        public void Tap(TapArgs args)
        {
            if (sq != null)
                sq.Kill();

            var startPos = args.Pos;

            sq = DOTween.Sequence();

            Obj.transform.position = startPos;
            Obj.transform.localScale = Vector3.one;

            FadeIn(.2f);

            var scaleDownDur = args.ScaleDownDuration.HasValue ? args.ScaleDownDuration.Value : .3f;
            var scaleUpDur = args.ScaleUpDuration.HasValue ? args.ScaleUpDuration.Value : .55f;

            sq.Append(Obj
                .DOScale(Vector3.one / 2f, scaleDownDur)
                .SetEase(Ease.Linear));

            sq.Append(Obj
              .DOScale(Vector3.one, scaleUpDur)
              .SetEase(Ease.Linear));

            sq.SetLoops(-1, LoopType.Restart);
        }



        void FadeIn(float duration)
        {
            m_CG.DOKill();
#if DOTWEEN_ENABLED
            m_CG.DOFade(1, duration); 
#endif
        }
        public void FadeOut(float duration)
        {
            m_CG.DOKill();
#if DOTWEEN_ENABLED
            m_CG.DOFade(0, duration); 
#endif
        }


        public void FadeOutInstant()
        {
            m_CG.DOKill();
            m_CG.alpha = 0;
        }



        public void Terminate()
        {
            m_CG?.DOKill();

            if (sq != null)
                sq.Kill();

            DOTween.Kill(Obj);
        }


        #region Inspector

        public void z_Tap()
        {
            var tapArgs = new TapArgs()
            {
                Pos = Obj.transform.position,
                ScaleDownDuration = .4f,
                ScaleUpDuration = .6f
            };

            Tap(tapArgs);
        }

        #endregion


        public struct MoveArgs
        {
            public List<Vector3> Poses;
            public float Duration;
            public float WaitAtStart;
            public float WaitAtDestination;
            public Action<HintIndicator> OnStarted;
            public Action<HintIndicator, Vector3> OnUpdated;
            public Action<HintIndicator> OnReached;
            public Action<HintIndicator> OnCompleted;
        }

        public struct TapArgs
        {
            public Vector3 Pos;
            public float? ScaleDownDuration;
            public float? ScaleUpDuration;
        }
    }


    public static class HandIndicatorExtentions
    {

    }
}
