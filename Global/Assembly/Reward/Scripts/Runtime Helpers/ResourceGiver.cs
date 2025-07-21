using BEH.Reward;
using MadApper;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BEH
{
    public class ResourceGiver : MonoBehaviour
    {
        public enum Type { ScreenPos, WorldPos, SelfPos }

        [Space(10)] public Type m_Type;
        [Space(10)] public Rewardable m_Rewardable;
        [Space(10)] public Transform m_SourcePos;
        [Space(10)] public RewardTarget m_ManualTarget;
        [Space(10)] public bool m_BlockScreen;
        [Space(20)] public List<UnityEventDelay> m_OnResourceGiven;

        Vector3? selfPos;
        Vector3? worldPos;
        Vector3? screenPos;

        public IEnumerable<IResourceTracking> GetIResourceTrackings() => m_Rewardable.GetIResourceTrackings();

        void SetPos(Vector3 pos, Type type)
        {
            NullifyPoses();

            switch (type)
            {
                case Type.ScreenPos:
                    screenPos = pos;
                    break;
                case Type.WorldPos:
                    worldPos = pos;
                    break;
                case Type.SelfPos:
                    selfPos = pos;
                    break;
            }
        }
        void NullifyPoses() => screenPos = worldPos = selfPos = null;

        public void SetPosByGiver()
        {
            var pos = m_SourcePos != null ? m_SourcePos.position : transform.position;
            SetPos(pos, m_Type);
        }
        public void SetupReward(Vector3 pos, Type type, int rewardAmount)
        {
            SetPos(pos, type);
            SetupReward(rewardAmount);
        }

        public void SetupReward(int rewardAmount)
        {
            m_Rewardable.SetModifiedAmount(rewardAmount);          
            m_Rewardable.TryRefreshText();
        }

        public void SetManualTarget(RewardTarget target) => m_ManualTarget = target;


        public void GiveResources(Dictionary<string, string> trackingOptions = null, Action onComplete = null)
        {
            ModifyTrackingAndCompleteAction(out Action onRewardClaimed, trackingOptions: trackingOptions, onComplete: onComplete);

            var data = new RewardSystem.RewardsWorldSpaceData()
            {
                WorldPos = worldPos,
                ScreenPos = screenPos,
                SelfPos = selfPos,
                Rewardable = m_Rewardable,
                ManualTarget = m_ManualTarget,
                BlockScreen = m_BlockScreen,
                OnRewardClaimed = onRewardClaimed
            };

            RewardSystem.OnPutRewardToScreenPos(data);
        }
        public void GiveResourcesViaRewardSystem(Dictionary<string, string> trackingOptions = null, Action onComplete = null)
        {
            var rewards = new List<Rewardable>() { m_Rewardable };

            ModifyTrackingAndCompleteAction(out Action onRewardClaimed, trackingOptions: trackingOptions, onComplete: onComplete);

            var rewardsToClaimData = new RewardSystem.RewardsToClaimData
            {
                Rewards = rewards,
                OnAllRewardsClaimed = onRewardClaimed
            };

            RewardSystem.OnPutRewardsToLayout(rewardsToClaimData);
        }


        public void GiveResourcesBehindTheScene(int rewardAmount, Dictionary<string, string> trackingOptions = null)
        {
            var trackings = GetIResourceTrackings();

            var resource = m_Rewardable.m_RewardData.ResourceSO;
            var amount = rewardAmount;

            resource.Modify(rewardAmount);

            if (trackings != null)
                foreach (var track in trackings)
                {
                    track?.SetOptions(trackingOptions);
                    track?.Source(resource: resource, amount: amount);
                }
        }



        public void ModifyTrackingAndCompleteAction(out Action onRewardClaimed,
           Dictionary<string, string> trackingOptions = null, Action onComplete = null)
        {
            var trackings = m_Rewardable.GetIResourceTrackings();

            if (trackings != null)
                foreach (var item in trackings)
                    item.SetOptions(trackingOptions);

            onRewardClaimed = () =>
            {
                onComplete?.Invoke();

                if (m_OnResourceGiven != null)
                    foreach (var item in m_OnResourceGiven)
                        item?.Invoke();
            };
        }

        [Button]
        public void z_GiveResourceByGiverParams()
        {
            SetPosByGiver();
            GiveResources();
        }

        [Button]
        public void z_GiveResourceViaRewardSystem()
        {
            GiveResourcesViaRewardSystem();
        }
        public void z_GiveResourcesBehindTheScenes()
        {
            var amount = m_Rewardable.m_RewardData.Amount;
            GiveResourcesBehindTheScene(amount);
        }


        public void z_GiveResourceToReachCapBehindTheScene()
        {
            var resource = m_Rewardable.m_RewardData.ResourceSO;
            var amount = RewardableRuntimeHelper.GetAmountToReachCap(resource);

            if (!amount.HasValue || amount.Value <= 0)
                return;

            resource.Modify(amount.Value);

            var trackings = GetIResourceTrackings();

            if (trackings != null)
                foreach (var track in trackings)
                    track?.Source(resource: resource, amount: amount.Value);
        }






    }

}