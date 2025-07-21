#if ANIMATIONSEQ_ENABLED
using DG.Tweening;
using System;
using UnityEngine;


namespace BrunoMikoski.AnimationSequencer
{
    [Serializable]
    public class SetTargetCanvasGroupPropertiesStep : GameObjectAnimationStep
    {
        [SerializeField]
        private float targetAlpha = 1f;
        [SerializeField]
        private bool targetInteractable = true;
        [SerializeField]
        private bool targetBlockraycast = true;


        private CanvasGroup canvasGroup;

        private float originalAlpha;
        private bool originalInteractable;
        private bool originalBlockraycast;

        public override string DisplayName => "Set Target Canvas Group Properties";
        public override void AddTweenToSequence(Sequence animationSequence)
        {
            Sequence behaviourSequence = DOTween.Sequence();

            behaviourSequence.SetDelay(Delay);

            behaviourSequence.AppendCallback(() =>
            {
                if (canvasGroup == null)
                {
                    canvasGroup = target.GetComponent<CanvasGroup>();

                    if (canvasGroup == null)
                    {
                        Debug.LogError($"{target} does not have CanvasGroup component");
                        return;
                    }
                }

                if (targetAlpha >= 0 && targetAlpha != canvasGroup.alpha)
                {
                    originalAlpha = canvasGroup.alpha;
                    canvasGroup.alpha = targetAlpha;
                }

                if (targetInteractable != canvasGroup.interactable)
                {
                    originalInteractable = canvasGroup.interactable;
                    canvasGroup.interactable = targetInteractable;
                }

                if (targetBlockraycast != canvasGroup.blocksRaycasts)
                {
                    originalBlockraycast = canvasGroup.blocksRaycasts;
                    canvasGroup.blocksRaycasts = targetBlockraycast;
                }

            });

            if (FlowType == FlowType.Join)
                animationSequence.Join(behaviourSequence);
            else
                animationSequence.Append(behaviourSequence);
        }

        public override void ResetToInitialState()
        {
            if (canvasGroup == null)
                return;

            //return;

            //canvasGroup.alpha = originalAlpha;
            //canvasGroup.interactable = originalInteractable;
            //canvasGroup.blocksRaycasts = originalBlockraycast;
        }

        public override string GetDisplayNameForEditor(int index)
        {
            string display = "NULL";

            if (canvasGroup != null)
                display = canvasGroup.name;

            return $"{index}. Set {display} (CanvasGroup) Properties";
        }
    }
}

#endif