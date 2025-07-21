using UnityEngine;
using Lofelt.NiceVibrations;
using Sirenix.OdinInspector;

namespace MadApper
{
    [CreateAssetMenu(fileName = "New Vibration", menuName = "MadApper/Vibration")]
    public class VibrationSO : ScriptableObject
    {
        [Title("Vibration Type")]
        [Tooltip("Use a preset vibration pattern instead of custom settings")]
        public bool usePreset = false;
        
        [ShowIf("usePreset")]
        [Title("Preset Settings")]
        [Tooltip("Preset vibration pattern to use")]
        public HapticPatterns.PresetType preset = HapticPatterns.PresetType.Selection;
        
        [HideIf("usePreset")]
        [Title("Custom Vibration Settings")]
        [Range(0f, 1f)]
        [Tooltip("Strength of the vibration (0-1)")]
        public float amplitude = 0.5f;
        
        [HideIf("usePreset")]
        [Range(0f, 1f)]
        [Tooltip("Frequency of the vibration (0-1)")]
        public float frequency = 0.5f;
        
        [HideIf("usePreset")]
        [Range(0.01f, 1f)]
        [Tooltip("Duration of the vibration in seconds (Android only)")]
        public float duration = 0.01f;
        
        /// <summary>
        /// Play this vibration with the configured settings
        /// </summary>
        public void Play()
        {
            if (!HapticsHelper.s_IsVibratorOn)
                return;
                
            if (usePreset)
            {
                HapticsHelper.Vibrate(preset);
            }
            else
            {
                #if UNITY_ANDROID
                float adjustedAmplitude = Mathf.Clamp(amplitude - 0.1f, 0.1f, 1f);
                HapticPatterns.PlayConstant(adjustedAmplitude, frequency, duration);
                #else
                HapticPatterns.PlayEmphasis(amplitude, frequency);
                #endif
            }
        }
    }
}