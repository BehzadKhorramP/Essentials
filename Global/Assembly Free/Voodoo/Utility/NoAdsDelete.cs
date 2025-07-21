using UnityEngine;

#if VOODOOSAUCE_ENABLED
using Sirenix.OdinInspector;
using Voodoo.Sauce.IAP;
#endif

namespace BEH.Voodoo
{
    public class NoAdsDelete : MonoBehaviour
    {
#if VOODOOSAUCE_ENABLED
        [Button]
        public void DeleteNoAds()
        {
            PremiumHelper.DisablePremium();
        }
#endif
    }
}
