using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;
using BEH;

#if TINYSAUCE_ENABLED
using Voodoo.Tiny.Sauce.Internal;
#endif

namespace MadApper
{
    public class TinySaucePrefabLoader : MonoBehaviour,IActiveableSystem
    {
        const string k_PrefabName = "TinySaucePrefabLoader";
        const string k_Directory = "Assets/VoodooPackages";
        public string i_PrefabName => k_PrefabName;

        [SerializeField] GameObject m_Prefab;


#if TINYSAUCE_ENABLED


#if UNITY_EDITOR

        [Button]
        private void Validate()
        {
            if (m_Prefab != null)
                return;

            var allPrefabs = MADUtility.GetAllPrefabs_Editor<TinySauceBehaviour>(k_Directory);

            if (allPrefabs != null && allPrefabs.Count > 0)
            {
                m_Prefab = allPrefabs[0].gameObject;
                EditorUtility.SetDirty(this);

                if (allPrefabs.Count > 0)
                {
                    Debug.LogWarning("There is more than one TinySauce prefab in the project!");
                }
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeIAPSystemOnLoad()
        {
            if (!IActiveableSystem.IsActive(k_PrefabName))
                return;

            var obj = MADUtility.TryLoadAndInstantiate<TinySaucePrefabLoader>(k_PrefabName);

            if (obj == null)
                return;

            var tsPrefab = obj.m_Prefab;

            if (tsPrefab == null)
            {
                Debug.LogWarning($"couldnt get TinySauce prefab!");
                return;
            }

            Instantiate(tsPrefab);
        }

#endif

    }
}
