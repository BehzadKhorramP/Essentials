
using System;
using System.Collections.Generic;

namespace MadApper
{

    public interface IResourceTracking
    {
        public const string s_LevelKey = "Level";
        public const string s_ItemKey = "Item";

        public bool IsDestroyable { get; }
        public void SetOptions(Dictionary<string, string> options = null);
        public void Source(ResourceItemSO resource, int amount, ResourceType resourceType = ResourceType.Simple);
        public void Sink(ResourceItemSO resource, int amount, ResourceType resourceType = ResourceType.Simple);
    }


    public static class ResourceTrackingExtention
    {
        public static string TryGetInjectedLevel(this Dictionary<string, string> options)
        {
            if (options == null)
                return null;

            if (!options.ContainsKey(IResourceTracking.s_LevelKey))
                return null;

            return options[IResourceTracking.s_LevelKey];
        }

        public static string GetPayout(string inspectorPayout, string virtualPayout, Dictionary<string, string> options)
        {
            var payout = "";

            var injectedLevel = options.TryGetInjectedLevel();

            if (!string.IsNullOrEmpty(injectedLevel))
            {
                payout = injectedLevel;
            }

            if (!string.IsNullOrEmpty(virtualPayout))
            {
                if (!string.IsNullOrEmpty(payout))
                    payout += "-";

                payout += $"{virtualPayout}";
            }

            if (!string.IsNullOrEmpty(inspectorPayout))
            {
                if (!string.IsNullOrEmpty(payout))
                    payout += "-";

                payout += $"{inspectorPayout}";
            }

            return payout;
        }
    }


}
