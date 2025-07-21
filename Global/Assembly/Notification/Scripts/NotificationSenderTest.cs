using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Notifications
{
    public class NotificationSenderTest : NotificationSenderBase
    {
        public int Level;
        public override int GetLevel() => Level;

        private void Start()
        {
            TrySendNotifications();
        }

        public void z_TrySetLevelAndSendNotifs(string str)
        {
            if (!int.TryParse(str, out int value))
                return;

            Level = value;

            TrySendNotifications();
        }
        public void z_TrySendNotificationInSeconds(string str)
        {
            if (!float.TryParse(str, out float value))
                return;

            z_SendNotificationInSeconds(value);
        }

        public async void z_SendNotificationInSeconds(float seconds)
        {
            var delivery = DateTime.Now.AddSeconds(seconds);

            var message = m_Settings.RunTimeValue.GetMessage();

            var system = NotificationSystem.Instance;

            await system.WaitForSystemToFullyInitialize();

            this.Log($"{delivery}-{message.Title}-{message.Body}");

            system.SendNotification(title: message.Title, body: message.Body, deliveryTime: delivery);
        }

    }
}
