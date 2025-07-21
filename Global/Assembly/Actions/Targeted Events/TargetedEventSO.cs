
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace MadApper
{

    [CreateAssetMenu(fileName = "TargetedEventSO", menuName = "Targeted Event/TargetedEventSO")]

    public class TargetedEventSO : ScriptableObject
    {
        [Space(10)] public UnityAction m_OnEventRaised;

        [Button]
        public void RaiseEvent()
        {
            m_OnEventRaised?.Invoke();
        }
    }

   
    public class GenericTargetedEventSO<T> : ScriptableObject
    {
        [Space(10)] public UnityAction<T> m_OnEventRaised;

        [Space(10)] public T m_SerializedObj;

        [Button]
        public void RaiseEvent(T parameter)
        {
            m_OnEventRaised?.Invoke(parameter);
        }
        public void RaiseEvent()
        {
            if (m_SerializedObj == null)
            {
                this.LogWarning($"Serialized Obj is null in {name}");
                return;
            }

            m_OnEventRaised?.Invoke(m_SerializedObj);
        }
    }

}