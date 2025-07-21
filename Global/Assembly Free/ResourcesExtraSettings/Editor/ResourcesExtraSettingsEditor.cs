using UnityEditor;
using UnityEngine;


namespace MadApper
{
    [CustomEditor(typeof(ResourcesExtraSettings))]
    public class ResourcesExtraSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ResourcesExtraSettings script = (ResourcesExtraSettings)target;

            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.Space(50);

            if (GUILayout.Button("ReGenerate Class!", GUILayout.Height(40)))
            {
                script.GenerateClass();
            }

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Refresh GameAnalytics Items!", GUILayout.Height(40)))
            {
                script.RefreshGameAnalyticsItems();
            }

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Refresh TinySauce Items!",GUILayout.Height(40)))
            {
                script.RefreshTinySauceItems();
            }



            serializedObject.ApplyModifiedProperties();
        }
    }

}