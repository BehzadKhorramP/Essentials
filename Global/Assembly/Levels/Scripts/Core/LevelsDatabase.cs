using System;

namespace MadApper.Levels
{
    #region Database

    public class LevelsDatabase : ILevelsDatabase
    {
        #region Level

        public int GetLastWonLevel() => DataBase.Data.LastWonLevel;
        public int GetCurrentLevel() => DataBase.Data.CurrentLevel;
        public void SetLastWonLevel(int value)
        {
            var lastWon = GetLastWonLevel();
            if (value <= lastWon) return;
            DataBase.Data.LastWonLevel = value;
            DataBase.Save();
        }
        public void SetCurrentLevel(int value)
        {
            DataBase.Data.CurrentLevel = value;
            DataBase.Save();
        }
        public void SetLastWonLevelForced(int value)
        {
            DataBase.Data.LastWonLevel = value;
            DataBase.Save();
        }

        #endregion

        #region Stage

        public int GetLastWonStage() => DataBase.Data.LastWonStage;
        public int GetCurrentStage() => DataBase.Data.CurrentStage;
        public void SetLastWonStage(int value)
        {
            DataBase.Data.LastWonStage = value;
            DataBase.Save();
        }
        public void SetCurrentStage(int value)
        {
            DataBase.Data.CurrentStage = value;
            DataBase.Save();
        }



        #endregion

        [Serializable]
        public class SaveData : ISaveable
        {
            const string k_SaveName = "ILevelsData";
            public string SaveName => k_SaveName;
            public string Hash { get; set; }
            public bool IgnoreHash => false;

            public int LastWonLevel;
            public int LastWonStage;
            public int CurrentLevel = 1;
            public int CurrentStage = 1;

            public SaveData()
            {
                CurrentLevel = 1;
                CurrentStage = 1;
            }
        }
        public class DataBase : DataBase<SaveData> { }
    }

    #endregion
}