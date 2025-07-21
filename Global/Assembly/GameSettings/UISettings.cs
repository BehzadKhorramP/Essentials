using BEH;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MadApper
{
    public class UISettings : MonoBehaviour
    {
        [AutoGetOrAdd][ReadOnly][SerializeField] AppearerEvents m_AppearerEvents;

        [SerializeField] UISettingsToggle m_Music;
        [SerializeField] UISettingsToggle m_Sounds;
        [SerializeField] UISettingsToggle m_Vibration;

        // hooked to SettingsOpen targetedEvent
        public void z_Open()
        {
            if (m_Music != null)
                m_Music.Setup(this);

            if (m_Sounds != null)
                m_Sounds.Setup(this);

            if (m_Vibration != null)
                m_Vibration.Setup(this);

            m_AppearerEvents.z_Appear();
        }

        public void OnToggleTapped(UISettingsToggle obj)
        {
            var isEnabled = GetValue(obj);
            isEnabled = !isEnabled;
            SetValue(obj, isEnabled);

            obj.Refresh(isEnabled);

            // for events that should trigger only when tapped
            obj.RefreshOnTapped(isEnabled);
        }

        public bool GetValue(UISettingsToggle obj)
        {
            if (obj == m_Music)
                return GameSettingsData.IsMusicOn;
            else if (obj == m_Sounds)
                return GameSettingsData.IsSoundOn;
            else if (obj == m_Vibration)
                return GameSettingsData.IsVibrationOn;

            return true;
        }
        void SetValue(UISettingsToggle obj, bool value)
        {
            if (obj == m_Music)
                GameSettingsData.IsMusicOn = value;
            else if (obj == m_Sounds)
                GameSettingsData.IsSoundOn = value;
            else if (obj == m_Vibration)
                GameSettingsData.IsVibrationOn = value;
        }

        public void z_OnReplayAdditive()
        {
            ScenesLoader.Reload();
            m_AppearerEvents.z_Disappear();
        }
        public void z_OnReplaySimple()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
