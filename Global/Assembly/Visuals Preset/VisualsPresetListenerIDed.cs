using MadApper;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BEH.VisualsPreset
{
    public class VisualsPresetListenerIDed : MonoBehaviour
    {
        [PropertySpace(10,20)]
        [Title("Setup")]
        [SerializeField] bool m_SwitchOnEnable = true;
        [SerializeField] UnityEventDelayList<IDSO> onUpdateVisuals;
        [SerializeField] List<Switch> m_Switches;

        [SerializeField, ReadOnly, AutoGetOrCreateSO] protected VisualsPresetSettingsSO m_VisualsPresetSettingsSO;

        private void Awake()
        {
            if (m_VisualsPresetSettingsSO == null)
            {
                $"visual preset settings is null on {name}".LogWarning();
                return;
            }

            var idSO = m_VisualsPresetSettingsSO.Value.GetIDedSO();

            onUpdateVisuals?.Invoke(idSO);
        }

        private void OnEnable()
        {
            if (m_VisualsPresetSettingsSO == null)
            {
                $"visual preset settings is null on {name}".LogWarning();
                return;
            }
            var idSO = m_VisualsPresetSettingsSO.Value.GetIDedSO();

            if (m_SwitchOnEnable)
                z_Switch(idedSO: idSO);

        }




        public void z_Switch(IDSO idedSO)
        {
            var f = m_Switches.Find(x => x.IDSO == idedSO);

            if (f == null)
                return;

            foreach (var item in m_Switches)            
                item.GameObject.SetActive(false);

            f.GameObject.SetActive(true);            
        }

        [Serializable]
        public class Switch
        {
            public IDSO IDSO;
            public GameObject GameObject;
        }
    }
}
