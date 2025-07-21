using MadApper;
using System;
using UnityEngine;

namespace BEH
{
    public class PrivacyPolicy : MonoBehaviour
    {
        public const string k_PP = "https://madapperstudios.blogspot.com/2022/09/private-policy.html";
        const string k_PPAccepted = "PrivacyPolicyAccepted";

        [Space(10)] public UnityEventDelay m_ShowPirvacyPolicy;
        [Space(10)] public UnityEventDelay m_OnAccepted;

        bool IsAcceptedAlready()
        {

#if UNITY_IOS
            return true;
#endif

            if (PlayerPrefs.HasKey(k_PPAccepted))
                return PlayerPrefs.GetInt(k_PPAccepted) > 0;
            return false;
        }

        [ContextMenu("DeleteSavedData")]
        public void DeleteSavedData()
        {
            if (PlayerPrefs.HasKey(k_PPAccepted))
                PlayerPrefs.DeleteKey(k_PPAccepted);
        }
        void SaveAccepted()
        {
            PlayerPrefs.SetInt(k_PPAccepted, 1);
        }

        [ContextMenu("TryShow")]

        public void z_TryShowPrivacyPolicyPopUp()
        {
            if (IsAcceptedAlready())
            {
                m_OnAccepted?.Invoke();
                return;
            }

            m_ShowPirvacyPolicy?.Invoke();
        }

        public void z_OnOpenPrivacyPolicy()
        {
            Application.OpenURL(k_PP);
        }

        public void z_OnAccpeted()
        {
            SaveAccepted();
            m_OnAccepted?.Invoke();
        }
    }

}