using MadApper;
using MadApper.Singleton;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BEH
{
    public class LoadingScreen : PersistentSingleton<LoadingScreen>, IActiveableSystem
    {
        const string k_PrefabName = "LoadingScreen";
        public string i_PrefabName => k_PrefabName;

        [SerializeField][ReadOnly][AutoGetOrAdd] AppearerEvents m_Events;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            if (!IActiveableSystem.IsActive(k_PrefabName))
                return;

            MADUtility.TryLoadAndInstantiate<LoadingScreen>(k_PrefabName);
        }

    }
}
