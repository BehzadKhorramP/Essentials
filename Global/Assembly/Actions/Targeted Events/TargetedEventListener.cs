using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace MadApper
{
    public class TargetedEventListener : MonoBehaviour
    {
        [Space(10), SerializeReference] public TargetedEventListenerBase m_Listener;

        private void OnEnable()
        {
            m_Listener?.OnEnable();
        }
        private void OnDisable()
        {
            m_Listener?.OnDisable();
        }
    }






    [Serializable]
    public abstract class TargetedEventListenerBase
    {
        [HideInInspector] public string name;

        public TargetedEventListenerBase()
        {
            name = GetName();
        }
        public abstract string GetName();

        public abstract void OnEnable();
        public abstract void OnDisable();
    }

    [Serializable]
    public abstract class TargetedEventListenerGeneric<TSO, TData> : TargetedEventListenerBase where TSO : GenericTargetedEventSO<TData>
    {
        [Space(10), SerializeField] public TSO m_SO;
        [Space(10), SerializeField] public List<TSO> m_SoList;      
        [Space(10), SerializeField] public List<UnityEventDelay<TData>> m_Actions;

        public override void OnEnable()
        {
            if (m_SO != null)
                m_SO.m_OnEventRaised += OnEventRaised;

            if (m_SoList != null)
                foreach (var item in m_SoList)
                    item.m_OnEventRaised += OnEventRaised;

        }
        public override void OnDisable()
        {
            if (m_SO != null)
                m_SO.m_OnEventRaised -= OnEventRaised;


            if (m_SoList != null)
                foreach (var item in m_SoList)
                    item.m_OnEventRaised -= OnEventRaised;


            foreach (var item in m_Actions)
                item?.Dispose();
        }

        protected virtual void OnEventRaised(TData data)
        {
            foreach (var item in m_Actions)
            {
                item?.Invoke(data);
            }
        }
    }

    [Serializable]
    public class TargetedEventListener_Void : TargetedEventListenerBase
    {
        [Space(10), SerializeField] public TargetedEventSO m_SO;
        [Space(10), SerializeField] public List<TargetedEventSO> m_SoList;
        [Space(10), SerializeField] public UnityEvent m_Action;

        public override string GetName() => "VOID Listener";

        public override void OnEnable()
        {
            if (m_SO != null)
                m_SO.m_OnEventRaised += OnEventRaised;

            if (m_SoList != null)
                foreach (var item in m_SoList)
                    item.m_OnEventRaised += OnEventRaised;

        }
        public override void OnDisable()
        {
            if (m_SO != null)
                m_SO.m_OnEventRaised -= OnEventRaised;

            if (m_SoList != null)
                foreach (var item in m_SoList)
                    item.m_OnEventRaised -= OnEventRaised;

        }

        void OnEventRaised()
        {
            m_Action?.Invoke();
        }
    }

    [Serializable]
    public class TargetedEventListener_String : TargetedEventListenerGeneric<TargetedEventSO_String, string>
    {
        public override string GetName() => "STRING Listener";
    }


    [Serializable]
    public class TargetedEventListener_Bool : TargetedEventListenerGeneric<TargetedEventSO_Bool, bool>
    {
        public override string GetName() => "BOOL Listener";
    }

    [Serializable]
    public class TargetedEventListener_Int : TargetedEventListenerGeneric<TargetedEventSO_Int, int>
    {
        public override string GetName() => "INT Listener";
    }
    [Serializable]
    public class TargetedEventListener_Float : TargetedEventListenerGeneric<TargetedEventSO_Float, float>
    {
        public override string GetName() => "FLOAT Listener";
    }

    [Serializable]
    public class TargetedEventListener_Transform : TargetedEventListenerGeneric<TargetedEventSO_Transform, Transform>
    {
        public override string GetName() => "TRANSFORM Listener";
    }






#if UNITY_EDITOR

    [CustomEditor(typeof(TargetedEventListener))]
    public class TargetedEventListenerEditor : Editor
    {
        List<TargetedEventListenerBase> list;
        List<TargetedEventListenerBase> m_List
        {
            get
            {
                if (list == null || list.Count == 0)
                {
                    list = this.GetAllDerivedInstancesInAllAssemblies<TargetedEventListenerBase>().ToList();
                }
                return list;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.Space(30);

            TargetedEventListener t = (TargetedEventListener)target;

            for (int i = 0; i < m_List.Count; i++)
            {
                var rv = m_List[i];

                var isSelected = t.m_Listener != null && rv.GetType() == t.m_Listener.GetType();

                GUI.backgroundColor = isSelected ? Color.green : Color.white;

                if (GUILayout.Button($"SET : {rv.GetName()}"))
                {
                    t.m_Listener = rv;
                    EditorUtility.SetDirty(t);
                }
            }

            serializedObject.ApplyModifiedProperties();

        }

    }
#endif
}