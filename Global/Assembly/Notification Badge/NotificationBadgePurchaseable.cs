using System;
using UnityEngine;

namespace MadApper.Essentials
{
    public class NotificationBadgePurchaseable : NotificationBadge
    {
        [SerializeField] PurchaseByResourceObserver m_Purchaser;

        protected override void OnEnable()
        {
            base.OnEnable();
            ResourcesDataManager.onResourceChanged += OnResourceChanged_NotifBadgePurchaser;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            ResourcesDataManager.onResourceChanged -= OnResourceChanged_NotifBadgePurchaser;
        }

        private void OnResourceChanged_NotifBadgePurchaser(string arg1, ResourceData arg2, ResourceType arg3)
        {
            z_Refresh();
        }

        public override bool ShouldShow()
        {
            if (m_Purchaser == null) return false;
            return m_Purchaser.HasEnoughResourceToPurchase();
        }
    }
}
