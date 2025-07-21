using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MadApper
{
    public class ConsumeResource : MonoBehaviour
    {
        [Space(10)] public ResourceItemSO m_ResourceToConsume;

        [Space(10)] public int m_Count = 1;

        [Space(10)] public TextMeshProUGUI m_Text;
        [Space(20)] public Sprite m_ResourceSprite;
        [Space(10)] public Image m_Icon;
        [Space(10)] public TargetedEventSO_String m_OpenShopSectionEventSO;

        [Space(20)] public UnityEventDelay onSucceeded;
        [Space(10)] public UnityEventDelay onFailed;


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (m_Icon != null && m_ResourceSprite != null)
            {
                if (m_Icon.sprite != m_ResourceSprite)
                {
                    m_Icon.sprite = m_ResourceSprite;
                    EditorUtility.SetDirty(m_Icon);
                }
            }

            if (m_Text == null)
                return;

            var currText = m_Text.text;
            var t = $"{m_Count}";

            if (t == currText)
                return;

            m_Text.text = t;

            EditorUtility.SetDirty(m_Text);
        }
#endif


        public void UpdateSetup(int count)
        {
            if (m_Icon != null)
            {
                m_Icon.gameObject.SetActive(true);

                if (count == 0)
                    m_Icon.gameObject.SetActive(false);
            }

            if (m_Text != null)
            {
                m_Text.text = $"{count}";

                if (count == 0)
                {
                    m_Text.text = "";
                }
            }
        }

        public bool IsPossibleToConsume(int count)
        {
            if (m_ResourceToConsume == null)
            {
                this.LogWarning($"ResourceToConsume is null!");
                return false;
            }

            var isPremium = m_ResourceToConsume.IsPremium();

            if (isPremium)
            {
                this.Log($"Succeeded | IsPremium");
                return true;
            }

            var isUnlimited = m_ResourceToConsume.HasActiveUnlimitedTimer();

            if (isUnlimited)
            {
                this.Log($"Succeeded | IsUnlimited");
                return true;
            }

            var amount = m_ResourceToConsume.GetAmount();

            return amount >= count;
        }


        public void TryConsume(int count, out bool resourceSubtracted, Action onsucceeded = null, Action onfailed = null, bool raiseResourceEvent = true)
        {
            resourceSubtracted = false;

            if (m_ResourceToConsume == null)
            {
                this.LogWarning($"ResourceToConsume is null!");
                return;
            }

            var isPremium = m_ResourceToConsume.IsPremium();

            if (isPremium)
            {
                this.Log($"Succeeded | IsPremium");
                onSucceeded?.Invoke();
                onsucceeded?.Invoke();
                return;
            }

            var isUnlimited = m_ResourceToConsume.HasActiveUnlimitedTimer();

            if (isUnlimited)
            {
                this.Log($"Succeeded | IsUnlimited");
                onSucceeded?.Invoke();
                onsucceeded?.Invoke();
                return;
            }

            var amount = m_ResourceToConsume.GetAmount();

            if (amount >= count)
            {
                resourceSubtracted = true;
                m_ResourceToConsume.Modify(-count, raiseEvent: raiseResourceEvent);
                onSucceeded?.Invoke();
                onsucceeded?.Invoke();

                TrackDataSink(count);
            }
            else
            {
                OnFailedToConsume();
                onfailed?.Invoke();
            }
        }

        public void OnFailedToConsume()
        {
            onFailed?.Invoke();
        }



        public void z_TryConsume()
        {
            TryConsume(m_Count, out bool resourceSubtracted);
        }

        public void z_OpenShopSection()
        {
            if (m_ResourceToConsume == null)
                return;

            m_OpenShopSectionEventSO?.RaiseEvent(m_ResourceToConsume.m_ID);
        }





        void TrackDataSink(int count)
        {
            var trackings = GetComponents<IResourceTracking>();

            if (trackings == null)
                return;

            foreach (var track in trackings)
                track?.Sink(resource: m_ResourceToConsume, amount: count);

        }
    }

}