using BEH;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using UnityEngine;

namespace MadApper
{
    public abstract class GameContextHelper : MonoBehaviour
    {
        [FoldoutGroup("Refs")][SerializeField][AutoGetOrCreateSO][ReadOnly] protected GameContextManager m_ContextMng;
        [FoldoutGroup("Refs")][SerializeField][AutoGetOrAdd][ReadOnly] protected UnityEventDelayRaiser m_EventRaiser;
        [PropertySpace(10, 10)][SerializeField] protected bool executeInEditor;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            if (m_EventRaiser == null) return;

            m_EventRaiser.m_Type = UnityEventDelayRaiser.Type.OnEnable;
            m_EventRaiser.m_Events = new List<UnityEventDelay>() { new UnityEventDelay() { m_Event = new UnityEngine.Events.UnityEvent() } };

            UnityEventTools.AddVoidPersistentListener(m_EventRaiser.m_Events[0].m_Event, Execute);

            this.TrySetDirty();
            m_EventRaiser.TrySetDirty();

            if (executeInEditor) Execute();
        }
#endif

        void Execute()
        {
            var activeContext = m_ContextMng.GetContext();
            ExecuteInternal(activeContext);
        }

        protected abstract void ExecuteInternal(GameContext activeContext);
    }
}
