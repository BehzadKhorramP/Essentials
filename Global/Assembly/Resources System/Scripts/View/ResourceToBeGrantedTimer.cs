using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace MadApper
{
    public class ResourceToBeGrantedTimer : MonoBehaviour
    {
        public enum TextType { Hour, Minutes, Seconds }


        [Space(10)][SerializeField][AutoGetOrCreateSO] ResourcesInitialValues m_InitialValues;

        [Space(10)] public ResourceItemSO m_Resource;

        [Space(10)] public TextMeshProUGUI m_Text;

        [Space(10)] public TextType m_TextType = TextType.Hour;

        [Space(10)] public bool isAutoActivate;

        [Space(10)] public UnityEventDelay m_OnStartedTimer;
        [Space(10)] public UnityEventDelay m_OnReachedToCap;

        int Inteval = -1;

        float elapsed;

        bool IsActive { get; set; }

        bool IsInitialized { get; set; }
        string text = "";

        private void OnEnable()
        {
            ResourcesDataManager.onResourceChanged += OnResourceChagned;
        }
        private void OnDisable()
        {
            ResourcesDataManager.onResourceChanged -= OnResourceChagned;
        }

        private void Start()
        {
            if (m_Resource == null)
                return;

            if (isAutoActivate)
            {              
                var data = m_Resource.GetResourceSavedData();

                TryActivate(data);
            }
        }
        private void Update()
        {
            if (!IsActive)
                return;

            elapsed += Time.unscaledDeltaTime;

            if (elapsed < 1)
                return;

            elapsed = 0;

            TryUpdate();
        }

        private void OnResourceChagned(string id, ResourceData data, ResourceType typ)
        {
            if (!isAutoActivate)
                return;

            if (m_Resource == null || !m_Resource.IsEquals(id))
                return;

            TryActivate(data);
        }


        void TryActivate(ResourceData data)
        {
            Initialize(); 

            var reachedCap = IsReachedCap(data);

            if (reachedCap)
                return;

            z_Activate();
        }
    
        void Initialize()
        {
            if (IsInitialized || m_InitialValues == null || m_Resource == null)
                return;

            var grantFree = m_InitialValues.GetGrantFreePerIntervals();
                      
            if (grantFree == null || !grantFree.Any())
                return;

            var g = grantFree.Find(x => x.Resource.IsEquals(m_Resource));

            if (g == null)
                return;

            Inteval = g.FreePerInterval;
            IsInitialized = true;
        }

        bool IsReachedCap(ResourceData data)=> (data.Amount >= data.Cap) || data.IsPremium || m_Resource.HasActiveUnlimitedTimer() || Inteval <= 0;
        void TryUpdate()
        {
            var data = m_Resource.GetResourceSavedData();          

            var reachedCap = IsReachedCap(data);

            if (reachedCap)
            {
                m_OnReachedToCap?.Invoke();
                IsActive = false;
                return;
            }

            var lastTimeFreeGranted = data.LastTimeFreeGranted;
            var nexTimeFreeWillBeGranted = lastTimeFreeGranted.AddSeconds(Inteval);

            var seconds = (nexTimeFreeWillBeGranted - DateTime.Now).TotalSeconds;
            var duration = TimeSpan.FromSeconds(seconds);

            switch (m_TextType)
            {
                case TextType.Hour:
                    text = $"{(int)duration.TotalHours}:{duration:mm}:{duration:ss}";
                    break;
                case TextType.Minutes:
                    text = $"{duration:mm}:{duration:ss}";
                    break;
            }

            m_Text?.SetText(text);
        }




        public void z_Activate()
        {
            Initialize();

            elapsed = 1;

            IsActive = true;

            m_OnStartedTimer?.Invoke();
        }

        public void z_Deactivate()
        {
            IsActive = false;

            elapsed = 0;
        }

    }

}