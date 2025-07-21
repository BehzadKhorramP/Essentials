using System;
using System.Collections.Generic;
using UnityEngine;
using MadApper.Singleton;

#if GAMEANALYTICSSDK_ENABLED
using GameAnalyticsSDK;
#endif


namespace MadApper
{
    public class LevelPlaytimeTracker : LazyPersistentSingleton<LevelPlaytimeTracker>
    {
        public const double k_MaxTimePermitted = 600;

        public static string s_LevelID;
        public static bool s_CheckMaxTimePermitted;
        public static bool s_IsPaused;
        public static DateTime s_StartTime;
        public static double S_Timer;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            var awaker = Instance;

            NullifyLevelToTrackTimePlayed();

            //  s_IsPaused = false;
        }

        protected override void Awake()
        {
            base.Awake();

            var scene = new SceneChangeSubscriber.Builder()
                .SetOnSceneChanged(SceneChanged)
                .SetOnSceneToBeChangedReset(Reset)
                .Build();
        }

        private void OnDisable()
        {
            //if (s_IsPaused) return;
            //if (string.IsNullOrEmpty(s_LevelID)) return;

            //EndTimeTracking(s_LevelID);
            //NullifyLevelToTrackTimePlayed();
        }

#if UNITY_EDITOR
        void OnApplicationFocus(bool hasFocus)
        {
            if (string.IsNullOrEmpty(s_LevelID)) return;

            if (hasFocus)
            {
                s_IsPaused = false;
                ResumeTimeTracking();
            }
            else
            {
                s_IsPaused = true;
                PauseTimeTracking();
            }
        }
#endif

        private void OnApplicationPause(bool pause)
        {
            if (string.IsNullOrEmpty(s_LevelID)) return;

            if (pause)
            {
                s_IsPaused = true;
                PauseTimeTracking();                
            }
            else
            {
                s_IsPaused = false;
                ResumeTimeTracking();
            }
        }

        private void SceneChanged(string obj)
        {
            // s_IsPaused = false;
        }
        private void Reset(string obj)
        {
            //if (string.IsNullOrEmpty(s_LevelID)) return;

            //EndTimeTracking(s_LevelID);
            //NullifyLevelToTrackTimePlayed();
        }



        public static void NullifyLevelToTrackTimePlayed()
        {
            s_LevelID = null;
            S_Timer = 0;
        }

        public static void StartTimeTracking(string id, bool checkMaxTimePermitted = true)
        {
            s_LevelID = id;
            s_CheckMaxTimePermitted = checkMaxTimePermitted;

            //var savedData = DataBase.GetSaveData(id);

            S_Timer = 0;
            s_StartTime = DateTime.Now;

            $"Timer Started : [{id}]".Log();
        }

        public static void PauseTimeTracking()
        {
            var id = s_LevelID;

            var playTime = (DateTime.Now - s_StartTime).TotalSeconds;
            S_Timer += playTime;

            $"Timer Paused : [{id}] :  [{S_Timer}]".Log();
        }
        public static void ResumeTimeTracking()
        {
            var id = s_LevelID;

            //   var savedData = DataBase.GetSaveData(id);

            s_StartTime = DateTime.Now;

            $"Timer Resumed : [{id}]".Log();
        }

        public static void EndTimeTracking(string id)
        {
            // var savedData = DataBase.GetSaveData(id);

            var playTime = (DateTime.Now - s_StartTime).TotalSeconds;
            S_Timer += playTime;

            //   savedData.TimePlayed += playTime;
            //if (s_CheckMaxTimePermitted)
            //    if (savedData.TimePlayed > k_MaxTimePermitted)
            //        savedData.TimePlayed = k_MaxTimePermitted;

            if (s_CheckMaxTimePermitted)
                if (S_Timer > k_MaxTimePermitted)
                    S_Timer = k_MaxTimePermitted;
                        
            var @event = $"Levels:Playtime:{id}";
            var @value = (float)(S_Timer);

#if UNITY_EDITOR
            $"Timer Ended : [{id}]  :  [{S_Timer}]".Log();
            return;
#endif

#if GAMEANALYTICSSDK_ENABLED 
            GameAnalytics.NewDesignEvent(@event, @value);
#endif

            //   DataBase.Save();
        }




        //#region DB

        //[Serializable]
        //public class SaveData : ISaveable
        //{
        //    readonly string name = "TimeTrackingData";
        //    public string SaveName => name;

        //    public List<LevelTimeData> LevelsData;

        //    [Serializable]
        //    public class LevelTimeData
        //    {
        //        public string ID;
        //        public Double TimePlayed;

        //        public LevelTimeData(string iD)
        //        {
        //            ID = iD;
        //            TimePlayed = 0;
        //        }
        //    }

        //    public SaveData()
        //    {
        //        LevelsData = new List<LevelTimeData>();
        //    }
        //    public string Hash { get; set; }


        //    public bool IgnoreHash => false;

        //}
        //public class DataBase : DataBase<SaveData>
        //{

        //    public static SaveData.LevelTimeData GetSaveData(string id)
        //    {
        //        var item = Data.LevelsData.Find(x => x.ID.Equals(id));

        //        if (item != null)
        //            return item;

        //        item = new SaveData.LevelTimeData(id);

        //        Data.LevelsData.Add(item);
        //        Save();

        //        return item;
        //    }
        //}
        //#endregion


    }

}