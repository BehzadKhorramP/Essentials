using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MadApper.Essentials
{
    using static MadApper.Essentials.UIUnlockableBar;

    public class UIUnlockablePopup : MonoBehaviour
    {
        [SerializeField] List<Image> m_Icons;
        [SerializeField] List<TextMeshProUGUI> m_Titles;
        [SerializeField] List<TextMeshProUGUI> m_Descriptions;

        [Title("Activables")]
        [SerializeField] List<Transform> m_DescriptionsT;

        Action onContinuePressed;

        public void Initialize(UnlockableData unlockableData, Action onContinuePressed)
        {
            var popupData = unlockableData.PopupData;

            this.onContinuePressed = onContinuePressed;

            if (m_Icons != null)
                m_Icons.ForEach(x => x.sprite = unlockableData.IconSprite);

            var titleValid = !string.IsNullOrEmpty(popupData.Title) && m_Titles != null;
            if (titleValid)
                m_Titles.ForEach(x => x.SetText(popupData.Title));

            var descsValid = !string.IsNullOrEmpty(popupData.Description) && m_Descriptions != null;
            if (descsValid)
                m_Descriptions.ForEach(x => x.SetText(popupData.Description));

            m_DescriptionsT.ForEach(x => x.gameObject.SetActive(descsValid));
        }

        public void z_OnContinuePressed()
        {
            onContinuePressed?.Invoke();
        }


        [Serializable]
        public class PopupData
        {
            public UIUnlockablePopup Prefab;

            [TextArea(1, 2)] public string Title;
            [TextArea(2, 3)] public string Description;
        }

    }
}
