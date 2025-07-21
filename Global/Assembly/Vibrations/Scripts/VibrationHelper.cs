using Lofelt.NiceVibrations;
using UnityEngine;

namespace MadApper.Yann {
    public class VibrationHelper : MonoBehaviour {
        private static bool isVibrating;
        [Space(10)]
        [Tooltip("Vibration settings to use")]
        public VibrationSO m_Data;

        public static bool s_IsVibratorOn {
            get {
                return GameSettingsData.IsVibrationOn;
            }
        }

        private void OnVibrate() {
            if (m_Data == null) {
                return;
            }

            PlayVibration(m_Data);
        }

        public void z_OnVibrate() {
            OnVibrate();
        }

        public void z_StopVibration() {
            StopVibration();
        }

        /// <summary>
        ///     Plays a vibration using the specified vibration settings
        /// </summary>
        public static bool PlayVibration(VibrationSO vibrationSO) {
            if (!s_IsVibratorOn || vibrationSO == null) {
                return false;
            }

            if (vibrationSO.usePreset) {
                HapticPatterns.PlayPreset(vibrationSO.preset);
            }
            else {
                #if UNITY_ANDROID
                float adjustedAmplitude = Mathf.Clamp(vibrationSO.amplitude - 0.1f, 0.1f, 1f);
                HapticPatterns.PlayConstant(adjustedAmplitude, vibrationSO.frequency, vibrationSO.duration);
                isVibrating = true;
                #else
                HapticPatterns.PlayConstant(vibrationSO.amplitude, vibrationSO.frequency, vibrationSO.duration);
                #endif
            }

            return true;
        }

        /// <summary>
        ///     Stops any ongoing vibration
        /// </summary>
        public static void StopVibration() {
            if (!isVibrating) {
                return;
            }

            HapticController.Stop();
            isVibrating = false;
        }

        // Legacy methods for backward compatibility
        public static void Vibrate(HapticPatterns.PresetType preset) {
            if (!s_IsVibratorOn) {
                return;
            }

            HapticPatterns.PlayPreset(preset);
        }

        public static void Vibrate(float amplitude, float frequency) {
            Vibrate(amplitude, frequency, 0.01f);
        }

        public static void Vibrate(float amplitude, float frequency, float duration) {
            if (!s_IsVibratorOn) {
                return;
            }

            #if UNITY_ANDROID
            amplitude = Mathf.Clamp(amplitude - 0.1f, 0.1f, 1f);
            HapticPatterns.PlayConstant(amplitude, frequency, duration);
            isVibrating = true;
            #else
            HapticPatterns.PlayConstant(amplitude, frequency, duration);
            #endif
        }
    }
}