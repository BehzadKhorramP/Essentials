using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MadApper
{
    public class ResourceViewSlider : ResourceViewCap
    {
        [Space(10)] public Image m_Slider;

        public override void OnResourceChagend(ResourceData data)
        {
            var cap = data.Cap;
            var amount = data.Amount;
            var amountNormalized = amount * 1f / cap;

            m_Slider.DOKill();
#if DOTWEEN_ENABLED
            m_Slider.DOFillAmount(amountNormalized, .3f).SetEase(Ease.Linear);

#endif
            base.OnResourceChagend(data);
        }

        public void OnDestroy()
        {
            m_Slider.DOKill();
        }
    }
}
