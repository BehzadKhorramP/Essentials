using BEH;
using System;
using UnityEngine;

namespace MadApper
{
    [Serializable]
    public class PurchaseByResource
    {
        [SerializeField] ResourceItemSO m_ResourceToSpend;
        [SerializeField] int m_Price;
        [SerializeField] int m_ExtraPrice;
        public PurchaseByResource(ResourceItemSO resourceToSpend, int price, int extraPrice = 0)
        {
            m_ResourceToSpend = resourceToSpend;
            m_Price = price;
            m_ExtraPrice = extraPrice;
        }

        public ResourceItemSO GetResourceToSpend() => m_ResourceToSpend;

        public void SetPrice(int price) => m_Price = price;
        public void SetExtraPrice(int extraPrice) => m_ExtraPrice = extraPrice;
        public void SetPrices(int price, int extraPrice)
        {
            m_Price = price;
            m_ExtraPrice = extraPrice;
        }
        public int GetBasePrice()
        {
            return m_Price;
        }

        public int GetPrice(int count = 1)
        {
            return (m_Price * count) + m_ExtraPrice;
        }

        public bool HasEnoughResourceToPurchase(int count = 1)
        {
            if (m_ResourceToSpend == null)
            {
                $"{nameof(m_ResourceToSpend)} is null".LogWarning();
                return false;
            }

            var currAmount = m_ResourceToSpend.GetAmount();
            return currAmount >= GetPrice(count);
        }
        public void TryPurchase(int count = 1, Action<bool, string> onPurchaseResult = null)
        {
            var message = "";

            if (m_ResourceToSpend == null)
            {
                message = $"{nameof(m_ResourceToSpend)} is null";
                message.LogWarning();
                onPurchaseResult?.Invoke(false, message);
                return;
            }

            var isPossible = HasEnoughResourceToPurchase(count);

            if (isPossible)
            {
                var sinkAmount = GetPrice(count);
                m_ResourceToSpend.Modify(-sinkAmount);
                onPurchaseResult?.Invoke(true, message);
            }
            else
            {
                message = $"Not enough [{m_ResourceToSpend.m_ID}]!";
                onPurchaseResult?.Invoke(false, message);
            }
        }

    }


    [CreateAssetMenu(fileName = "PurchaseByResourceSO", menuName = "Resources/PurchaseByResourceSO")]
    public class PurchaseByResourceSO : ScriptableObject
    {
        [SerializeField] PurchaseByResource m_PurchaseByResource;
        public PurchaseByResource GetPurchaseByResource() => m_PurchaseByResource;

    }

}