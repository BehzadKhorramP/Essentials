#if ANIMATIONSEQ_ENABLED
using BrunoMikoski.AnimationSequencer;
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

[Serializable]
public class CanvasGroupInteractabilityDOTweenAction : DOTweenActionBase
{
    public override Type TargetComponentType => typeof(CanvasGroup);

    public override string DisplayName => "Interactable Canvas Group";


    [SerializeField]
    private bool interactable;
    public bool Interactable
    {
        get => interactable;
        set => interactable = value;
    }

    private CanvasGroup canvasGroup;
    private bool previousInteractable;

    protected override Tweener GenerateTween_Internal(GameObject target, float duration)
    {
        if (canvasGroup == null)
        {
            canvasGroup = target.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                Debug.LogError($"{target} does not have {TargetComponentType} component");
                return null;
            }
        }

        previousInteractable = canvasGroup.interactable;

        float a = 0;

        var canvasTween = DOTween.To(() => a, x => a = x, 0, duration).OnComplete(() =>
        {
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        });

        canvasTween.SetTarget(canvasGroup);

        return canvasTween;
    }

    public override void ResetToInitialState()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.interactable = previousInteractable;
        canvasGroup.blocksRaycasts = previousInteractable;
    }
}

#endif