using UnityEngine;

namespace MadApper
{
    public class TargetedEventHelper : MonoBehaviour
    {
        [Space(10)] public TargetedEventSO m_SO;

        // Inspector Setup
        public void z_RaiseAction() => m_SO?.RaiseEvent();

    }

}