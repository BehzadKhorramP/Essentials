using MadApper;
using System.Collections.Generic;
using UnityEngine;


namespace MadApper
{
    public class ResourceTracking_TinySauce : MonoBehaviour, IResourceTracking
    {
        [Space(10)] public ResourcePlacementSO m_Placement;

        [Space(10)] public string m_Payout;

        public virtual string GetVirtualPayout() => "";


        #region IResourceTracking

        Dictionary<string, string> options;
        public virtual bool IsDestroyable => false;

        public void SetOptions(Dictionary<string, string> options = null) => this.options = options;
        public void Sink(ResourceItemSO resource, int amount, ResourceType resourceType = ResourceType.Simple)
        {

#if TINYSAUCE_ENABLED
            if (amount == 0)
                return;

            var payout = ResourceTrackingExtention.GetPayout(inspectorPayout: m_Payout, virtualPayout: GetVirtualPayout(), options: options);

            SetOptions(null);

#if UNITY_EDITOR
            this.LogBlue($"TINYSAUCE : SINK - Resource: {resource.m_ID} -Amount: {amount} -Placement: {m_Placement.m_ID} " +
                $"-Payout: {payout}");

            return;
#endif

            TinySauce.OnCurrencyTaken(currency: resource.m_ID, currencyAmount: amount, itemType: m_Placement.m_ID, itemId: payout);

#endif
        }

        public void Source(ResourceItemSO resource, int amount, ResourceType resourceType = ResourceType.Simple)
        {
#if TINYSAUCE_ENABLED
            if (amount == 0)
                return;

            var payout = ResourceTrackingExtention.GetPayout(inspectorPayout: m_Payout, virtualPayout: GetVirtualPayout(), options: options);

            SetOptions(null);

#if UNITY_EDITOR
            this.LogBlue($"TINYSAUCE : SOURCE - Resource: {resource.m_ID} -Amount: {amount} -Placement: {m_Placement.m_ID} " +
                $"-Payout: {payout}");

            return;
#endif

            TinySauce.OnCurrencyGiven(currency: resource.m_ID, currencyAmount: amount, itemType: m_Placement.m_ID, itemId: payout);

#endif
        }
        #endregion
    }
}