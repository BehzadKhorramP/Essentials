using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MadApper.Essentials
{
    [Serializable]
    public class DotweenPunchPosition
    {
        public Vector3 Punch;
        public float Duration = .5f;
        public int Vibrato = 2;
        public float Elasticity = 1;
        public bool Snapping;

        public Tweener Execute(Transform transform)
        {
            return transform.DOPunchPosition(Punch, duration: Duration, vibrato: Vibrato, elasticity: Elasticity, snapping: Snapping);
        }
    }
    [Serializable]
    public class DotweenPunchScale
    {
        public Vector3 Punch;
        public float Duration = .5f;
        public int Vibrato = 2;
        public float Elasticity = 1;

        public Tweener Execute(Transform transform)
        {
            return transform.DOPunchScale(Punch, duration: Duration, vibrato: Vibrato, elasticity: Elasticity);
        }
    }

    [Serializable]
    public class DotweenShakeRotation
    {
        public Vector3 Strength;
        public float Duration = .5f;
        public int Vibrato = 10;
        public float Randomness = 90;
        public bool Fadeout = true;
        public ShakeRandomnessMode RandomnessMode = ShakeRandomnessMode.Full;

        public Tweener Execute(Transform transform)
        {
            return transform.DOShakeRotation(Duration, strength: Strength, vibrato: Vibrato, randomness: Randomness, fadeOut: Fadeout, randomnessMode: RandomnessMode);
        }
    }

    [Serializable]
    public class DotweenScale
    {
        public Vector3 EndValue;
        public float Duration = .5f;
        public Ease Ease;
        public Tweener Execute(Transform transform)
        {
            return transform.DOScale(endValue: EndValue, duration: Duration).SetEase(Ease);
        }
    }

    public static class DOTweenExtentions
    {
        public static async UniTask WaitForTweensToFinish(this object obj, CancellationToken cancellationToken)
        {
            List<Tween> tweens = DOTween.TweensByTarget(obj);

            if (tweens != null && tweens.Count > 0)
                await UniTask.WaitUntil(() => !tweens.Exists(t => t.IsActive() && !t.IsComplete()), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Makes the transform jump to a target position relative to a specified parent's local space.
        /// If the referenceParent is null, it behaves like a world space jump.
        /// </summary>
        /// <param name="transform">The transform to animate.</param>
        /// <param name="targetPos">The target world position to jump to.</param>
        /// <param name="height">The maximum height of the jump arc.</param>
        /// <param name="duration">The duration of the jump.</param>
        /// <param name="arcDirection">The normalized direction of the arc relative to world space (e.g., Vector3.up).</param>
        /// <param name="referenceParent">The parent Transform whose local space the jump should operate in. If null, uses world space.</param>
        /// <returns>A DOTween Tween object.</returns>
        public static Tween JumpTo(this Transform transform, Vector3 targetPos, float height, float duration, Vector3 arcDirection, Transform referenceParent = null, Ease ease = Ease.Linear)
        {
            arcDirection = arcDirection.normalized; // Ensure arc direction is a unit vector

            // Determine initial and target positions in the chosen reference space
            Vector3 startPosInRefSpace;
            Vector3 targetPosInRefSpace;
            Vector3 arcDirectionInRefSpace;

            if (referenceParent != null)
            {             
                startPosInRefSpace = referenceParent.InverseTransformPoint(transform.position);
                targetPosInRefSpace = referenceParent.InverseTransformPoint(targetPos);             
                arcDirectionInRefSpace = referenceParent.InverseTransformDirection(arcDirection);
            }
            else
            {
                // If no referenceParent, use world space directly
                startPosInRefSpace = transform.position;
                targetPosInRefSpace = targetPos;
                arcDirectionInRefSpace = arcDirection;
            }

            var tween = DOVirtual.Float(0, 1, duration, t =>
            {               
                Vector3 basePosInRefSpace = Vector3.Lerp(startPosInRefSpace, targetPosInRefSpace, t);              
                float arc = height * 4f * t * (1 - t);             
                Vector3 currentPosInRefSpace = basePosInRefSpace + arcDirectionInRefSpace * arc;
             
                if (referenceParent != null)
                {
                    transform.position = referenceParent.TransformPoint(currentPosInRefSpace);
                }
                else
                {
                    transform.position = currentPosInRefSpace;
                }

            }).SetEase(ease);

            tween.SetTarget(transform); 

            return tween;
        }
        public static Tween JumpToWithSpeed(this Transform transform, Vector3 targetPos, float height, float speed, Vector3 arcDirection)
        {
            arcDirection = arcDirection.normalized;
            Vector3 startPos = transform.position;

            float distance = Vector3.Distance(startPos, targetPos);
            float duration = distance / speed;

            var tween = DOVirtual.Float(0, 1, duration, t =>
            {
                Vector3 basePos = Vector3.Lerp(startPos, targetPos, t);
                float arc = height * 4f * t * (1 - t);
                transform.position = basePos + arcDirection * arc;
            }).SetEase(Ease.Linear);

            tween.SetTarget(transform);

            return tween;
        }
    }

}
