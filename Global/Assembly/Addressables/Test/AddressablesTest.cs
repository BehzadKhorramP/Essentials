using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MadApper.Addressable
{
    public class AddressablesTest : MonoBehaviour
    {
        public List<GameObject> m_Prefabs;

        public List<AssetReference> m_Refs;

#if UNITY_EDITOR
        [Button]
        public void CreateAddressableGroups()
        {
            m_Refs = new List<AssetReference>();

            foreach (var item in m_Prefabs)
            {
                var name = $"Test_{item.name}_Group";
                var ar = item.SetAddressableGroup(name);
                m_Refs.Add(ar);
            }
        }
#endif
        [Button]
        public async void LoadRefrencesAsync()
        {
            if (m_Refs == null)
                return;

            var loadings = new List<AddressableToLoadArg<GameObject>>();

            foreach (var item in m_Refs)
            {
                loadings.Add(new AddressableToLoadArg<GameObject>()
                {
                    AssetReference = item,
                    OnLoaded = (obj) => Instantiate(obj, transform)
                });
            }

            Action<float> onTotalProgress = (x) => { Debug.Log($"TOTAL:{x}"); };

            await loadings.Load(onTotalProgress);

        }

    }
}
