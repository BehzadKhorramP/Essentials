using MadApper;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BEH.VisualsPreset
{
    public abstract class VisualsPresetListener<TVisualsSetSO, TValue> : MonoBehaviour
        where TVisualsSetSO : ScriptableObject, IVisualSetSO<TValue>
    {

        [SerializeField] TVisualsSetSO m_OriginalSetSO;
        [SerializeField] UnityEventDelayList<TValue> onUpdateVisuals;
        [SerializeField, ReadOnly, AutoGetOrCreateSO] protected VisualsPresetSettingsSO m_VisualsPresetSettingsSO;

        private void Awake()
        {
            if (m_VisualsPresetSettingsSO == null)
            {
                $"visual preset settings is null on {name}".LogWarning();
                return;
            }

            var visualSetSO = m_VisualsPresetSettingsSO.Value.GetProperVisualSetSO<TVisualsSetSO, TValue>(m_OriginalSetSO);

            if (visualSetSO == null)
                return;

            UpdateVisuals(visualSetSO);
        }

        protected virtual void UpdateVisuals(TVisualsSetSO visualSet)
        {
            visualSet.UpdateVisuals(onUpdateVisuals);
        }
    }
}
