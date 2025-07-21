using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MadApper.Notifications
{
    public class NotificationSettingsSO : SingletonScriptable<NotificationSettingsSO>
    {
        const string fireEmoji = "\U0001F525"; // 🔥
        const string rocketEmoji = "\U0001F680"; // 🚀
        const string trophyEmoji = "\U0001F3C6"; // 🏆
        const string coffeeEmoji = "\u2615"; // ☕
        const string jigsawEmoji = "\U0001F9E9"; //🧩
        const string manDanceEmoji = "\U0001F57A"; //🕺
        const string womanDanceEmoji = "\U0001F483"; //💃
        const string discoBallEmoji = "\U0001FAA9"; //🪩
        const string clockEmoji = "\u23F0"; //⏰
        const string rescueHatEmoji = "\u26D1"; //⛑️
        const string thinkingEmoji = "\U0001F914"; //🤔

#if UNITY_EDITOR

        [MenuItem("MAD/Notifications/Settings", false, 100)]
        static void EditSettings()
        {
            Selection.activeObject = GetSO();
        }
#endif


        [PropertySpace(10, 10)] public int LevelToSend = 10;

        [PropertySpace(10, 10)] public List<int> DaysSinceGameLastOpenedToSend = new List<int> { 1, 3, 5, 7 };

        [PropertySpace(10, 10)] public int HourToSend = 19;

        [PropertySpace(10, 10)]
        [SerializeField]
        List<NotificationMessage> m_Messages = new List<NotificationMessage>()
        {
            new NotificationMessage()
            {
                Title = $"Ready for a 'Pick Me Up' {coffeeEmoji} {jigsawEmoji}",
                Body = "Revitalize yourself and pick up where you left of!"
            },
            new NotificationMessage()
            {
                Title = $"Get back into the Grove! {manDanceEmoji} {discoBallEmoji} {womanDanceEmoji}",
                Body = "Come back and take on the challenge!"
            },
            new NotificationMessage()
            {
                Title = $"Pick up where you left off! {jigsawEmoji} {clockEmoji}",
                Body = "New Challenges are ready! Come see for yourself…"
            },
            new NotificationMessage()
            {
                Title = $"Did it get too hard? {rescueHatEmoji} {thinkingEmoji}",
                Body = "Don't quit now! New levels are waiting for you!"
            },
        };


        List<NotificationMessage> availableMessages = new List<NotificationMessage>();


        public NotificationMessage GetMessage()
        {
            if (availableMessages == null || availableMessages.Count == 0)
            {
                availableMessages = new List<NotificationMessage>();
                availableMessages.AddRange(m_Messages);
                availableMessages.Shuffle();
            }

            var messagee = availableMessages[0];
            availableMessages.RemoveAt(0);
            return messagee;

        }
    }


    [Serializable]
    public class NotificationMessage
    {
        public string Title;
        [TextArea(2, 3)] public string Body;
    }


}
