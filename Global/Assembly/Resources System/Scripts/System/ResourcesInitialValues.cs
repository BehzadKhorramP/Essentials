using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MadApper
{
    public class ResourcesInitialValues : SingletonScriptable<ResourcesInitialValues>
    {

        [Space(10)][SerializeField] List<InitialValue> m_Values;
        [Space(20)][SerializeField] List<GrantFreePerInterval> m_GrantFreePerIntervals;
        [Space(10)][SerializeField] ResourceItemSO[] m_AllSOs;

#if UNITY_EDITOR
        [MenuItem("MAD/Resources/Initial Values", false, 100)]
        static void Edit()
        {
            Selection.activeObject = GetSO();
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            m_AllSOs = m_AllSOs.GetAllInstances_Editor();
            this.TrySetDirty();
        }
#endif


        #region Getter Setter

        public List<InitialValue> GetInitialValues() => m_Values;
        public List<GrantFreePerInterval> GetGrantFreePerIntervals() => m_GrantFreePerIntervals;
        public ResourceItemSO[] GetAllSOs() => m_AllSOs;

        #endregion


        public void SetInitialValues(out bool shouldInitialEventRaised)
        {
            shouldInitialEventRaised = false;

            if (IsSetAlready())
                return;

            if (m_Values == null || m_Values.Count == 0)
                return;

            shouldInitialEventRaised = true;

            foreach (var item in m_Values)
            {
                item.Resource.Set(item.Amount, item.Type, raiseEvent: false);
            }

            Save();
        }


        public void TrackInitialValues(IResourceTracking[] trackings)
        {
            foreach (var item in m_Values)
            {
                if (item.Type != ResourceType.Simple)
                    continue;

                var resource = item.Resource;
                var amount = item.Amount;

                foreach (var track in trackings)
                    track?.Source(resource: resource, amount: amount);
            }
        }
        public static bool IsSetAlready()
        {
            return ResourcesDataManager.DataBase.Data.IsInitialValuesSet;
        }
        public static void Save()
        {
            ResourcesDataManager.DataBase.Data.IsInitialValuesSet = true;
            ResourcesDataManager.DataBase.Save();
        }

        [ContextMenu("DELETE")]
        public static void Delete()
        {
            ResourcesDataManager.DataBase.Data.IsInitialValuesSet = false;
            ResourcesDataManager.DataBase.Save();
        }



        [Serializable]
        public class InitialValue
        {
            public ResourceItemSO Resource;
            public ResourceType Type;
            public int Amount;
        }

        [Serializable]
        public class GrantFreePerInterval
        {
            public ResourceItemSO Resource;

            [Tooltip("In Seconds")]
            public int FreePerInterval;
            public int FreeAmount = 1;
        }
    }
}