using UnityEngine;
using UnityEngine.UI;

namespace MadApper
{
    public class UIRebuildLayout : MonoBehaviour
    {
        [SerializeField][AutoGetInChildren] RectTransform m_Layout;

        public void z_RebuildLayout()
        {
            z_RebuildLayout(m_Layout);
        }
        public void z_RebuildLayout(RectTransform rect)
        {
            if (rect == null)
                return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }
}
