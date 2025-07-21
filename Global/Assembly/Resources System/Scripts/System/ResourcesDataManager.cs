using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper
{
    public enum ResourceType
    {
        Cap, Simple, UnlimitedTimer, Premium
    }

    [Serializable]
    public class ResourceData
    {
        public int Amount;
        public int Cap;
        public bool IsPremium;
        public DateTime UnlimitedTill;
        public DateTime LastTimeFreeGranted;
        public ResourceData()
        {
            Cap = -1;

            UnlimitedTill = DateTime.Now;
            LastTimeFreeGranted = DateTime.Now;
        }
    }

    public static class ResourcesDataManager
    {
        public const string CurrencyKey = "Coin";

        public static Action<string, int> onResourceChangedAmount;

        public static Action<string, ResourceData, ResourceType> onResourceChanged;
        public static Action<string, ResourceData, ResourceType> onResourceChangedForced;

        public static void ModifyResource(string key, int amount, ResourceType type = ResourceType.Simple, bool save = true, bool raiseEvent = true)
        {
            var data = GetData(key);

            switch (type)
            {
                case ResourceType.Simple:
                    data.Amount += amount;
                    if (data.Amount <= 0)
                        data.Amount = 0;

                    if (data.Cap > 0)
                    {
                        if (amount < 0)
                        {
                            var hadReachedCap = data.Amount + Mathf.Abs(amount) >= data.Cap;
                            var nowBelowCap = data.Amount < data.Cap;

                            if (hadReachedCap && nowBelowCap)
                                data.LastTimeFreeGranted = DateTime.Now;
                        }
                    }

                    break;
                case ResourceType.UnlimitedTimer:
                    var tillDate = DataBase.GetTimeDataClamped(data);
                    data.UnlimitedTill = tillDate.AddSeconds(amount);
                    break;
                case ResourceType.Premium:
                    data.IsPremium = amount > 0 ? true : false;
                    break;
                case ResourceType.Cap:
                    data.Cap += amount;
                    if (data.Cap <= 0)
                        data.Cap = 0;
                    break;
            }

            if (amount != 0)
                onResourceChangedAmount?.Invoke(key, amount);

            if (amount != 0 && raiseEvent)
                onResourceChanged?.Invoke(key, data, type);

            if (save)
                DataBase.Save();
        }

        public static void SetResource(string key, int amount, ResourceType type, bool raiseEvent = true)
        {
            var res = GetData(key);

            switch (type)
            {
                case ResourceType.Simple:
                    res.Amount = amount;
                    if (res.Amount <= 0)
                        res.Amount = 0;
                    break;
                case ResourceType.UnlimitedTimer:
                    var tillDate = DateTime.Now;
                    res.UnlimitedTill = tillDate.AddSeconds(amount);
                    break;
                case ResourceType.Premium:
                    res.IsPremium = amount > 0 ? true : false;
                    break;
                case ResourceType.Cap:
                    res.Cap = amount;
                    if (res.Cap <= 0)
                        res.Cap = 0;
                    break;
            }

            if (raiseEvent)
                onResourceChanged?.Invoke(key, res, type);

            DataBase.Save();
        }

        public static void RaiseEvent(string key, ResourceType type)
        {
            var res = GetData(key);

            onResourceChanged?.Invoke(key, res, type);
        }
        public static void RaiseForcedEvent(string key, ResourceType type)
        {
            var res = GetData(key);

            onResourceChangedForced?.Invoke(key, res, type);
        }





        public static ResourceData GetData(string key)
        {
            return DataBase.GetData(key);
        }



        [Serializable]
        public class SaveData : ISaveable
        {
            const string name = "ResourcesData";
            public string SaveName => name;

            public bool IsInitialValuesSet;

            public Dictionary<string, ResourceData> Resourcess;

            public SaveData()
            {
                Resourcess = new Dictionary<string, ResourceData>();
            }
            public string Hash { get; set; }

            public bool IgnoreHash => false;
        }


        public class DataBase : DataBase<SaveData>
        {


            public static ResourceData GetData(string key)
            {
                ResourceData res = null;

                if (!Data.Resourcess.ContainsKey(key))
                {
                    res = new ResourceData();
                    Data.Resourcess.Add(key, res);
                    Save();
                }
                else
                {
                    res = Data.Resourcess[key];
                }

                return res;
            }

            public static void EmptyResource(string id)
            {
                Data.Resourcess[id] = new ResourceData();

                Save();
            }

            public static DateTime GetTimeDataClamped(ResourceData data)
            {
                var unlimiteddiff = (data.UnlimitedTill - DateTime.Now).TotalSeconds;

                if (unlimiteddiff <= 0)
                    data.UnlimitedTill = DateTime.Now;

                return data.UnlimitedTill;
            }
        }
    }

}