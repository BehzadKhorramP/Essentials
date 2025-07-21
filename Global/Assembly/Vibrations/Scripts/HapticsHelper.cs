using Lofelt.NiceVibrations;
using UnityEngine;

namespace MadApper
{
    public class HapticsHelper : MonoBehaviour
    {
        public static bool s_IsVibratorOn => GameSettingsData.IsVibrationOn;

        public HapticPatterns.PresetType m_Preset = HapticPatterns.PresetType.Selection;
       

        public static void Vibrate(HapticPatterns.PresetType preset)
        {
            if (!s_IsVibratorOn)
                return;

            HapticPatterns.PlayPreset(preset);
        }

        public static void Vibrate(float amplitude, float frequency)
        {
            if (!s_IsVibratorOn)
                return;

#if UNITY_ANDROID
            amplitude = Mathf.Clamp(amplitude - 0.1f, 0.1f, 1f);
            HapticPatterns.PlayConstant(amplitude, frequency, 0.01f);
#else
            HapticPatterns.PlayEmphasis(amplitude, frequency);
#endif
        }

        public void z_Vibrate()
        {
            Vibrate(m_Preset);
        }
        public void z_VibrateOrdered(int index)
        {
            float hapticsValue = 0.1f + (0.1f * index);
            Vibrate(hapticsValue, 1f);
        }

    }

}