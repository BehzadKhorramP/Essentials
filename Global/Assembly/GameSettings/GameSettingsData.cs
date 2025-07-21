using System;

namespace MadApper
{
    public static class GameSettingsData
    {     
        [Serializable]
        public class SaveData : ISaveable
        {
            readonly string name = "GameSettingsData";
            public string SaveName => name;

            public bool IsMusicOn;
            public bool IsSoundOn;
            public bool IsVibrationOn;
            public SaveData()
            {               
                IsMusicOn = true;
                IsSoundOn = true;
                IsVibrationOn = true;
            }

            public string Hash { get; set; }
            public bool IgnoreHash => false;
        }

        public class DataBase : DataBase<SaveData> { }


        public static bool IsMusicOn
        {
            get
            {
                return DataBase.Data.IsMusicOn;
            }
            set
            {
                DataBase.Data.IsMusicOn = value;
                DataBase.Save();
            }
        }
        public static bool IsSoundOn
        {
            get
            {
                return DataBase.Data.IsSoundOn;
            }
            set
            {
                DataBase.Data.IsSoundOn = value;
                DataBase.Save();
            }
        }
        public static bool IsVibrationOn
        {
            get
            {
                return DataBase.Data.IsVibrationOn;
            }
            set
            {
                DataBase.Data.IsVibrationOn = value;
                DataBase.Save();
            }
        }
    }

}