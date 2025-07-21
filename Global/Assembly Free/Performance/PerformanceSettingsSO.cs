
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace MadApper
{
    public class PerformanceSettingsSO : SingletonScriptable<PerformanceSettingsSO>
    {

        [Serializable]
        public class SceneSpecific
        {
            public SceneSO SceneSO;
            public List<Tier> Tiers;

            public List<Tier> AndroidTiers;
            public List<Tier> IOSTiers;
            public List<Tier> EditorTiers;
        }

        [Serializable]
        public class Tier
        {
            public float m_ResolutionDivider = 1;
            public int m_FPS = 100;
            public UnityEventDelay m_OnTierEvent;
        }

        public List<SceneSpecific> Scenes;


#if UNITY_EDITOR
        [MenuItem("MAD/Performance/Settings", false, 100)]
        static void EditSettings()
        {
            Selection.activeObject = GetSO();
        }

#endif


        public List<Tier> GetTiers()
        {
            var scene = SceneManager.GetActiveScene();
            var data = Scenes.Find(x => x.SceneSO != null && x.SceneSO.m_SceneName.Equals(scene.name));

            if (data == null) data = Scenes[0];

#if UNITY_EDITOR
            if (data.EditorTiers != null && data.EditorTiers.Any())
                return data.EditorTiers;
#elif UNITY_ANDROID
            if (data.AndroidTiers != null && data.AndroidTiers.Any())
                return data.AndroidTiers;
#elif UNITY_IOS
            if (data.IOSTiers != null && data.IOSTiers.Any())
                return data.IOSTiers;
#endif

            return data.Tiers;
        }
    }
}
