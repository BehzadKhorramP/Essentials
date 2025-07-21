using MadApper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BEH
{
    public class DebuggingObject : MonoBehaviour
    {
        [SerializeField][AutoGetOrCreateSO] DebugSettingsSO m_SettingsSO;
        [SerializeField] Transform m_Parent;
        private void Awake()
        {
            m_Parent.gameObject.SetActive(m_SettingsSO.Value.IsDebug);
        }

        public void z_CorrectDebugCode()
        {
            m_SettingsSO.Value.IsDebug = true;
            m_Parent.gameObject.SetActive(true);
        }
    }
}
