using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MadApper
{
    public class PurchaseByResourceObserver : MonoBehaviour
    {
        [Space(10)][SerializeField] PurchaseByResourceSO m_PurchaseByResourceSO;
        [Space(10)][SerializeField] PurchaseByResource m_PurchaseByResource;

        [SerializeField] int m_Count = 1;

        [Space(10)]
        [SerializeField] UnityEvent m_OnPurchaseSucceeded;
        [SerializeField] UnityEvent<string> m_OnPurchaseFailed;

        [Title("Visuals")]
        [SerializeField] TextMeshProUGUI[] m_PurchasePriceText;

        [Space(10)]
        [SerializeField] UnityEvent m_HasEnough;
        [SerializeField] UnityEvent m_NotEnough;


#if UNITY_EDITOR

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (TryRefreshTextPrice())
                foreach (var item in m_PurchasePriceText)
                    EditorUtility.SetDirty(item);
        }

#endif

        private void OnEnable()
        {
            RefreshState();
            ResourcesDataManager.onResourceChanged += OnResourceChanged_ChangeState;
        }
        private void OnDisable()
        {
            ResourcesDataManager.onResourceChanged -= OnResourceChanged_ChangeState;
        }

        public PurchaseByResource GetActivePurchaseByResource()
        {
            if (m_PurchaseByResourceSO != null)
            {
                if (m_PurchaseByResourceSO.GetPurchaseByResource().GetResourceToSpend() != null)
                    return m_PurchaseByResourceSO.GetPurchaseByResource();
            }
            else if (m_PurchaseByResource != null && m_PurchaseByResource.GetResourceToSpend() != null)
            {
                return m_PurchaseByResource;
            }

            return null;
        }
        public ResourceItemSO GetResourceToSpend()
        {
            var purchaser = GetActivePurchaseByResource();
            if (purchaser != null) return purchaser.GetResourceToSpend();
            return null;
        }

        public void Setup(PurchaseByResource purchase, int count)
        {
            m_PurchaseByResourceSO = null;
            m_PurchaseByResource = purchase;

            SetCount(count);
            Refresh();
        }
        public void Setup(PurchaseByResourceSO purchaseSO, int count, int extraPrice)
        {
            m_PurchaseByResourceSO = purchaseSO;
            m_PurchaseByResource = null;

            SetExtraPrice(extraPrice);
            SetCount(count);
            Refresh();
        }
        public void SetExtraPrice(int extraPrice)
        {
            var activePurchaser = GetActivePurchaseByResource();
            if (activePurchaser != null) activePurchaser.SetExtraPrice(extraPrice);            
        }
        public void SetCount(int count)
        {
            m_Count = count;
        }
        public void Refresh()
        {
            TryRefreshTextPrice();
            RefreshState();
        }

        void RefreshState()
        {
            var has = HasEnoughResourceToPurchase();

            if (has) m_HasEnough?.Invoke();
            else m_NotEnough?.Invoke();
        }
        private void OnResourceChanged_ChangeState(string id, ResourceData arg2, ResourceType arg3)
        {
            var resource = GetResourceToSpend();

            if (resource == null) return;
            if (!resource.IsEquals(id)) return;

            RefreshState();
        }

        bool TryRefreshTextPrice()
        {
            if (m_PurchasePriceText == null || m_PurchasePriceText.Length == 0)
                return false;

            var purchaser = GetActivePurchaseByResource();
            if (purchaser == null)
                return false;

            var price = purchaser.GetPrice(m_Count);
            string text = null;

            if (price == 0) text = "Free";
            else text = $"{price}";

            var dirty = false;

            foreach (var item in m_PurchasePriceText)
            {
                if (item.text != text)
                {
                    item.text = text;
                    dirty = true;
                }
            }

            return dirty;
        }

        public bool HasEnoughResourceToPurchase()
        {
            var purchaser = GetActivePurchaseByResource();
            if (purchaser == null)
            {
                this.LogWarning($"active purchaser is null in {name}");
                return false;
            }

            return purchaser.HasEnoughResourceToPurchase(m_Count);
        }


        public void OnPurchaseResult(bool succeed, string message)
        {
            if (succeed)
            {
                TrackDataSink();
                m_OnPurchaseSucceeded?.Invoke();
            }
            else
            {
                this.Log(message);
                var id = GetResourceID();
                m_OnPurchaseFailed?.Invoke(id);
            }
        }



        string GetResourceID()
        {
            var resource = GetResourceToSpend();
            if (resource == null) return "";
            return resource.m_ID;
        }

        public void TryPurchase(Action<bool, string> onPurchaseResult = null)
        {
            var purchaser = GetActivePurchaseByResource();
            if (purchaser == null)
            {
                this.LogWarning($"active purchaser is null in {name} - Trying to Purchase");
                return;
            }

            purchaser.TryPurchase(count: m_Count, onPurchaseResult: (succeed, message) =>
            {
                onPurchaseResult?.Invoke(succeed, message);
                OnPurchaseResult(succeed, message);
            });
        }


        public void z_TryPurchase()
        {
            var purchaser = GetActivePurchaseByResource();
            if (purchaser == null)
            {
                this.LogWarning($"active purchaser is null in {name} - z_Trying to Purchase");
                return;
            }

            purchaser.TryPurchase(count: m_Count, onPurchaseResult: OnPurchaseResult);
        }


        // hooked if PurchaseByResourceSO has been changed (e.g. AB tests)
        public void z_ToBeShown()
        {
            Refresh();
        }


        void TrackDataSink()
        {
            var resource = GetResourceToSpend();
            var pricePiad = GetActivePurchaseByResource().GetPrice(m_Count);
            var trackings = GetComponents<IResourceTracking>();

            if (trackings == null) return;

            foreach (var track in trackings)
                track?.Sink(resource: resource, amount: pricePiad);
        }




    }

}