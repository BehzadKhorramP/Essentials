using UnityEngine;
using UnityEngine.UI;

namespace MadApper
{
    public class ImageUtils : MonoBehaviour
    {
        [SerializeField][AutoGetOrAdd] Image m_Image;
        [SerializeField] Color m_Color;
        public void z_SetColor() => m_Image.color = m_Color;

    }

}