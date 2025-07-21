using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using static MadApper.Essentials.UIUnlockableBar;

namespace MadApper.Essentials
{
    public class UIUnlockableBarExample : MonoBehaviour
    {
        [SerializeField][AutoGetInChildren] UIUnlockableBar m_UnlockableBar;
        [SerializeField] List<UnlockableData> m_TestUnlockableData;

        [TitleGroup("Test Values")]
        [SerializeField] float m_AnimatorDuration = 1;
        [SerializeField] int m_FinishedLevel = 1;


        [Button(ButtonSizes.Large)]
        public void Setup()
        {
            m_UnlockableBar.gameObject.SetActive(true);

            SystemArgs sysArgs = new SystemArgs()
            {
                FinishedLevel = m_FinishedLevel,
                Unlockables = m_TestUnlockableData,
                OnNoMoreUnlockables = OnNoMoreUnlockable               
            };

            m_UnlockableBar.Setup(sysArgs);
        }

        [Button(ButtonSizes.Large)]
        private void AnimateBar()
        {
            AnimatorArgs animArgs = new AnimatorArgs()
            {
                Duration = m_AnimatorDuration,
                OnCompletedNotUnlocked = OnCompletedNotUnlocked,
                OnCompletedUnlocked = OnCompletedUnlocked,
                OnContinuePressedFromPopup = OnContinuePressedFromPopup
            };

            m_UnlockableBar.AnimateBar(animArgs);
        }

      
        private void OnNoMoreUnlockable()
        {
            m_UnlockableBar.gameObject.SetActive(false);
        }

        private void OnCompletedNotUnlocked()
        {
            $"UIUnlockableBar completed - item not unlocked!".LogBlue();
        }
        private void OnCompletedUnlocked()
        {
            $"UIUnlockableBar completed - item unlocked!".LogBlue();
        }
        private void OnContinuePressedFromPopup()
        {
            $"UIUnlockablePopup continue pressed!".LogBlue();
        }



        public void z_TryWithInput(string @input)
        {
            if (!int.TryParse(input, out int result))
                return;

            if (result <= 0) return;

            m_FinishedLevel = result;

            Setup();
            AnimateBar();
        }
    }
}
