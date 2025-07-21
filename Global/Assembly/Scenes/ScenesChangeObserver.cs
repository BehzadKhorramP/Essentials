using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MadApper
{


    public interface ISceneChangeSubscriber
    {
        public int GetPriority();
        public void OnSceneUnloaded(string scene);
        public void OnSceneChanged(string scene);
        public void OnSceneToBeChangedReset(string scene);
        public void OnSceneActivatedInitialize(string scene);
    }



    [DefaultExecutionOrder(-1000)]
    public static class ScenesChangeObserver
    {
        static List<ISceneChangeSubscriber> subscribers;
        static List<ISceneChangeSubscriber> s_Subscribers
        {
            get
            {
                if (subscribers == null)
                    subscribers = new List<ISceneChangeSubscriber>();
                return subscribers;
            }         
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.activeSceneChanged += OnSceneChanged;
            ScenesLoader.onSceneToBeChanged += OnSceneToBeChangedReset;
            ScenesLoader.onSceneActivated += OnSceneActivatedInitialize;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }


#if UNITY_EDITOR
        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Reset();
            }
        }
#endif

        static void Reset()
        {
            subscribers = null;

#if UNITY_EDITOR

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif

            SceneManager.activeSceneChanged -= OnSceneChanged;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            ScenesLoader.onSceneToBeChanged -= OnSceneToBeChangedReset;
            ScenesLoader.onSceneActivated -= OnSceneActivatedInitialize;
        }


        private static void OnSceneActivatedInitialize(string scene)
        {
            for (int i = s_Subscribers.Count - 1; i >= 0; i--)
            {
                var item = s_Subscribers[i];
                item.OnSceneActivatedInitialize(scene);
            }
        }

        private static void OnSceneToBeChangedReset(string scene)
        {
            for (int i = s_Subscribers.Count - 1; i >= 0; i--)
            {
                var item = s_Subscribers[i];             
                item.OnSceneToBeChangedReset(scene);
            }
        }

        static void OnSceneUnloaded(Scene scene)
        {
            var sceneName = scene.name;
            for (int i = s_Subscribers.Count - 1; i >= 0; i--)
            {
                var item = s_Subscribers[i];
                item.OnSceneUnloaded(sceneName);
            }
        }

        static void OnSceneChanged(Scene arg0, Scene scene)
        {
            var sceneName = scene.name;

            // it's in reverse cause some subscriber might unsubscribe in the process
            for (int i = s_Subscribers.Count - 1; i >= 0; i--)
            {
                var item = s_Subscribers[i];
                item.OnSceneChanged(sceneName);
            }
        }


        public static void Add(ISceneChangeSubscriber sub)
        {
            if (s_Subscribers.Contains(sub))
                return;

            s_Subscribers.Add(sub);

            // cause every events will be sent iterating backwards
            subscribers = subscribers.OrderBy(x => x.GetPriority()).ToList();

            var sceneName = SceneManager.GetActiveScene().name;

            sub.OnSceneUnloaded(sceneName);
            sub.OnSceneChanged(sceneName);
            sub.OnSceneActivatedInitialize(sceneName);
        }
        public static void Remove(ISceneChangeSubscriber sub)
        {
            if (!s_Subscribers.Contains(sub))
                return;

            s_Subscribers.Remove(sub);
        }
    }

    #region Subscribers


    /// <summary>   
    /// Order:   
    /// <para>
    /// OnSceneToBeChangedReset
    /// </para>
    /// <para>
    /// OnSceneUnloaded (maybe or not)
    /// </para>
    /// <para>
    /// OnSceneChanged
    /// </para>
    /// <para>
    /// OnSceneActivatedInitialize
    /// </para>   
    /// </summary>


    [Serializable]
    public class SceneChangeSubscriber : ISceneChangeSubscriber
    {
        int priority = 0;
       
        Action<string> m_OnSceneChanged;
        Action<string> m_OnSceneUnloaded;
        Action<string> m_OnSceneActivatedInitialize;
        Action<string> m_OnSceneToBeChangedReset;
        List<string> m_TargetScenes;

        private SceneChangeSubscriber() { }
        public void Subscribe() => ScenesChangeObserver.Add(this);
        public void Unsubscribe() => ScenesChangeObserver.Remove(this);

        public bool IsValid(string scene)
        {
            if (m_TargetScenes == null || m_TargetScenes.Count == 0)
                return true;
            return m_TargetScenes.Contains(scene);
        }

        public virtual void OnSceneChanged(string scene)
        {
            if (!IsValid(scene))
                return;
            m_OnSceneChanged?.Invoke(scene);
        }
        public virtual void OnSceneUnloaded(string scene)
        {
            if (!IsValid(scene))
                return;
            m_OnSceneUnloaded?.Invoke(scene);
        }
        public void OnSceneToBeChangedReset(string scene)
        {
            if (!IsValid(scene))
                return;
            m_OnSceneToBeChangedReset?.Invoke(scene);
        }
        public void OnSceneActivatedInitialize(string scene)
        {
            if (!IsValid(scene))
                return;
          
            m_OnSceneActivatedInitialize?.Invoke(scene);
        }

        public int GetPriority() => priority;

        public class Builder
        {
            readonly SceneChangeSubscriber source = new SceneChangeSubscriber();
            public Builder() { }

            public Builder SetOnSceneUnloaded(Action<string> onSceneUnloaded)
            {
                source.m_OnSceneUnloaded = onSceneUnloaded;
                return this;
            }
            public Builder SetOnSceneChanged(Action<string> onSceneChanged)
            {
                source.m_OnSceneChanged = onSceneChanged;
                return this;
            }
            public Builder SetOnSceneActivatedInitialize(Action<string> inialized)
            {
                source.m_OnSceneActivatedInitialize = inialized;
                return this;
            }
            public Builder SetOnSceneToBeChangedReset(Action<string> reset)
            {
                source.m_OnSceneToBeChangedReset = reset;
                return this;
            }
            public Builder AddTargetScenes(IEnumerable<string> scenes)
            {
                if (scenes == null)
                    return this;

                foreach (var scene in scenes)
                {
                    if (!string.IsNullOrEmpty(scene))
                    {
                        if (source.m_TargetScenes == null)
                            source.m_TargetScenes = new List<string>();

                        if (!source.m_TargetScenes.Contains(scene))
                            source.m_TargetScenes.Add(scene);
                    }
                }

                return this;
            }
            public Builder AddActiveScene()
            {
                var scene = SceneManager.GetActiveScene().name;

                if (source.m_TargetScenes == null)
                    source.m_TargetScenes = new List<string>();

                if (!source.m_TargetScenes.Contains(scene))
                    source.m_TargetScenes.Add(scene);

                return this;
            }
            public Builder AddGameObjectScene(GameObject go)
            {
                if (go != null)
                {
                    var scene = go.scene.name;

                    if (source.m_TargetScenes == null)
                        source.m_TargetScenes = new List<string>();

                    if (!source.m_TargetScenes.Contains(scene))
                        source.m_TargetScenes.Add(scene);
                }

                return this;
            }
                      
            public Builder Priority(int priority)
            {
                source.priority = priority;
                return this;
            }

            public SceneChangeSubscriber Build()
            {
                source.Subscribe();
                return source;
            }
        }
    }

    #endregion





}
