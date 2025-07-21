using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if TINYSAUCE_ENABLED
using Voodoo.Tiny.Sauce.Internal;
#endif

namespace MadApper
{
    public class TinySauceUtility : MonoBehaviour
    {
#if TINYSAUCE_ENABLED

        [SerializeField] UnityEventDelayList onTinySauceInitFinished;


        private void Start()
        {
            TinySauce.SubscribeOnInitFinishedEvent(OnInitFinished);
        }

        private void OnInitFinished(bool arg1, bool arg2)
        {
            TinySauce.UnsubscribeOnInitFinishedEvent(OnInitFinished);

            onTinySauceInitFinished?.Invoke();
        }

#endif
    }
}
