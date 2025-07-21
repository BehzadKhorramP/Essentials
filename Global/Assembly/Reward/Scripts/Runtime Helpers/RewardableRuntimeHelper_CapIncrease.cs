using TMPro;
using UnityEngine;

namespace BEH.Reward
{
    public class RewardableRuntimeHelper_CapIncrease : RewardableRuntimeHelper
    {
        [SerializeField] TextMeshProUGUI m_FromText;
        [SerializeField] TextMeshProUGUI m_ToText;

      //  public bool IsAddtionalReward;

        private void OnEnable()
        {
            z_OnAdjustCapFromToIncreaseText();
        }


        // it's also called from OnShopToInitialize EventSO
        public void z_OnAdjustCapFromToIncreaseText()
        {
            if (m_Rewardable == null)
                return;


            RewardData data = m_Rewardable.m_RewardData;

            //if (IsAddtionalReward)
            //{
            //    if (m_Rewardable.m_AdditionalRewardData != null && m_Rewardable.m_AdditionalRewardData.Count > 0)
            //    {
            //        data = m_Rewardable.m_AdditionalRewardData[0].RewardData;
            //    }               
            //}

            var fromCap = GetCurrentCap(data);

            if (fromCap.HasValue)
            {
                var toCap = data.Amount + fromCap;

                m_FromText?.SetText($"{fromCap.Value}");
                m_ToText?.SetText($"{toCap.Value}");
            }
        }
    }
}
