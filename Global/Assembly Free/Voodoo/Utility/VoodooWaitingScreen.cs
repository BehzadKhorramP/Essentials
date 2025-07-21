using MadApper;
using System.Collections.Generic;
using UnityEngine;

namespace BEH.Voodoo
{
    public class VoodooWaitingScreen : MonoBehaviour
    {      
        public List<UnityEventDelay> m_OnStarted;
        public List<UnityEventDelay> m_OnFinished;

        public void z_OnStarted()
        {           
            foreach (var item in m_OnStarted)
                item?.Invoke();
        }

        public void z_OnFinished()
        {            
            foreach (var item in m_OnFinished)
                item?.Invoke();
        }
    }
}
