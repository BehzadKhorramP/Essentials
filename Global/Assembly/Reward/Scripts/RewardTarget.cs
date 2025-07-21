using MadApper;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BEH.Reward
{
    public class RewardTarget : TargetableBase<Rewardable>
    {
        public ResourceItemSO m_RewardItemSO;

        public List<ResourceItemSO> m_TempRewardItemSOList;


#if UNITY_EDITOR

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            var resource = GetComponentInParent<ResourcesView>();

            if (resource == null)
                return;

            if (resource.m_ItemSO != null && m_RewardItemSO != resource.m_ItemSO)
            {
                m_RewardItemSO = resource.m_ItemSO;
                EditorUtility.SetDirty(this);
            }
        }

#endif
        public override bool IsTargetExtra(Rewardable seeker)
        {
            if (m_RewardItemSO == null) return false;
            if (!seeker.IsRewardDataValid()) return false;
            if (!seeker.m_ResourceSO.IsEquals(m_RewardItemSO)) return false;

            return true;
        }

        public override bool IsTempTargetExtra(Rewardable seeker)
        {
            if (!seeker.IsRewardDataValid()) return false;

            foreach (var item in m_TempRewardItemSOList)
                if (seeker.m_ResourceSO.IsEquals(item))
                    return true;

            return false;
        }


    }

}