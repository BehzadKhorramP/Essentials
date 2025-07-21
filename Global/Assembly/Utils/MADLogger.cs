#define DEVELOPMENT

using System.Text;
using UnityEngine;


namespace MadApper
{

    public static class MADLogger
    {
        const string k_LightBlue = "lightblue";
        const string k_Cyan = "cyan";
        const string k_Red = "red";
        const string k_Green = "green";
        const string k_Yellow = "yellow";
        const string k_Orange = "orange";
        const string k_Pink = "#FFCFCB";

        const int k_Priority = 0;


        public static void LogCustom(this Object myObj, object msg, string color, int priority)
        {
#if UNITY_EDITOR || DEVELOPMENT
            if (priority < k_Priority)
                return;

            Debug.Log($"[<color={color}>{myObj.name}</color>] : [<color={color}>{msg}</color>]", myObj);
#endif
        }
        public static void LogCustom(this string msg, string color, int priority)
        {
#if UNITY_EDITOR || DEVELOPMENT
            if (priority < k_Priority)
                return;

            Debug.unityLogger.Log($"[<color={color}>{msg}</color>]");
#endif
        }



        public static void Log(this Object myObj, object msg, int priority = 0)
        {
            myObj.LogCustom(msg, k_LightBlue, priority);
        }
        public static void Log(this Object myObj, params object[] msg)
        {
#if UNITY_EDITOR || DEVELOPMENT

            StringBuilder message = new StringBuilder();
            foreach (var item in msg)
            {
                message.Append(item);
                message.Append(" | ");
            }
            myObj.LogCustom(message.ToString(), k_LightBlue, 0);
#endif
        }

        public static void Log(this string msg, int priority = 0)
        {
            msg.LogCustom(k_LightBlue, priority);
        }

        public static void LogWithPriority(this Object myObj, int priority = 0, params object[] msg)
        {
#if UNITY_EDITOR || DEVELOPMENT

            if (priority < k_Priority)
                return;

            StringBuilder message = new StringBuilder();
            foreach (var item in msg)
            {
                message.Append(item);
                message.Append(" | ");
            }
            myObj.LogCustom(message.ToString(), k_LightBlue, priority);
#endif
        }



        public static void LogBlue(this Object myObj, object msg, int priority = 0)
        {
            myObj.LogCustom(msg, k_Cyan, priority);
        }
        public static void LogBlue(this string msg, int priority = 0)
        {
            msg.LogCustom(k_Cyan, priority);
        }
        public static void LogRed(this Object myObj, object msg, int priority = 0)
        {
            myObj.LogCustom(msg, k_Red, priority);
        }

        public static void LogGreen(this Object myObj, object msg, int priority = 0)
        {
            myObj.LogCustom(msg, k_Green, priority);
        }
        public static void LogGreen(this string msg, int priority = 0)
        {
            msg.LogCustom(k_Green, priority);
        }
        public static void LogYellow(this string msg, int priority = 0)
        {
            msg.LogCustom(k_Yellow, priority);
        }

        public static void LogYellow(this Object myObj, object msg, int priority = 0)
        {
            myObj.LogCustom(msg, k_Yellow, priority);
        }

        public static void LogOrange(this string msg, int priority = 0) 
        {
            msg.LogCustom(k_Orange, priority);
        }

        public static void LogOrange(this Object myObj, object msg, int priority = 0)
        {
            myObj.LogCustom(msg, k_Orange, priority);
        }

        public static void LogPink(this string msg, int priority = 0)
        {
            msg.LogCustom(k_Pink, priority);
        }

        public static void LogPink(this Object myObj, object msg, int priority = 0)
        {
            myObj.LogCustom(msg, k_Pink, priority);
        }

        public static void LogCyan(this string msg, int priority = 0)
        {
            msg.LogCustom(k_Cyan, priority);
        }

        public static void LogCyan(this Object myObj, object msg, int priority = 0)
        {
            myObj.LogCustom(msg, k_Cyan, priority);
        }
        
        

        public static void LogWarning(this Object myObj, object msg)
        {
            Debug.LogWarning($"[{myObj.name}] : [{msg}]", myObj);
        }
        public static void LogWarning(this string msg)
        {
            Debug.unityLogger.LogFormat(LogType.Warning, msg);
        }
        public static void LogError(this Object myObj, object msg)
        {
            Debug.LogError($"[{myObj.name}] : [{msg}]", myObj);
        }
        public static void LogError(this string msg)
        {
            Debug.unityLogger.LogFormat(LogType.Error, msg);
        }
    }




}