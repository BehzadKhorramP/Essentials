using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper
{
    public class UnityEventDelayRaiser : MonoBehaviour
    {

        [Flags]
        public enum Type
        {
            OnEnable = 1,
            Awake = 2,
            Start = 4,
            OnDisable = 8,
            Manual = 16,
            All = OnEnable | Awake | Start | OnDisable | Manual
        }

        [Space(10)] public Type m_Type;

        [Space(10)] public List<UnityEventDelay> m_Events;


        private void OnEnable()
        {
            if ((m_Type & Type.OnEnable) != Type.OnEnable)
                return;

            RaiseEvents();
        }
        private void Awake()
        {
            if ((m_Type & Type.Awake) != Type.Awake)
                return;

            RaiseEvents();
        }
        private void Start()
        {
            if ((m_Type & Type.Start) != Type.Start)
                return;

            RaiseEvents();
        }

        private void OnDisable()
        {
            if ((m_Type & Type.OnDisable) != Type.OnDisable)
                return;

            RaiseEvents();
        }

        [Button]
        public virtual void RaiseEvents()
        {
            foreach (var item in m_Events)
                item.Invoke();
        }

        public void Stop() => m_Events.ForEach(x => x.Stop());

        public void z_RaiseEvents() => RaiseEvents();
    }




}