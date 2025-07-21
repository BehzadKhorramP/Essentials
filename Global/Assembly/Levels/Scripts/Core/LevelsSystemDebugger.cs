using Sirenix.OdinInspector;
using UnityEngine;

namespace MadApper.Levels
{
    public abstract class LevelsSystemDebugger : MonoBehaviour
    {
        public abstract ILevelsDatabase i_LevelDatabase { get; }

        [SerializeField] ResourceItemSO softResource;

        public void z_TrySetCurrentLevel(string input)
        {
            if (int.TryParse(input, out var value))
            {
                if (value < 1)
                    value = 1;

                SetCurrentLevel(value);
                SetCurrentStage(1);
                ReloadScene();
            }
        }
        public void z_TrySetCurrentStage(string input)
        {
            if (int.TryParse(input, out var value))
            {
                if (value < 1)
                    value = 1;

                SetCurrentStage(value);
                ReloadScene();
            }
        }
        public void z_GotoNextLevel()
        {
            var current = i_LevelDatabase.GetCurrentLevel();
            SetCurrentLevel(current + 1);
            SetCurrentStage(1);
            ReloadScene();
        }
        public void z_GotoPreviousLevel()
        {
            var current = i_LevelDatabase.GetCurrentLevel();
            var value = current - 1;
            if (value < 1) value = 1;
            SetCurrentLevel(value);
            SetCurrentStage(1);
            ReloadScene();
        }
        public void z_GotoNextStage()
        {
            var current = i_LevelDatabase.GetCurrentStage();
            SetCurrentStage(current + 1);
            ReloadScene();
        }
        public void z_GotoPreviousStage()
        {
            var current = i_LevelDatabase.GetCurrentStage();
            var value = current - 1;
            if (value < 1) value = 1;
            SetCurrentStage(value);
            ReloadScene();
        }


        public void z_SetSoftResource(string @input)
        {
            if (!int.TryParse(input, out int value)) return;
            softResource.Set(value);
        }

        [Button]
        public void SetCurrentLevel(int value)
        {
            i_LevelDatabase.SetCurrentLevel(value);
            i_LevelDatabase.SetLastWonLevel(value - 1);
        }
        [Button]
        public void SetCurrentStage(int value)
        {
            i_LevelDatabase.SetCurrentStage(value);
            i_LevelDatabase.SetLastWonStage(value - 1);
        }

        protected void ReloadScene()
        {
            ScenesLoader.Reload();
        }
    }








}
