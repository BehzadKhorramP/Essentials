using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper
{
    public class DebugForcedManualFPS : MonoBehaviour
    {
        [SerializeField] PerformanceHelper helper;
        [SerializeField] int fps;

        private void OnEnable()
        {
            if (helper != null) helper.gameObject.SetActive(false);

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = fps;
        }
    }

}