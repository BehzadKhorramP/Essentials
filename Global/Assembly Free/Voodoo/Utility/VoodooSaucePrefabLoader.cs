using UnityEditor;
using UnityEngine;
#if VOODOOSAUCE_ENABLED
using Voodoo.Sauce.Core;
#endif

namespace BEH
{

    public class VoodooSaucePrefabLoader : MonoBehaviour
    {
        const string k_Directory = "Assets/VoodooSauce";
        const string k_PrefabName = "VoodooSaucePrefabLoader";

        public GameObject m_VoodooSaucePrefab;

#if VOODOOSAUCE_ENABLED


#if UNITY_EDITOR

        [Button]
        private void Validate()
        {
            if (m_VoodooSaucePrefab != null)
                return;

            var allPrefabs = BEHUtility.GetAllPrefabs_Editor<VoodooSauceBehaviour>(k_Directory);

            if (allPrefabs != null && allPrefabs.Count > 0)
            {
                m_VoodooSaucePrefab = allPrefabs[0].gameObject;
                EditorUtility.SetDirty(this);

                if (allPrefabs.Count > 0)
                {
                    Debug.LogWarning("There is more than one VoodooSauce prefab in the project!");
                }
            }
        }
#endif


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeIAPSystemOnLoad()
        {
            var obj = BEHUtility.TryLoadAndInstantiate<VoodooSaucePrefabLoader>(k_PrefabName);

            if (obj == null)
                return;

            var VSprefab = obj.m_VoodooSaucePrefab;

            if (VSprefab == null)
            {
                Debug.LogWarning($"couldnt get VoodooSauce prefab!");
                return;
            }

            Instantiate(VSprefab);
        }


#endif

    }


}