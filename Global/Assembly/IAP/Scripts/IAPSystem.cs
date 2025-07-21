using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace BEH.IAP
{
    public class IAPSystem : PersistentSingleton<IAPSystem>, IDetailedStoreListener
    {
        static IStoreController s_StoreController;
        static IExtensionProvider s_ExtensionProvider;

        protected static ProductCatalog catalog;

        public static bool s_UnityPurchasingInitialized;
        public static List<IAPInitializationSubscriber> s_InitializationSubscribers;

        public static Action<string> onTryPurchase;
        public static Action onPurchaseFailed;
        public static Action<PurchaseEventArgs> onPurchasedSuccessfully;


        [RuntimeInitializeOnLoadMethod]
        static void InitializeIAPSystemOnLoad()
        {
#if !VOODOOSAUCE_ENABLED

            if (Instance != null)
                return;

            new GameObject("BEH-IAP System").AddComponent<IAPSystem>();
#endif
        }

        #region UnityCallbacks

        private async void Start()
        {
            await UnityServices.InitializeAsync();

            InitializePurchasing();
        }

        protected override void OnDestroy()
        {
            Nullify();

            base.OnDestroy();
        }

        void Nullify()
        {
            s_UnityPurchasingInitialized = false;
            s_InitializationSubscribers = null;
            s_StoreController = null;
            s_ExtensionProvider = null;
            catalog = null;
        }

        #endregion


        [ContextMenu("INITIALIZE")]
        private void InitializePurchasing()
        {
            var text = "Initializing Purchasing ...";

            this.LogBlue(text);

            var module = StandardPurchasingModule.Instance();

            module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;

            var builder = ConfigurationBuilder.Instance(module);

            catalog = ProductCatalog.LoadDefaultCatalog();

            if (!catalog.IsEmpty())
                IAPConfigurationHelper.PopulateConfigurationBuilder(ref builder, catalog);

            UnityPurchasing.Initialize(this, builder);
        }




        #region Initialization Subscribers
        public static void SubscribeToInitialized(IAPInitializationSubscriber subscriber)
        {
            if (s_UnityPurchasingInitialized)
            {
                subscriber.OnInitialized();
                return;
            }

            if (s_InitializationSubscribers == null)
                s_InitializationSubscribers = new List<IAPInitializationSubscriber>();

            if (!Contains(subscriber))
                s_InitializationSubscribers.Add(subscriber);

            bool Contains(IAPInitializationSubscriber s)
                => s_InitializationSubscribers.Find(x => x == s) != null;
        }
        public static void UnSubscribeToInitialized(IAPInitializationSubscriber subscriber)
        {
            if (s_InitializationSubscribers == null)
                return;

            if (subscriber == null)
                return;

            var item = s_InitializationSubscribers.Find(x => x == subscriber);

            if (item != null)
                s_InitializationSubscribers.Remove(item);
        }
        #endregion



        #region IDetailedStoreListener

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            var text = "In-App Purchasing successfully initialized";

            this.LogBlue(text);

            s_StoreController = controller;
            s_ExtensionProvider = extensions;

            s_UnityPurchasingInitialized = true;

            if (s_InitializationSubscribers != null)
                foreach (var item in s_InitializationSubscribers)
                    item.OnInitialized();

            s_InitializationSubscribers = null;
        }

        /// <summary>
        /// For advanced scripted store-specific IAP actions, use this session's <typeparamref name="IStoreExtension"/>s after initialization.
        /// </summary>
        /// <typeparam name="T">A subclass of <typeparamref name="IStoreExtension"/> such as <typeparamref name="IAppleExtensions"/></typeparam>
        /// <returns></returns>
        public T GetStoreExtensions<T>() where T : IStoreExtension
        {
            return s_ExtensionProvider.GetExtension<T>();
        }


        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            var errorMessage = $"Purchasing failed to initialize. Reason: {error}.";

            if (message != null)
                errorMessage += $" More details: {message}";

            this.Log(errorMessage);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var text = $"Purchase succeeded - Product: '{purchaseEvent.purchasedProduct.definition.id}";

            this.LogBlue(text);

            IAPInProgress.Disappear();

            onPurchasedSuccessfully?.Invoke(purchaseEvent);

            var product = purchaseEvent.purchasedProduct;

            IAPTracking.TrackIAP(new IAPTracking.Data()
            {
                ItemID = product.definition.id,
                ItemType = product.definition.type.ToString(),
                Amount = (int)(product.metadata.localizedPrice * 100),
                Currency = product.metadata.isoCurrencyCode,
                CartType = "Shop"
            });

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {         
            var text = "Purchase failed - Product: ";

            if (product != null)
                text += $"'{product.definition.id}'";

            text += $",Purchase failure reason: {failureDescription.reason}" +
                    $",Purchase failure details: {failureDescription.message}";


            this.LogWarning(text);

            onPurchaseFailed?.Invoke();

            IAPInProgress.Failed();
        }


        #region Obsolete

        //Obsolete
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) { }
        public void OnInitializeFailed(InitializationFailureReason error) { }

        #endregion


        #endregion



        public static void TryPurchase(string productId)
        {
            if (s_UnityPurchasingInitialized == false || s_StoreController == null)
                return;

            if (!HasProductInCatalog(productId))
                return;

            IAPInProgress.Appear();

            onTryPurchase?.Invoke(productId);

            s_StoreController.InitiatePurchase(productId);
        }



        public static Product GetStoreProduct(string productId)
        {
            if (s_UnityPurchasingInitialized == false || s_StoreController == null)
                return null;

            return s_StoreController.products.WithStoreSpecificID(productId);
        }

        public static bool HasProductInCatalog(string productId)
        {
            var hasProduct = catalog.allProducts.FirstOrDefault(x => x.id.Equals(productId)) != null;

            if (!hasProduct)
                Debug.LogWarning("The product catalog has no product with the ID \"" + productId + "\"");

            return hasProduct;
        }




    }
}
