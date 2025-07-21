using BEH;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper
{
    public class UISettingsToggle : MonoBehaviour
    {

        [SerializeField] UnityEventDelayList m_OnEnabled;
        [SerializeField] UnityEventDelayList m_OnDisabled;

        [SerializeField] UnityEventDelayList m_OnEnabledOnTapped;
        [SerializeField] UnityEventDelayList m_OnDisabledOnTapped;

        UISettings uiSettings;

        public void Setup(UISettings uiSettings)
        {
            this.uiSettings = uiSettings;

            Refresh(this.uiSettings.GetValue(this));
        }

        public void Refresh(bool isEnabled)
        {
            if (isEnabled)
                m_OnEnabled?.Invoke();
            else
                m_OnDisabled?.Invoke();
        }
        public void RefreshOnTapped(bool isEnabled)
        {
            if (isEnabled)
                m_OnEnabledOnTapped?.Invoke();
            else
                m_OnDisabledOnTapped?.Invoke();
        }
        public void z_OnTapped()
        {
            uiSettings.OnToggleTapped(this);
        }
    }
}
