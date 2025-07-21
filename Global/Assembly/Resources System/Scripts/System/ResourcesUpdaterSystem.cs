using MadApper.Singleton;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MadApper
{
    public class ResourcesUpdaterSystem : PersistentSingleton<ResourcesUpdaterSystem>
    {
        const string k_PrefabName = "ResourcesUpdaterSystem";

        [Space(10)][SerializeField] SingletonScriptableHelper<ResourcesInitialValues> m_InitialValues;

        List<ResourceState> resources = new();

        float elapsed;

        bool shouldInitialEventRaised;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            MADUtility.TryLoadAndInstantiate<ResourcesUpdaterSystem>(k_PrefabName);
        }

        private void OnEnable()
        {
            ResourcesDataManager.onResourceChanged += TryUpdateResources;
        }
        private void OnDisable()
        {
            ResourcesDataManager.onResourceChanged -= TryUpdateResources;
        }

        protected override void Awake()
        {
            base.Awake();

            Initialize();
        }

        private void Start()
        {
            TrySendInitialEvent();
        }

        void Initialize()
        {
            if (m_InitialValues == null || m_InitialValues.RunTimeValue == null)
                return;

            m_InitialValues.RunTimeValue.SetInitialValues(out shouldInitialEventRaised);

            resources = new List<ResourceState>();

            var grantFreePerIntervals = m_InitialValues.RunTimeValue.GetGrantFreePerIntervals();

            if (grantFreePerIntervals != null && grantFreePerIntervals.Any())
            {
                foreach (var item in grantFreePerIntervals)
                {
                    resources.Add(new ResourceState()
                    {
                        ResourceSO = item.Resource,
                        GrantFreeAmount = item.FreeAmount,
                        GrantFreePerInterval = item.FreePerInterval
                    });
                }
            }

            foreach (var item in resources)
            {
                var data = item.ResourceSO.GetResourceSavedData();

                if (data.IsPremium) item.IsPremium = true;
                else
                {
                    var timerTill = data.UnlimitedTill;
                    var remainingSeconds = (int)(timerTill - DateTime.Now).TotalSeconds;

                    // only to save the data
                    item.ResourceSO.Set(remainingSeconds, ResourceType.UnlimitedTimer, raiseEvent: false);
                }

                TryGrantFree(item.ResourceSO, data, GetToBeGrantedCount(data, interval: item.GrantFreePerInterval, multiplier: item.GrantFreeAmount));
            }
        }
        void TrySendInitialEvent()
        {
            if (!shouldInitialEventRaised) return;
            var trackings = GetComponents<IResourceTracking>();
            m_InitialValues.RunTimeValue.TrackInitialValues(trackings);
        }

        void TryGrantFree(ResourceItemSO item, ResourceData data, int amount, bool raiseEvent = true)
        {
            if (amount <= 0) return;
            data.LastTimeFreeGranted = DateTime.Now;
            item.Modify(amount: amount, save: true, raiseEvent: raiseEvent);
        }

        int GetToBeGrantedCount(ResourceData data, int interval, int multiplier)
        {
            if (interval <= 0)
                return 0;

            var amount = data.Amount;
            var cap = data.Cap;

            if (amount >= cap)
                return 0;

            if (multiplier <= 0)
                multiplier = 1;

            var lastTimeFreeGranted = data.LastTimeFreeGranted;
            var offlineDifference = (int)(DateTime.Now - lastTimeFreeGranted).TotalSeconds;
            var toBeGrantendCount = (offlineDifference / interval) * multiplier;
            var diff = cap - amount;

            if (diff < 0)
                diff = 0;

            toBeGrantendCount = Mathf.Min(diff, toBeGrantendCount);

            return toBeGrantendCount;
        }


        private void TryUpdateResources(string id, ResourceData data, ResourceType type)
        {
            if (resources == null) return;

            if (type == ResourceType.Premium)
            {
                var item = resources.Find(x => x.ResourceSO.IsEquals(id));
                if (item == null) return;
                item.IsPremium = data.IsPremium;
            }
        }



        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;

            if (elapsed < 1) return;

            elapsed = 0;

            if (resources == null || !resources.Any()) return;

            foreach (var item in resources)
            {
                if (item.IsPremium) continue;

                var data = item.ResourceSO.GetResourceSavedData();

                TryGrantFree(item.ResourceSO, data, GetToBeGrantedCount(data, interval: item.GrantFreePerInterval, multiplier: item.GrantFreeAmount), raiseEvent: true);

                UpdateUnlimitedTill();

                void UpdateUnlimitedTill()
                {
                    var unlimitedTill = data.UnlimitedTill;
                    var remaining = (unlimitedTill - DateTime.Now).TotalSeconds;

                    if (remaining >= 0)
                    {
                        remaining -= 1;

                        item.ResourceSO.RaiseEvent(type: ResourceType.UnlimitedTimer);

                        if (remaining <= 0)
                        {
                            item.ResourceSO.RaiseForcedEvent(type: ResourceType.Simple);
                        }
                    }
                }
            }
        }



        [Serializable]
        public class ResourceState
        {
            public ResourceItemSO ResourceSO;

            public bool IsPremium;

            public int GrantFreePerInterval = -1;

            public int GrantFreeAmount = 1;
        }


    }

}