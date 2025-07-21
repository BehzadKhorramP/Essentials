using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace BEH.IAP
{
    public class IAPInitializationSubscriber : MonoBehaviour
    {
        [Space(10)] public IAPProductSO m_ProductSO;
        [Space(10)] public TextMeshProUGUI m_PriceText;      
        [Space(10)] public UnityEvent onEnable;
        public UnityEvent onInitialized;

        public void OnEnable()
        {
            onEnable?.Invoke();

            IAPSystem.SubscribeToInitialized(this);
        }
        public void OnDisable()
        {
            IAPSystem.UnSubscribeToInitialized(this);
        }

        public void OnInitialized()
        {
            UpdatePrice();

            onInitialized?.Invoke();
        }

        private void UpdatePrice()
        {
            if (m_ProductSO.IsValid())
            {
                var product = IAPSystem.GetStoreProduct(m_ProductSO.m_ProductID);

                if (product != null)
                    m_PriceText?.SetText(product.metadata.localizedPriceString);
            }
        }
    }

}