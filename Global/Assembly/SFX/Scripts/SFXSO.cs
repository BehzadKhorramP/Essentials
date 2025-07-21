using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MadApper
{
    [CreateAssetMenu(fileName = "SFX", menuName = "SFX/SFXSO")]
    public class SFXSO : ScriptableObject
    {
        [Space(10)] public string m_ID;

        [Space(10)] public AudioClip[] Clips;

        [Space(10)] public bool isOrdered;

        int? index;
        public AudioClip GetAudioClip()
        {
            if(Clips == null || Clips.Length == 0)
                return null;
            if (!isOrdered)
                return Clips.GetRandom();
            else
            {
                if (!index.HasValue)
                    index = Random.Range(0, Clips.Length);
                if (index >= Clips.Length)
                    index = 0;
                var val = Clips[index.Value];
                index++;
                return val;
            }
        }

        public AudioClip GetAudioClip(int index)
        {
            if (index< Clips.Length)
                return Clips[index];

            return Clips[^1];
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(m_ID))
                return;

            m_ID = name;

            EditorUtility.SetDirty(this);
        }
#endif
    }

}