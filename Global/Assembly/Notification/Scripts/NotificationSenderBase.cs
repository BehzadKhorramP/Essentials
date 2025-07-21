using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MadApper.Notifications
{
    public abstract class NotificationSenderBase : MonoBehaviour
    {
        public const string k_DayInstalled = "DayInstalled";

        [SerializeField][ReadOnly] protected SingletonScriptableHelper<NotificationSettingsSO> m_Settings;

        List<QueuedNotification> queuedNotifications = new List<QueuedNotification>();

        protected bool wasSuccesful;

        public abstract int GetLevel();


        /// <summary>
        /// usually call this once at the app startup,
        /// but you can call it after level finishes if you want to
        /// </summary>
        public void TrySendNotifications()
        {
            if (m_Settings.RunTimeValue == null)
                return;

            queuedNotifications = new List<QueuedNotification>();

            var dateInstalled = GetDateInstalled();
#if UNITY_EDITOR
            this.Log("Date Installed : " + dateInstalled);
#endif
            if (GetLevel() < m_Settings.RunTimeValue.LevelToSend)
            {
#if UNITY_EDITOR
                this.Log("not yet reached To a level to send notifs");
#endif
                return;
            }

            TryQueueD7Notif(dateInstalled);
            TryQueueReqularNotifs();
            SendNotifs();
        }


        void TryQueueD7Notif(DateTime dateInstalled)
        {
            var diff = (DateTime.Now - new DateTime(dateInstalled.Year, dateInstalled.Month, dateInstalled.Day)).Days;

            if (diff > 0)
                QueuNotification(diff);
        }



        void TryQueueReqularNotifs()
        {
            foreach (var day in m_Settings.RunTimeValue.DaysSinceGameLastOpenedToSend)
            {
                QueuNotification(day);
            }
        }


        public void QueuNotification(int daysFromToday)
        {
            if (queuedNotifications.Any(x => x.DaysFromToday == daysFromToday))
                return;

            var message = m_Settings.RunTimeValue.GetMessage();

            queuedNotifications.Add(new QueuedNotification()
            {
                Message = message,
                DaysFromToday = daysFromToday,
            });
        }

        private async void SendNotifs()
        {
            var system = NotificationSystem.Instance;

            if (system == null)
                return;

            await system.WaitForSystemToFullyInitialize();

            foreach (var item in queuedNotifications)
            {
                var date = DateTime.Now.AddDays(item.DaysFromToday);
                var hour = m_Settings.RunTimeValue.HourToSend;
                var delivery = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0, DateTimeKind.Local);
                var message = item.Message;

#if UNITY_EDITOR
                this.Log($"{delivery}-{message.Title}-{message.Body}");
#endif

                system.SendNotification(title: message.Title, body: message.Body, deliveryTime: delivery);
            }

            wasSuccesful = true;
        }




        DateTime GetDateInstalled()
        {
            if (PlayerPrefs.HasKey(k_DayInstalled))
            {
                var str = PlayerPrefs.GetString(k_DayInstalled);
                if (DateTime.TryParse(str, out DateTime dateSaved))
                    return dateSaved;
            }

            var date = DateTime.Now;
            var value = date.ToShortDateString();

            PlayerPrefs.SetString(k_DayInstalled, value);
            PlayerPrefs.Save();

            return date;
        }


        [Button]
        public void z_DeleteDayInstalled()
        {
            if (!PlayerPrefs.HasKey(k_DayInstalled))
                return;

            PlayerPrefs.DeleteKey(k_DayInstalled);
            PlayerPrefs.Save();
        }


        public void z_TryDeleteNotificationsQueue()
        {
            queuedNotifications.Clear();

            var system = NotificationSystem.Instance;

            if (system == null)
                return;

            system.TryCancelAllNotications();
        }



        [Serializable]
        class QueuedNotification
        {
            public NotificationMessage Message;
            public int DaysFromToday;
        }

    }
}
