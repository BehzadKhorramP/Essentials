using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Essentials
{
    public class UnityEventDelayRaiserPlatformBased : UnityEventDelayRaiser
    {
        [Space(10)] public List<UnityEventDelay> m_iOSEvents;
        [Space(10)] public List<UnityEventDelay> m_AndroidEvents;
        [Space(10)] public UnityEventDelayList editorEvents;
        [Space(10)] public UnityEventDelayList nonEditorEvents;



        public override void RaiseEvents()
        {
#if UNITY_ANDROID
            foreach (var item in m_AndroidEvents)
                item.Invoke();
#else
            foreach (var item in m_iOSEvents)
                item.Invoke();
#endif

#if UNITY_EDITOR
            editorEvents?.Invoke();
#else
            nonEditorEvents?.Invoke();
#endif

        }
    }
}
