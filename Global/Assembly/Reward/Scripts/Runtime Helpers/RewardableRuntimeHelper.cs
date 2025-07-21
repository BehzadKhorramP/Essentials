using MadApper;
using UnityEngine;

namespace BEH.Reward
{
    [RequireComponent(typeof(Rewardable))]
    public class RewardableRuntimeHelper : MonoBehaviour
    {
        Rewardable rewardable;
        protected Rewardable m_Rewardable
        {
            get
            {
                if (rewardable == null)
                    rewardable = GetComponent<Rewardable>();
                return rewardable;
            }
        }

        public static int? GetMultiplyofCapAmount(RewardData data)
        {
            if (data == null || data.ResourceSO == null)
                return null;

            var item = data.ResourceSO;
            var orgAmount = data.Amount;
            return orgAmount * item.GetCap();
        }

        public static int? GetCurrentCap(RewardData data)
        {
            if (data == null || data.ResourceSO == null)
                return null;

            var item = data.ResourceSO;
            return item.GetCap();
        }
      
        public static int? GetAmountToReachCap(ResourceItemSO resource)
        {
            if (resource == null )
                return null;

            var amount = resource.GetAmount();
            var cap = resource.GetCap();

            var need = cap - amount;

            if (need <= 0)
                need = 0;

            return need;
        }





        // it may be called via Rewardable.m_OnPrepareRewardEvent
        public void z_ConvertAmountToMultiplyofCap()
        {
            if (m_Rewardable == null)
                return;

            var data = m_Rewardable.m_RewardData;

            var modAmount = GetMultiplyofCapAmount(data);

            if (!modAmount.HasValue)
                return;

            m_Rewardable.SetModifiedAmount(modAmount.Value);
        }



        // it may be called via Rewardable.m_OnPrepareRewardEvent
        public void z_ModifyToNeededAmountToFillTheCap()
        {
            if (!m_Rewardable.IsRewardDataValid())
                return;

            var resource = m_Rewardable.m_RewardData.ResourceSO;

            var modAmount = GetAmountToReachCap(resource);

            if (!modAmount.HasValue)
                return;

            if (modAmount.Value <= 0)
                return;

            m_Rewardable.SetModifiedAmount(modAmount.Value);
        }
    }

}