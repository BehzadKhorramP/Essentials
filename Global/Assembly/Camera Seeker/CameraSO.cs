using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MadApper
{

    [CreateAssetMenu(fileName = "CameraSO", menuName = "Camera/CameraSO")]

    public class CameraSO : ScriptableObject
    {
        public string m_ID;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (name == m_ID)
                return;

            m_ID = name;
            EditorUtility.SetDirty(this);
        }
#endif
    }

}