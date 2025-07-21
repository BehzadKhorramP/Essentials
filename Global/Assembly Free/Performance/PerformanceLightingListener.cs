using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper
{
    public class PerformanceLightingListener : MonoBehaviour
    {
        [SerializeField][AutoGetOrAdd] Light m_Light;


        public void z_OnDeactiveShadows()
        {
            m_Light.shadows = LightShadows.None;
        }
    }
}
