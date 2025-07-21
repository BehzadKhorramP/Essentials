
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace MadApper
{
   
    public class AppearerEvents : MonoBehaviour
    {
        [SerializeField] UnityEventDelayList m_AppearEvents;
        [SerializeField] UnityEventDelayList m_DisappearEvents;
        [SerializeField] UnityEventDelayList m_DefaultEvents;

        [Button]
        public void z_Appear() => m_AppearEvents?.Invoke();
        [Button]
        public void z_Disappear() => m_DisappearEvents?.Invoke();
        [Button]
        public void z_Defualt() => m_DefaultEvents?.Invoke();


        [Button]
        public void z_StopAll()
        {
            m_AppearEvents.Stop();
            m_DisappearEvents.Stop();
            m_DefaultEvents.Stop();
        }
    }


}
