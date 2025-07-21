namespace MadApper.Levels
{
    public interface ILevelsDatabase
    {
        #region Level

        int GetLastWonLevel();
        int GetCurrentLevel();
        void SetLastWonLevel(int value);
        void SetCurrentLevel(int value);
        void SetLastWonLevelForced(int value);

        #endregion

        #region Stage

        public int GetLastWonStage();
        public int GetCurrentStage();
        public void SetLastWonStage(int value);
        public void SetCurrentStage(int value);

        #endregion
    }

}
