#if VOODOOSAUCE_ENABLED

namespace BEH.Ads
{
    public class PurchaseByWatchRVObserver_Voodoo : PurchaseByWatchRVObserver
    {

        public override void Initialize()
        {
            base.Initialize();

            VoodooSauce.OnRewardedVideoButtonShown(rewardedType: m_Placement.m_ID);
        }

        public override bool IsAvailable()
        {
            return VoodooSauce.IsRewardedVideoAvailable();
        }

        public override void TryWatchRV()
        {
            VoodooSauce.ShowRewardedVideo(RewardedCallback, rewardedType: m_Placement.m_ID);
        }

        private void RewardedCallback(bool succeed)
        {
            var actions = GetActions();

            if (succeed)
            {
                actions.onSucceed?.Invoke();
            }
            else
            {
                actions.onFailed?.Invoke();
            }
        }
    }
}

#endif