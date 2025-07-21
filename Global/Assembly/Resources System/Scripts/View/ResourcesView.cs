using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MadApper
{
    public class ResourcesView : MonoBehaviour
    {
        [Space(10)] public ResourceItemSO m_ItemSO;
#if ANIMATIONSEQ_ENABLED
        [Space(10)] public Appearer m_MainAppearer;
#endif
        [Space(10)] public bool isTrivial;


        [SerializeField][AutoGetInChildren] public List<ResourcesViewTypeBase> resourceViews;

        // bridge pattern
        [ShowInInspector][ReadOnly] ResourcesViewTypeBase currentResourceView;

        private void OnEnable()
        {
            ResourcesDataManager.onResourceChanged += OnResourcesChangedView;
            ResourcesDataManager.onResourceChangedForced += OnResourceChangedViewForced;
        }
        private void OnDisable()
        {
            ResourcesDataManager.onResourceChanged -= OnResourcesChangedView;
            ResourcesDataManager.onResourceChangedForced -= OnResourceChangedViewForced;
        }



        public void Start()
        {
            Init();
        }

        void Init()
        {
            var data = m_ItemSO.GetResourceSavedData();

            InitializeResourceView(data);
            UpdateResourceView(data);
        }

        private void OnResourcesChangedView(string id, ResourceData data, ResourceType type)
        {
            if (!IsSame(id))
                return;

            RefreshResourceView(type);
            UpdateResourceView(data);
        }
        private void OnResourceChangedViewForced(string id, ResourceData data, ResourceType forceType)
        {
            if (!IsSame(id))
                return;

            RefreshResourceView(forceType, refreshForce: true);
            UpdateResourceView(data);
        }



        void UpdateResourceView(ResourceData data)
        {
            if (currentResourceView == null)
                return;

            currentResourceView.OnResourceChagend(data);
        }
        bool IsSame(string id) => m_ItemSO.IsEquals(id);
        bool IsLessPriorityType(ResourceType currType, ResourceType compareType) => (int)currType >= (int)compareType;


        void InitializeResourceView(ResourceData data)
        {
            ResourceType currentType = ResourceType.Simple;

            var isPremium = data.IsPremium;

            if (isPremium)
            {
                currentType = ResourceType.Premium;
            }
            else
            {
                var timerTill = data.UnlimitedTill;
                var remainingSeconds = (int)(timerTill - DateTime.Now).TotalSeconds;

                if (remainingSeconds > 0)
                {
                    currentType = ResourceType.UnlimitedTimer;
                }
            }

            currentResourceView = GetResourceView(currentType);

            if (currentResourceView == null)
            {
                this.LogWarning($"ResourceView for | {m_ItemSO.m_ID} | is null");
                return;
            }

            SwitchResourceViews();
        }

        void RefreshResourceView(ResourceType type, bool refreshForce = false)
        {
            if (refreshForce == false)
            {
                var currType = GetCurrentType();

                if (IsLessPriorityType(currType: currType, compareType: type))
                    return;
            }


            var newResourceView = GetResourceView(type);

            if (newResourceView == null)
            {
                this.LogWarning($"ResourceView for | {m_ItemSO.m_ID} | is null");
                return;
            }

            if (newResourceView == currentResourceView)
                return;

            currentResourceView = newResourceView;

            SwitchResourceViews();
        }

        ResourceType GetCurrentType() => currentResourceView != null ? currentResourceView.m_Type : ResourceType.Simple;



        ResourcesViewTypeBase GetResourceView(ResourceType type)
        {
            var rv = resourceViews.Find(x => x.m_Type == type);

            if (rv == null)
            {
                if (!isTrivial)
                    this.LogWarning($"Couldnt find ResourceView of Type | {type} | for {m_ItemSO.m_ID}");
                rv = resourceViews[0];
            }

            return rv;
        }

        void SwitchResourceViews()
        {
#if ANIMATIONSEQ_ENABLED
            foreach (var item in resourceViews)
            {
                item.m_Appearer?.DOKill();
            }

            foreach (var item in resourceViews)
            {
                if (item == currentResourceView)
                {
                    item.m_Appearer?.z_DefualtAndAppear();
                    item.m_OnActivationEvents?.Invoke();
                }
                else
                    item.m_Appearer?.Disappear();
            }
#endif
        }




        public void z_InitAndAppear()
        {
            Init();
#if ANIMATIONSEQ_ENABLED
            m_MainAppearer?.Appear();
#endif
        }
        public void z_InitAndDefaultAppear()
        {
            Init();
#if ANIMATIONSEQ_ENABLED
            m_MainAppearer?.z_DefualtAndAppear();
#endif
        }
        public void z_Default()
        {
#if ANIMATIONSEQ_ENABLED
            m_MainAppearer?.Default();
#endif
        }

        public void z_Appear()
        {
#if ANIMATIONSEQ_ENABLED
            m_MainAppearer?.Appear();
#endif
        }
        public void z_Disappear()
        {
#if ANIMATIONSEQ_ENABLED
            m_MainAppearer?.Disappear();
#endif
        }
    }

}