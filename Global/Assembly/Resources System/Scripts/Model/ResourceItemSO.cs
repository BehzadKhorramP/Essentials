using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;

namespace MadApper
{

    [CreateAssetMenu(fileName = "ResourceItemSO", menuName = "Resources/ResourceItemSO")]
    public class ResourceItemSO : ScriptableObject
    {
        [Serializable]
        public class TextRule
        {
            public string Prefix;
            public string Const;
            public string Suffix;
        }

        [Space(10)] public string m_UID;
        [Space(10)] public string m_ID;

        [Space(10)] public TextRule m_TextRule = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            var setDirty = false;

            if (string.IsNullOrEmpty(m_ID))
            {
                m_ID = name;
                setDirty = true;
            }
            if (string.IsNullOrEmpty(m_UID))
            {
                m_UID = name;
                setDirty = true;
            }

            if (setDirty)
                EditorUtility.SetDirty(this);
        }
#endif




        public bool IsEquals(ResourceItemSO other) => other != null && IsEquals(other.m_ID);
        public bool IsEquals(string other) => m_ID.Equals(other);

        public ResourceData GetResourceSavedData() => ResourcesDataManager.GetData(m_ID);
        public int GetAmount() => GetResourceSavedData().Amount;
        public DateTime GetUnimitedTimeTill() => GetResourceSavedData().UnlimitedTill;
        public bool IsPremium() => GetResourceSavedData().IsPremium;
        public int GetCap() => GetResourceSavedData().Cap;
        public bool HasActiveUnlimitedTimer() => (GetUnimitedTimeTill() - DateTime.Now).TotalSeconds > 0;
        public bool HasReachedCap()
        {
            var data = GetResourceSavedData();
            return data.Amount >= data.Cap;
        }
        public bool IsPremiumOrUnlimited() => IsPremium() || HasActiveUnlimitedTimer();
        public bool HasReachedCapOrIsPermiumOrUnlimited() => HasReachedCap() || IsPremium() || HasActiveUnlimitedTimer();


        public void Modify(int amount, ResourceType type = ResourceType.Simple, bool save = true, bool raiseEvent = true)
        {
            ResourcesDataManager.ModifyResource(m_ID, amount: amount, type: type, save: save, raiseEvent: raiseEvent);
        }

        [Button]
        public void Set(int amount, ResourceType type = ResourceType.Simple, bool raiseEvent = true)
        {
            ResourcesDataManager.SetResource(m_ID, amount: amount, type: type, raiseEvent: raiseEvent);
        }
        public void RaiseEvent(ResourceType type = ResourceType.Simple)
        {
            ResourcesDataManager.RaiseEvent(m_ID, type: type);
        }
        public void RaiseForcedEvent(ResourceType type = ResourceType.Simple)
        {
            ResourcesDataManager.RaiseForcedEvent(m_ID, type: type);
        }


        public string GetAmountText(int amount, ResourceType type, TextRule textRule)
        {
            if (textRule == null)
                textRule = m_TextRule;

            switch (type)
            {
                case ResourceType.Cap:
                    return $"{textRule.Prefix}{amount}{textRule.Suffix}";
                case ResourceType.Simple:
                    return $"{textRule.Prefix}{amount}{textRule.Suffix}";
                case ResourceType.UnlimitedTimer:
                    var timespan = TimeSpan.FromSeconds(amount);
                    var timeText = $"{textRule.Prefix}{amount}{textRule.Suffix}s";
                    if (amount >= 3600)
                        timeText = $"{textRule.Prefix}{(int)timespan.TotalHours}{textRule.Suffix}h";
                    else if (amount >= 60)
                        timeText = $"{textRule.Prefix}{timespan.ToString("%m")}{textRule.Suffix}m";
                    return timeText;
                case ResourceType.Premium:
                    return textRule.Const;
            }
            return $"{amount}";
        }





    }


}