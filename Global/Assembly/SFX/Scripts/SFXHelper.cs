

using DG.Tweening;
using System;
using UnityEngine;

namespace MadApper
{
    public class SFXHelper : MonoBehaviour
    {
        [Space(10)] public SFXData m_Data;
        [Space(10)] public bool pitchIncreaseWhenNotEnoughOrdered;

        AudioSource audioSource;

        public static Func<SFXData, AudioSource> onSFX;

        public void OnSFX()
        {
            if (m_Data == null)
                return;

            audioSource = onSFX?.Invoke(m_Data);
        }

        public void z_OnSFX()
        {
            OnSFX();
        }

        public void z_StopSFX()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource = null;
            }
        }

        public void z_FadeAndStopSFX()
        {
            if (audioSource != null)
            {
                var ac = audioSource;

                ac.DOKill();

#if DOTWEEN_ENABLED
                ac.DOFade(0, .2f).OnComplete(() =>
                      {
                          ac.Stop();
                      });
#else
                ac.Stop();
#endif

                audioSource = null;
            }
        }


        public void z_OrderedSFX(int index)
        {
            var clip = m_Data.SO.GetAudioClip(index);

            var pitch = 1;

            if (pitchIncreaseWhenNotEnoughOrdered)
            {

            }

            SoundEffectsManager.Instance?.OnSFXClip(m_Data.SO.m_ID, clip,
                vol: m_Data.Vol,
                pitch: pitch,
                randomVol: false,
                randomPitch: false);
        }

        public void z_IncreasedPitchSFX(int index)
        {
            var pitch = 1 + (index * SoundEffectsManager.k_Pitch);
            SoundEffectsManager.Instance.OnSFX(m_Data.SO, vol: m_Data.Vol, pitch: pitch, randomVol: false, randomPitch: false);
        }
    }
}
