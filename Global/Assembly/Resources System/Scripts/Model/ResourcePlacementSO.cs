using UnityEditor;
using UnityEngine;

namespace MadApper
{

    [CreateAssetMenu(fileName = "ResourcePlacementSO", menuName = "Resources/ResourcePlacementSO")]
    public class ResourcePlacementSO : ScriptableObject
    {
        public string m_ID;


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            var setDirty = false;

            if (string.IsNullOrEmpty(m_ID))
            {
                m_ID = name;
                setDirty = true;
            }

            if (setDirty)
                EditorUtility.SetDirty(this);
        }
#endif

    }

}