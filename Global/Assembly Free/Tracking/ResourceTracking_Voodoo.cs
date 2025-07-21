
using MadApper;
using System;
using System.Collections.Generic;
using UnityEngine;

#if VOODOOSAUCE_ENABLED
using Voodoo.Sauce.Internal.Analytics;
#endif

namespace BEH.Voodoo
{
    public class ResourceTracking_Voodoo : MonoBehaviour, IResourceTracking
    {

        public enum CurrencyUsed { iap, game_currency, rv }


        [Space(10)] public ResourcePlacementSO m_Placement;

        [Space(10)] public CurrencyUsed m_CurrencyUsed = CurrencyUsed.game_currency;

#if VOODOOSAUCE_ENABLED
        [Space(10)] public ItemType m_ItemType;
#endif


        public static Action<int> onSoftCurrencyUsed;

        public virtual string GetLevel() => "Level 1";


        #region IResourceTracking

        Dictionary<string, string> options;
        public virtual bool IsDestroyable => false;

        public void SetOptions(Dictionary<string, string> options = null) => this.options = options;

        public void Sink(ResourceItemSO resource, int amount, ResourceType resourceType = ResourceType.Simple)
        {
#if VOODOOSAUCE_ENABLED
            if (amount == 0)
                return;

            var parameters = GetParams(resource: resource, amount: amount);

            if (parameters == null)
                return;

            parameters.transactionType = TransactionType.Out;

            var injectedLevel = options.TryGetInjectedLevel();

            if (!string.IsNullOrEmpty(injectedLevel))
                parameters.level = injectedLevel;

            VoodooSauce.OnItemTransaction(parameters);

            if (parameters.item.itemType == ItemType.soft_currency)
                onSoftCurrencyUsed?.Invoke(amount);

            this.LogYellow($"VOODOO - {ParamsToString(parameters)}");

            SetOptions(null);
#endif
        }

        public void Source(ResourceItemSO resource, int amount, ResourceType resourceType = ResourceType.Simple)
        {

#if VOODOOSAUCE_ENABLED
            if (amount == 0)
                return;

            var parameters = GetParams(resource: resource, amount: amount);

            if (parameters == null)
                return;

            parameters.transactionType = TransactionType.In;

            var injectedLevel = options.TryGetInjectedLevel();

            if (!string.IsNullOrEmpty(injectedLevel))
                parameters.level = injectedLevel;

            VoodooSauce.OnItemTransaction(parameters);

            this.LogYellow($"VOODOO - {ParamsToString(parameters)}");


            SetOptions(null);
#endif
        }

        #endregion





#if VOODOOSAUCE_ENABLED

        ItemTransactionParameters GetParams(ResourceItemSO resource, int amount)
        {
            var itemInfo = GetItemTransactionInfo(resource);

            if (!itemInfo.HasValue)
                return null;

            var parameters = new ItemTransactionParameters(item: itemInfo.Value,
                nbUnits: amount, transactionType: TransactionType.In);

            parameters.level = GetLevel();
            parameters.placement = GetPlacementEnum();
            parameters.currencyUsed = m_CurrencyUsed;
            parameters.balance = resource.GetAmount();

            return parameters;
        }


        ItemTransactionInfo? GetItemTransactionInfo(ResourceItemSO resource)
        {
            ItemTransactionInfo? item = null;

#if RESOURCESEXTRA_ENABLED

            if (Enum.TryParse(resource.m_ID, out GeneratedResourcesExtra.ResourcesEnum pResource))
                item = new ItemTransactionInfo(name: pResource, type: m_ItemType);
#endif
            return item;
        }


        public string ParamsToString(ItemTransactionParameters obj)
        {
            return $"Transaction: {obj.transactionType} - ItemName: {obj.item.itemName} -Amount: {obj.nbUnits} " +
                $"-Level: {obj.level} -Placement: {obj.placement} -CurrencyUsed: {obj.currencyUsed} -Balance: {obj.balance} " +
                $"-ItemType: {obj.item.itemType}";
        }

#endif

        Enum GetPlacementEnum()
        {
            Enum res = null;

#if RESOURCESEXTRA_ENABLED

            if (Enum.TryParse(m_Placement.m_ID, out GeneratedResourcesExtra.PlacementsEnum pPlacement))
                res = pPlacement;
#endif
            return res;
        }

    }
}
