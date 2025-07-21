
using System;
using System.Collections.Generic;

namespace MadApper
{
    /// <summary>
    /// it should create SceneChangeSubscriber on RuntimeInitializeOnLoadMethod once
    /// <code>
    /// Reseting the collection is called on OnSceneChanged!
    /// </code>
    /// </summary>  
    public abstract class StaticCollection<T> where T : class
    {
        protected static void OnInitializeOnLoadImpl(Action<string> onSceneChanged = null, Action<string> onSceneActivated = null)
        {
            new SceneChangeSubscriber.Builder()
                .SetOnSceneChanged((scene) => { TryReset(); onSceneChanged?.Invoke(scene); })
                .SetOnSceneActivatedInitialize((scene) => onSceneActivated?.Invoke(scene))
                .Build();
        }
        static List<T> privateCollection;
        public static List<T> s_Collection
        {
            get
            {
                if (privateCollection == null)
                    privateCollection = new List<T>();

                return privateCollection;
            }
            set
            {
                privateCollection = value;
            }
        }

        static void TryReset()
        {
            if (privateCollection == null)
                return;

            privateCollection = null;
        }


        public static void Add(T member)
        {
            if (s_Collection.Contains(member))
                return;

            s_Collection.Add(member);
        }

        public static void Remove(T member)
        {
            if (!s_Collection.Contains(member))
                return;

            s_Collection.Remove(member);
        }

    }
}
