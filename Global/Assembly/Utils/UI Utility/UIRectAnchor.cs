using UnityEngine;

namespace MadApper
{
    public class UIRectAnchor : MonoBehaviour
    {
        [SerializeField][AutoGetOrAdd] RectTransform m_Rect;
        [SerializeField] Vector2 m_AnchorMin, m_AnchorMax;
        [SerializeField] Vector2 m_AnchorPosition;
        public void z_RefreshAnchorPosition()
        {
            m_Rect.anchoredPosition = m_AnchorPosition;
        }
        public void z_RefreshAnchors()
        {
            m_Rect.anchorMin = m_AnchorMin;
            m_Rect.anchorMax = m_AnchorMax;
        }
        public void z_SetAnchorPositionToBottomBySize()
        {
            m_Rect.anchoredPosition = new Vector2(-m_Rect.sizeDelta.x / 2f, m_Rect.sizeDelta.y);
        }


    }
}
