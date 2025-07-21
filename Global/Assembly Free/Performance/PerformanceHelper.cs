using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper
{
    public class PerformanceHelper : MonoBehaviour
    {
        public const int maxHeight_IOS = 2532;
        public const int maxHeight_Android = 2400;

        [Space(10)][SerializeField] SingletonScriptableHelper<PerformanceSettingsSO> m_Settings;
        [Space(10)] public float UpdateInterval = 0.3f;
        [Space(10)] public float FPSThreshold = 40;

        protected float _framesAccumulated = 0f;
        protected float _framesDrawnInTheInterval = 0f;
        protected float _timeLeft;
        protected int _currentFPS;
        protected int _totalFrames = 0;
        protected int _average;

        int _tier;
        float _badFPSCounter;
        float _badFPSCounterThreshold = 30;

        List<PerformanceSettingsSO.Tier> _tiers;

        [NonSerialized] static Vector2Int? initResSizes;
        static Vector2Int s_InitResSize => initResSizes ?? (initResSizes = new Vector2Int(Screen.width, Screen.height)).Value;




        private async void OnEnable()
        {
            _tiers = null;

            // just to give scene loader time to refresh activeScene
            await UniTask.DelayFrame(5);

            if (m_Settings.RunTimeValue == null)
                return;

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            QualitySettings.vSyncCount = 0;

            _timeLeft = UpdateInterval;
            _tier = DataBase.Data.Tier;
            _tiers = m_Settings.RunTimeValue.GetTiers();

            if (_tiers == null)
                return;

            UpdateTier();
        }
        protected void Update()
        {
            if (_tiers == null)
                return;

            if (_tier + 1 >= _tiers.Count)
                return;

            _framesDrawnInTheInterval++;
            _framesAccumulated = _framesAccumulated + Time.timeScale / Time.deltaTime;
            _timeLeft = _timeLeft - Time.deltaTime;

            if (_timeLeft <= 0.0)
            {
                _currentFPS = (int)Mathf.Clamp(_framesAccumulated / _framesDrawnInTheInterval, 0, 300);
                _framesDrawnInTheInterval = 0;
                _framesAccumulated = 0f;
                _timeLeft = UpdateInterval;
                _totalFrames++;
                _average += (_currentFPS - _average) / _totalFrames;

                if (_currentFPS >= 0 && _currentFPS <= 300)
                {
                    if (_currentFPS < FPSThreshold)
                        _badFPSCounter++;
                    else if (_badFPSCounter > 0)
                        _badFPSCounter--;

                    if (_badFPSCounter > _badFPSCounterThreshold)
                        GoToNextTier();
                }
            }
        }
        void GoToNextTier()
        {
            _badFPSCounter = 0;
            //   _badFPSCounterThreshold *= 2;
            _tier++;
            DataBase.SetTier(_tier);
            UpdateTier();
        }

        void UpdateTier()
        {
            if (_tier >= _tiers.Count)
                _tier = _tiers.Count - 1;

            var t = _tiers[_tier];

            SetResolution(t.m_ResolutionDivider);

            SetFPS(t.m_FPS);

            t.m_OnTierEvent?.Invoke();
        }


        void SetFPS(int fps)
        {
            if (fps < FPSThreshold)
                return;

            Application.targetFrameRate = fps;
        }

        void SetResolution(float divider)
        {
            var res = maxHeight_IOS;

#if UNITY_ANDROID
            res = maxHeight_Android;
#endif

            res = Mathf.RoundToInt(res / divider);

            Vector2Int targetRes = s_InitResSize;

            if (targetRes.y > res)
            {
                float scaleFactor = s_InitResSize.y / (res * 1f);
                targetRes = new Vector2Int((int)(s_InitResSize.x / scaleFactor), (int)(s_InitResSize.y / scaleFactor));
            }

            Screen.SetResolution(targetRes.x, targetRes.y, true);

            this.LogRed($"Tier : {_tier} - Resolution : {targetRes.x} - {targetRes.y}");
        }


        // hooked to Targeted Event SO (same name)
        public void z_VibrationOffForPerformanceResons()
        {
            GameSettingsData.IsVibrationOn = false;
        }

        public void z_SetTierDebug(string @input)
        {
            if (int.TryParse(@input, out int t))
            {
                _badFPSCounter = 0;

                if (t < 0) t = 0;
                if (t >= _tiers.Count) t = _tiers.Count - 1;

                _tier = t;

                DataBase.SetTier(_tier);
                UpdateTier();
            }
        }

        public class SaveData : ISaveable
        {
            static string saveName = "PerformanceData";
            public string SaveName => saveName;
            public string Hash { get; set; }
            public bool IgnoreHash => false;

            public int Tier = 0;
        }

        public class DataBase : DataBase<SaveData>
        {
            public static void SetTier(int tier)
            {
                Data.Tier = tier;
                Save();
            }
        }
    }
}
