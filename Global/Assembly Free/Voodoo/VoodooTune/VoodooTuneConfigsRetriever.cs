using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using MadApper.Singleton;
using MadApper;

namespace BEH.Voodoo
{
    public class VoodooTuneConfigsRetriever : PersistentSingleton<VoodooTuneConfigsRetriever>
    {
        const string k_PrefabName = "VoodooTuneConfigsRetriever";

        [Space(10)][SerializeField] SingletonScriptableHelper<VoodooTuneConfigsSettings> m_Settings;


        [Space(10)] public UnityEventDelay m_OnStarted;
        [Space(10)] public UnityEventDelay m_OnFinished;

        SceneChangeSubscriber sceneChangeSubscriber;

        public static Action onVTConfigsRetrieved;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
#if VOODOOSAUCE_ENABLED
            BEHUtility.TryLoadAndInstantiate<VoodooTuneConfigsRetriever>(k_PrefabName);        
#endif
        }


        private void Start()
        {
            if (m_Settings.RunTimeValue == null)
                return;

            var scenes = m_Settings.RunTimeValue.m_ScenesValidForInitialization.Select(x => x.m_SceneName);

            sceneChangeSubscriber = new SceneChangeSubscriber.Builder()
                .SetOnSceneChanged(OnSceneChanged)
                .AddTargetScenes(scenes)
                .Build();
        }

        public async void OnSceneChanged(string scene)
        {
            sceneChangeSubscriber.Unsubscribe();

            m_OnStarted?.Invoke();

            var tasks = 0;

            foreach (var item in m_Settings.RunTimeValue.m_AllSOs)
            {
                if (item == null)
                    continue;

                tasks++;

                item.Retrieve(onComplete: () => tasks--);
            }

            await UniTask.WaitUntil(() => tasks <= 0);

            m_OnFinished?.Invoke();

            onVTConfigsRetrieved?.Invoke();

        }

        private void OnDisable()
        {
            if (m_Settings.RunTimeValue == null)
                return;
            foreach (var item in m_Settings.RunTimeValue.m_AllSOs)
            {
                if (item == null)
                    continue;

                item.OnReset();
            }
        }
    }
}
