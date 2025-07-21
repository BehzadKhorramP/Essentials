
using System.Collections.Generic;
using UnityEngine;

#if VOODOOSAUCE_ENABLED
using Voodoo.Sauce.Core;
#endif


namespace BEH.Voodoo
{

    public class VoodooSauceUtilityEvents : MonoBehaviour
    {
#if VOODOOSAUCE_ENABLED
        public List<UnityEventDelay> m_OnInitialized;


        public void Start()
        {

            VoodooSauce.SubscribeOnInitFinishedEvent(OnInitialized);
        }

        private void OnInitialized(VoodooSauceInitCallbackResult obj)
        {
            VoodooSauce.UnSubscribeOnInitFinishedEvent(OnInitialized);

            foreach (var item in m_OnInitialized)
                item?.Invoke();
        }
#endif 
    }

}