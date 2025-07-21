using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MadApper
{
    [CreateAssetMenu(fileName = "SceneSO", menuName = "ScenesObserver/SceneSO")]
    public class SceneSO : ScriptableObject, ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        public Object m_SceneObject;
#endif
        public string m_SceneName;
        public bool m_DontUnload;     

        public void z_LoadSceneAdditive() => ScenesLoader.LoadSceneAdditive(m_SceneName);
        public void z_LoadSceneOld() => SceneManager.LoadScene(m_SceneName);

        public void OnAfterDeserialize() { }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (m_SceneObject == null)
                return;

            var path = AssetDatabase.GetAssetPath(m_SceneObject);

            if (path.Contains(".unity"))
            {
                m_SceneName = m_SceneObject.name.Replace(".unity", "");
                return;
            }

            Debug.LogWarning($"{m_SceneObject} is not a scene object!");

            m_SceneObject = null;
#endif
        }
    }
}
