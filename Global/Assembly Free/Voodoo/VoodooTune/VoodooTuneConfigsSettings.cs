using MadApper;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BEH.Voodoo
{
    public class VoodooTuneConfigsSettings : SingletonScriptable<VoodooTuneConfigsSettings>, IPreBuildValidate
    {
        [Space(10)] public VoodooTuneConfigSO[] m_AllSOs;
        [Space(10)] public List<SceneSO> m_ScenesValidForInitialization;


#if UNITY_EDITOR
        [MenuItem("BEH/Voodoo/VoodooTuneConfigs Settings", false, 100)]
        static void EditSettings()
        {
            Selection.activeObject = GetSO();
        }    

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            var length = 0;

            if (m_AllSOs != null)
                length = m_AllSOs.Length;

            m_AllSOs = m_AllSOs.GetAllInstances_Editor();

            if (length != m_AllSOs.Length)
                EditorUtility.SetDirty(this);
        }
#endif

    }
}
