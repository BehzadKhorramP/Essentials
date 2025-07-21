using UnityEngine;
using System.Collections.Generic;
#if GAMEANALYTICSSDK_ENABLED
using GameAnalyticsSDK;
#endif

namespace MadApper
{

    public class ResourceTracking_GA : MonoBehaviour, IResourceTracking
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
            return;
#endif

#if GAMEANALYTICSSDK_ENABLED
            if (amount == 0)
                return;

            var payout = ResourceTrackingExtention.GetPayout(inspectorPayout: m_Payout, virtualPayout: GetVirtualPayout(), options: options);

            GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, resource.m_ID, amount, m_Placement.m_ID, payout);

            this.LogBlue($"GAMEANALYTICS : SINK - Resource: {resource.m_ID} -Amount: {amount} -Placement: {m_Placement.m_ID} " +
                $"-Payout: {payout}");

            SetOptions(null);
#endif

        }

        public void Source(ResourceItemSO resource, int amount, ResourceType resourceType = ResourceType.Simple)
        {

#if TINYSAUCE_ENABLED
            return;
#endif

#if GAMEANALYTICSSDK_ENABLED
            if (amount == 0)
                return;

            var payout = ResourceTrackingExtention.GetPayout(inspectorPayout: m_Payout, virtualPayout: GetVirtualPayout(), options: options);

            GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, resource.m_ID, amount, m_Placement.m_ID, payout);

            this.LogBlue($"GAMEANALYTICS : SOURCE - Resource: {resource.m_ID} -Amount: {amount} -Placement: {m_Placement.m_ID} " +
                 $"-Payout: {payout}");

            SetOptions(null);
#endif
        }




        #endregion

    }

}