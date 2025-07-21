using MadApper.Singleton;
using NotificationSamples;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace MadApper.Notifications
{

    public class NotificationSystem : PersistentSingleton<NotificationSystem>, IActiveableSystem
    {
        const string k_PrefabName = "NotificationSystem";
        public string i_PrefabName => k_PrefabName;

        [SerializeField][AutoGetOrAdd][ReadOnly] GameNotificationsManager m_Manager;

        public bool IsFullyInitialized { get; private set; }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            if (!IActiveableSystem.IsActive(k_PrefabName))
                return;

            MADUtility.TryLoadAndInstantiate<NotificationSystem>(k_PrefabName);
        }

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            m_Manager.LocalNotificationDelivered += OnDelivered;
            m_Manager.LocalNotificationExpired += OnExpired;
        }

        private void OnDisable()
        {
            m_Manager.LocalNotificationDelivered -= OnDelivered;
            m_Manager.LocalNotificationExpired -= OnExpired;
        }


        public async UniTask WaitForSystemToFullyInitialize()
        {
            await RequestAuthorization();
            await WaitToInitialize();
        }

        /// <summary>
        /// Queue a notification with the given parameters.
        /// </summary>
        /// <param name="title">The title for the notification.</param>
        /// <param name="body">The body text for the notification.</param>
        /// <param name="deliveryTime">The time to deliver the notification.</param>        
        /// <param name="badgeNumber">The optional badge number to display on the application icon.</param>
        /// <param name="reschedule">
        /// Whether to reschedule the notification if foregrounding and the notification hasn't yet been shown.
        /// </param>
        /// the channel must be registered in <see cref="GameNotificationsManager.Initialize"/>.</param>      

        public async void SendNotification(string title, string body, DateTime deliveryTime, int? badgeNumber = null, bool reschedule = false)
        {
            SendNotificationInternal(title, body, deliveryTime, badgeNumber, reschedule);
        }

    

        #region Internal

        private void OnDelivered(PendingNotification obj) { }
        private void OnExpired(PendingNotification obj) { }

        void SendNotificationInternal(string title, string body, DateTime deliveryTime, int? badgeNumber = null, bool reschedule = true)
        {
            var notification = m_Manager.CreateNotification();

            if (notification == null)
            {
                return;
            }

            notification.Title = title;
            notification.Body = body;

            if (badgeNumber != null)
            {
                notification.BadgeNumber = badgeNumber.Value;
            }

            PendingNotification notificationToDisplay = m_Manager.ScheduleNotification(notification, deliveryTime);
            notificationToDisplay.Reschedule = reschedule;
        }

        async UniTask RequestAuthorization()
        {
#if UNITY_IOS
            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;

            using (var req = new AuthorizationRequest(authorizationOption, false))
            {
                await UniTask.WaitUntil(() => req.IsFinished == true);

                string res = "\n RequestAuthorization:";
                res += "\n finished: " + req.IsFinished;
                res += "\n granted :  " + req.Granted;
                res += "\n error:  " + req.Error;
                res += "\n deviceToken:  " + req.DeviceToken;

                this.Log(res);
            }
#endif
        }
        async UniTask WaitToInitialize()
        {           
            StartCoroutine(TryInitialize());

            await UniTask.WaitUntil(() => IsFullyInitialized == true);
        }
        IEnumerator TryInitialize()
        {
            if (m_Manager.Initialized)
                yield break;

            yield return m_Manager.Initialize();
                      
            IsFullyInitialized = true;

            TryCancelAllNotications();

            this.Log("Fully Initialized");
        }

        public void TryCancelAllNotications()
        {
            if (!IsFullyInitialized)
                return;

            m_Manager.CancelAllNotifications();

        }

        #endregion


    }
}
