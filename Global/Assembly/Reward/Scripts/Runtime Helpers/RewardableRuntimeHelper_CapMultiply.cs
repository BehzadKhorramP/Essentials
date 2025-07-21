using TMPro;
using UnityEngine;

namespace BEH.Reward
{

    public class RewardableRuntimeHelper_CapMultiply : RewardableRuntimeHelper
    {
        [SerializeField] TextMeshProUGUI m_Text;

        private void OnEnable()
        {
            z_OnAdjustMultiplyOfCapText();
        }

        // it's also called from OnShopToInitialize EventSO
        public void z_OnAdjustMultiplyOfCapText()
        {
            if (m_Rewardable == null)
                return;

            var data = m_Rewardable.m_RewardData;

            var modAmount = GetMultiplyofCapAmount(data);

            if (!modAmount.HasValue)
                return;

            m_Text?.SetText($"({modAmount})");
        }


    }

}