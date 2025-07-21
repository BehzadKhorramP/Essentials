using MadApper;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BEH.VisualsPreset
{

    [CreateAssetMenu(fileName = "VisualsPresetSO", menuName = "Visuals/VisualsPresetSO")]
    public class VisualsPresetSO : ScriptableObject
    {
        [SerializeField] IDSO m_ID;
        [SerializeField] List<VisualsSetSO> m_Sets;

        public IDSO GetIDedSO() => m_ID;

        public TVisualSetSO GetProperVisualSetSO<TVisualSetSO, TValue>(TVisualSetSO orgVisualSetSO)
            where TVisualSetSO : ScriptableObject, IVisualSetSO<TValue>

        {
            var res = m_Sets.Find(x => x.ID.Equals(orgVisualSetSO.ID));

            if (res is not TVisualSetSO casted)
                return null;

            return casted;
        }
    }

    public interface IVisualSetSO<TValue>
    {
        public string ID { get; set; }
        public void UpdateVisuals(UnityEventDelayList<TValue> @event);
    }

    public abstract class VisualsSetSO : ScriptableObject
    {
        public string ID;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ID))
            {
                ID = name;
                EditorUtility.SetDirty(this);
            }
        }
#endif       

    }


    public abstract class VisualsSet<TValue> : VisualsSetSO, IVisualSetSO<TValue>
    {
        [SerializeField] protected TValue m_Value;

        string IVisualSetSO<TValue>.ID { get => ID; set { } }

        public TValue GetValue() => m_Value;

        public void UpdateVisuals(UnityEventDelayList<TValue> @event) => @event?.Invoke(m_Value);

    }
}
