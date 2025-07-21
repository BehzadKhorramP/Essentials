using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

namespace BEH.IAP
{

    public class IAPProductObserver : MonoBehaviour
    {
        [Space(10)] public IAPProductSO m_ProductSO;

        [Space(10)] public UnityEvent<string> onSetIDonValidate;

        [Space(10)] public UnityEvent onPurchasedSuccessfully;


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (!m_ProductSO.IsValid())
                return;

            onSetIDonValidate?.Invoke(m_ProductSO.m_ProductID);

            var initializer = GetComponentInChildren<IAPInitializationSubscriber>();

            if (initializer != null)
            {
                if (initializer.m_ProductSO == null || (initializer.m_ProductSO != null && !initializer.m_ProductSO.m_ProductID.Equals(m_ProductSO)))
                {
                    initializer.m_ProductSO = m_ProductSO;
                    EditorUtility.SetDirty(initializer);
                }
            }
        }
#endif
        private void OnEnable()
        {
            IAPSystem.onPurchasedSuccessfully += OnPurchasedSuccessfully;
        }
        private void OnDisable()
        {
            IAPSystem.onPurchasedSuccessfully -= OnPurchasedSuccessfully;
        }
              

        private void OnPurchasedSuccessfully(PurchaseEventArgs args)
        {
            var product = args.purchasedProduct;
            var productId = product.definition.id;

            if (!m_ProductSO.IsValidAndEquals(productId))
                return;

            onPurchasedSuccessfully?.Invoke();
        }




        public void z_OnTryPurchase()
        {
            if (!m_ProductSO.IsValid())
                return;

            IAPSystem.TryPurchase(m_ProductSO.m_ProductID);
        }

    }

}