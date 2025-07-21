using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MadApper.Essentials
{
    using static MadApper.Essentials.UIUnlockablePopup;

    public class UIUnlockableBar : MonoBehaviour
    {
        [SerializeField] List<Image> m_Fills, m_Icons, m_Backgrounds;
        [SerializeField] List<TextMeshProUGUI> m_Texts;
        [PropertySpace(10, 10)][SerializeField] CanvasGroup m_TextCanvasGroup;

        [FoldoutGroup("Events")]
        [SerializeField] UnityEventDelayList m_OnSetup;
        [FoldoutGroup("Events")]
        [SerializeField] UnityEventDelayList m_OnUnlocked;

        UnlockableData currentUnlockable;
        float targetPercentage;
        float currentPercentage;
        bool isValidToAnimate;


        public void Setup(SystemArgs args)
        {
            var level = args.FinishedLevel;
            var nextUnlockable = args.Unlockables.Find(x => x.LevelToUnlock > level);
            var previousUnlockables = args.Unlockables.Where(x => x.LevelToUnlock <= level);
            var previousUnlockableLevel = 1;

            if (previousUnlockables != null && previousUnlockables.Any())
            {
                var previous = previousUnlockables.Last();
                previousUnlockableLevel = previous.LevelToUnlock;
            }

            if (nextUnlockable != null) currentUnlockable = nextUnlockable;
            else currentUnlockable = null;

            if (currentUnlockable == null)
            {
                args.OnNoMoreUnlockables?.Invoke();
                args.OnTargetPercentage?.Invoke(0);
                isValidToAnimate = false;
                return;
            }

            isValidToAnimate = true;

            var backgroundSprite = currentUnlockable.BackgroundSprite;

            m_Fills.ForEach(x => x.sprite = backgroundSprite);

            m_Backgrounds.ForEach(x =>
            {
                x.sprite = backgroundSprite;
                x.gameObject.SetActive(true);
            });

            m_Icons.ForEach(x =>
            {
                x.sprite = currentUnlockable.IconSprite;
                x.gameObject.SetActive(false);
                x.transform.localScale = Vector3.one;
            });

            var levelToUnlock = currentUnlockable.LevelToUnlock;
            var totalDiff = levelToUnlock - previousUnlockableLevel;
            var localProgress = level - previousUnlockableLevel + 1;
            currentPercentage = (localProgress - 1) * 1f / totalDiff;
            targetPercentage = localProgress * 1f / totalDiff;

            m_Fills.ForEach(x => x.fillAmount = currentPercentage);

            m_TextCanvasGroup.alpha = 1;

            var text = $"{(int)(currentPercentage * 100)}%";

            m_Texts.ForEach(x =>
            {
                x.SetText(text);
                x.transform.localScale = Vector3.one;
            });

            args.OnTargetPercentage?.Invoke(targetPercentage);

            m_OnSetup?.Invoke();
        }

        public void AnimateBar(AnimatorArgs args)
        {
            if (!isValidToAnimate) return;

            var duration = args.Duration;

            m_Fills.ForEach(x =>
            {
                x.DOKill();
                x.DOFillAmount(targetPercentage, duration);
            });

            var current = (int)(currentPercentage * 100);
            var target = (int)(targetPercentage * 100);

            DOVirtual.Int(current, target, duration, (x) =>
            {
                var text = $"{(int)(x)}%";
                m_Texts.ForEach(x => x.SetText(text));
            })
            .OnComplete(() =>
            {
                OnCompleted();
            });

            async void OnCompleted()
            {
                var extraDuration = .3f;

                m_Texts.ForEach(x => x.transform.DOPunchScale(Vector3.one / 3f, extraDuration));

                if (targetPercentage >= 1)
                {
                    m_Backgrounds.ForEach(x => x.gameObject.SetActive(false));
                    m_Icons.ForEach(x =>
                    {
                        x.gameObject.SetActive(true);
                        x.transform.DOPunchScale(Vector3.one / 3f, extraDuration);
                    });

                    m_TextCanvasGroup.DOKill();
                    m_TextCanvasGroup.DOFade(0, extraDuration);

                    m_OnUnlocked?.Invoke();

                    await UniTask.WaitForSeconds(extraDuration);

                    if (currentUnlockable != null 
                        && currentUnlockable.PopupData != null
                        && currentUnlockable.PopupData.Prefab != null)
                    {
                        await UniTask.WaitForSeconds(extraDuration);

                        var popup = Instantiate(currentUnlockable.PopupData.Prefab);
                        popup.Initialize(currentUnlockable, args.OnContinuePressedFromPopup);
                    }

                    args.OnCompletedUnlocked?.Invoke();
                }
                else
                {
                    await UniTask.WaitForSeconds(extraDuration);

                    args.OnCompletedNotUnlocked?.Invoke();
                }
            }

        }






        [Serializable]
        public class UnlockableData
        {
            [PropertySpace(0, 10)] public int LevelToUnlock;

            [PreviewField] public Sprite IconSprite;
            [PreviewField] public Sprite BackgroundSprite;

            public PopupData PopupData;

        }

        public struct AnimatorArgs
        {
            public float Duration;
            public Action OnCompletedNotUnlocked;
            public Action OnCompletedUnlocked;
            public Action OnContinuePressedFromPopup;
        }

        public struct SystemArgs
        {
            public int FinishedLevel;
            public List<UnlockableData> Unlockables;
            public Action OnNoMoreUnlockables;
            public Action<float> OnTargetPercentage;
        }



    }
}
