using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MadApper.Essentials
{
    public class UIIntView : MonoBehaviour
    {
        [SerializeField, PropertySpace(0, 10)] protected int m_InitDefualtValue = 0;
        [SerializeField] protected TextMeshProUGUI[] m_Texts;
        [SerializeField] protected UnityEventDelayList<int> m_OnFinishedUpdating;

        protected Tweener tweener;
        protected int currentValue;

        void Start()
        {
            new SceneChangeSubscriber.Builder()
                 .SetOnSceneActivatedInitialize(InitializeScene)
                 .SetOnSceneToBeChangedReset(ResetScene)
                 .AddGameObjectScene(gameObject)
                 .Build();
        }
        void InitializeScene(string obj)
        {
            SetInitCurrentValue();
            RefreshText();
        }
        private void ResetScene(string obj)
        {
            KillTweener();
        }

        public virtual void SetInitCurrentValue()
        {
            SetCurrentValue(m_InitDefualtValue);
        }

        public void SetCurrentValue(int value)
        {
            currentValue = value;
        }
        public void RefreshText()
        {
            var text = currentValue.ToString("N0");
            m_Texts.ForEach(x => x.SetText(text));
        }

        public void KillTweener()
        {
            if (tweener != null)
                tweener.Kill();
        }

        public int GetCurrentValue() => currentValue;


        public void UpdateText(int value, float duration = 1)
        {
            KillTweener();

            m_Texts.ForEach(x => x.transform.localScale = Vector3.one);

            var @in = currentValue;

            tweener = DOVirtual.Int(@in, value, duration, (x) =>
            {
                SetCurrentValue(x);
                RefreshText();
            })
            .OnComplete(() => m_OnFinishedUpdating?.Invoke(value));
        }
    }
}
