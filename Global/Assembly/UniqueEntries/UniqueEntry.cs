using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MadApper
{
    public class UniqueEntry : MonoBehaviour
    {
        public string Key;

        [SerializeField] UnityEventDelayList m_Exists;
        [SerializeField] UnityEventDelayList m_DoesntExist;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (Key != name)
            {
                Key = name;
                EditorUtility.SetDirty(this);
            }
        }
#endif

        public void z_Examine()
        {
            if (string.IsNullOrEmpty(Key))
                return;

            var exists = UniqueEntriesSystem.Exists(Key);

            if (exists)
                m_Exists?.Invoke();
            else
                m_DoesntExist?.Invoke();
        }

        public void z_Add() => UniqueEntriesSystem.Add(Key);

        [Button]
        public void DeleteEntry()
        {
            UniqueEntriesSystem.Remove(Key);
        }
    }
}
