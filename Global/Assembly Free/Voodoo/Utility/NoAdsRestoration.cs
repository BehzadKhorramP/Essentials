#if IAP_ENABLED
using BEH.IAP; 
#endif

using System.Collections.Generic;
using UnityEngine;

#if VOODOOSAUCE_ENABLED
using Voodoo.Sauce.IAP;
#endif

namespace BEH.Voodoo
{

    public class NoAdsRestoration : MonoBehaviour
    {

#if VOODOOSAUCE_ENABLED

        public List<IAPProductSO> m_Products;

        private void OnEnable()
        {
            IAPSystem_Voodoo.onPurchasedSuccessfully += OnPurchasedSuccessfully_RestoreNoAds;

        }
        private void OnDisable()
        {
            IAPSystem_Voodoo.onPurchasedSuccessfully -= OnPurchasedSuccessfully_RestoreNoAds;
        }

        private void OnPurchasedSuccessfully_RestoreNoAds(ProductReceivedInfo info, PurchaseValidation validation)
        {
            var productId = info.ProductId;

            this.Log(productId);

            if (!IsNoAds(productId))
                return;

            var isRestored = validation.isRestorationPurchase.HasValue
               && validation.isRestorationPurchase.Value == true;

            PremiumHelper.EnablePremium();

            foreach (var item in m_Products)
            {
                if (ShopSystem.DataBase.IsPurchased(item.m_ProductID))
                    continue;                            

                ShopSystem.DataBase.OnPurchased(item.m_ProductID);
                ShopSystem.DataBase.OnClaimed(item.m_ProductID);
            }
        }


        bool IsNoAds(string productID)
        {
            foreach (var item in m_Products)
            {
                if (item.IsValidAndEquals(productID))
                    return true;
            }

            return false;
        } 
#endif
    }

}