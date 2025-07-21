using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MadApper
{
    public static class ScenesLoader 
    {
        public static Action<string> onSceneToBeChanged;

        public static Action<string> onSceneActivated;

        public static async void LoadSceneAdditive(string sceneName)
        {
            var sceneSelector = SceneSelector.Instance;

            if (sceneSelector == null)
                return;

            if (string.IsNullOrEmpty(sceneName))
            {
                $"sceneName is nullOrEmpty!".LogWarning();
                return;
            }

            DOTween.KillAll();

            var currScene = SceneManager.GetActiveScene();

            onSceneToBeChanged?.Invoke(currScene.name);

            await UniTask.DelayFrame(2);

            await LoadScene(sceneSelector.m_LoadingSO.m_SceneName);

            UnloadAllScenes();

            await LoadScene(sceneName);

            UnloadScene(sceneSelector.m_LoadingSO.m_SceneName);
        }



        static async UniTask LoadScene(string sceneName)
        {
            var sceneToBeActive = SceneManager.GetSceneByName(sceneName);
            var isLoaded = sceneToBeActive.IsValid() && sceneToBeActive.isLoaded;

            if (!isLoaded)
            {
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                await UniTask.WaitUntil(() => asyncLoad.isDone == true);
                sceneToBeActive = SceneManager.GetSceneByName(sceneName);
            }

            await SetActiveScene(sceneToBeActive);
        }

        static async UniTask UnloadAllScenes()
        {
            List<UniTask> tasks = new List<UniTask>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                tasks.Add(UnloadScene(scene));
            }

            if (tasks != null)
                await UniTask.WhenAll(tasks);
        }

        static async UniTask UnloadScene(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            await UnloadScene(scene);
        }

        static async UniTask UnloadScene(Scene scene)
        {
            var sceneSelector = SceneSelector.Instance;

            if (sceneSelector == null)
                return;

            var sceneData = sceneSelector.m_ScenesSO.Find(x => x.m_SceneName.Equals(scene.name, StringComparison.OrdinalIgnoreCase));

            var shouldUnload = (scene.IsValid() && scene.isLoaded) && (sceneData == null || (sceneData != null && sceneData.m_DontUnload == false));

            if (shouldUnload)
            {
                var unload = SceneManager.UnloadSceneAsync(scene);

                if (unload != null)
                {
                    SceneActivation(scene, false);
                    await UniTask.WaitUntil(() => unload.isDone == true);
                }
            }
            else
            {
                SceneActivation(scene, false);
            }
        }

        static async UniTask SetActiveScene(Scene scene)
        {
            SceneManager.SetActiveScene(scene);
            SceneActivation(scene, true);
            await UniTask.DelayFrame(2);
            onSceneActivated?.Invoke(scene.name);
        }

        static void SceneActivation(Scene scene, bool active)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();

            foreach (GameObject rootObj in rootObjects)
                rootObj.SetActive(active);
        }




        public static void Reload()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            LoadSceneAdditive(sceneName);
        }

    }
}
