using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Essentials
{

    public abstract class NotificationBadge : MonoBehaviour
    {
        [SerializeField] string m_UID;
        [SerializeField] GameObject m_Show;

        // to prevent adding to sessionDB when the badge has not yet been shown
        bool isShown;

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }

        public void SetUID(string uid) => m_UID = uid;

        public void z_Refresh()
        {
            var show = WillBeShown();
            RefreshInternal(show);
        }

        public void z_TryClearPermanent()
        {
            if (string.IsNullOrEmpty(m_UID)) return;

            NotificationBadgeDataBase.ClearPermanent(m_UID);
            z_Refresh();
        }
        public void z_TryClearForSession()
        {
            if (!isShown) return;
            if (string.IsNullOrEmpty(m_UID)) return;

            NotificationBadgeDataBase.ClearSession(m_UID);
            z_Refresh();
        }

        public void z_RemovePermanent()
        {
            if (string.IsNullOrEmpty(m_UID)) return;

            NotificationBadgeDataBase.RemovePermanent(m_UID);
        }

        void RefreshInternal(bool show)
        {
            if (show) isShown = true;

            m_Show.gameObject.SetActive(show);
        }

        public bool WillBeShown()
        {
            if (string.IsNullOrEmpty(m_UID)) return false;
            return ShouldShow() && !NotificationBadgeDataBase.IsCleared(m_UID);
        }
        public abstract bool ShouldShow();
    }




    public class NotificationBadgeDataBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            RemoveSessions();
        }


        public static bool IsCleared(string uid) =>
            DataBase.Data.Permanents.Contains(uid) || DataBase.Data.Sessions.Contains(uid);
        public static void ClearPermanent(string uid)
        {
            if (DataBase.Data.Permanents.Contains(uid)) return;
            DataBase.Data.Permanents.Add(uid);
            DataBase.Save();
        }
        public static void ClearSession(string uid)
        {
            if (DataBase.Data.Sessions.Contains(uid)) return;
            DataBase.Data.Sessions.Add(uid);
            DataBase.Save();
        }
        public static void RemovePermanent(string uid)
        {
            if (!DataBase.Data.Permanents.Contains(uid)) return;
            DataBase.Data.Permanents.Remove(uid);
            DataBase.Save();
        }
        public static void RemoveSessions()
        {
            DataBase.Data.Sessions = new List<string>();
            DataBase.Save();
        }

        [Serializable]
        public class SaveData : ISaveable
        {
            const string k_SaveName = "NotificationBadgeData";
            public string SaveName => k_SaveName;
            public string Hash { get; set; }
            public bool IgnoreHash => false;

            public List<string> Permanents;
            public List<string> Sessions;

            public SaveData()
            {
                Permanents = new List<string>();
                Sessions = new List<string>();
            }
        }
        public class DataBase : DataBase<SaveData> { }
    }
}
