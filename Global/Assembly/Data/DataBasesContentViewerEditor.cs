#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json.Linq;


[CustomEditor(typeof(DataBasesContentViewer))]
public class DataBasesContentViewerEditor : Editor
{
    private Vector2 scrollPosition;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        string saveDirectory = Application.persistentDataPath;
        string[] saveFiles = Directory.GetFiles(saveDirectory, "*.sav");

        foreach (string filePath in saveFiles)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUI.indentLevel = 0;

            EditorGUILayout.LabelField(Path.GetFileName(filePath), EditorStyles.boldLabel);

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                JObject jsonObject = JObject.Parse(jsonContent);

                DisplayJsonObject(jsonObject, 1);
            }
            catch (System.Exception e)
            {
                EditorGUILayout.LabelField($"Error reading file: {e.Message}");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        EditorGUILayout.EndScrollView();

        serializedObject.ApplyModifiedProperties();
    }

    private void DisplayJsonObject(JObject jsonObject, int indentLevel)
    {
        EditorGUI.indentLevel = indentLevel;

        foreach (var property in jsonObject.Properties())
        {
            if (property.Value.Type == JTokenType.Object)
            {
                EditorGUI.indentLevel = indentLevel;
                EditorGUILayout.LabelField(property.Name + ":");
                DisplayJsonObject((JObject)property.Value, indentLevel + 1);
            }
            else if (property.Value.Type == JTokenType.Array)
            {
                EditorGUI.indentLevel = indentLevel;
                EditorGUILayout.LabelField($"{property.Name} (Count: {((JArray)property.Value).Count}):");
                DisplayJsonArray((JArray)property.Value, indentLevel + 1);
            }
            else
            {
                EditorGUI.indentLevel = indentLevel;
                EditorGUILayout.LabelField($"{property.Name}: {property.Value}");
            }
        }
    }

    private void DisplayJsonArray(JArray jsonArray, int indentLevel)
    {
        EditorGUI.indentLevel = indentLevel;

        for (int i = 0; i < jsonArray.Count; i++)
        {
            if (jsonArray[i].Type == JTokenType.Object)
            {
                EditorGUI.indentLevel = indentLevel;
                EditorGUILayout.LabelField($"[{i}]:");
                DisplayJsonObject((JObject)jsonArray[i], indentLevel + 1);
            }
            else
            {
                EditorGUI.indentLevel = indentLevel;
                EditorGUILayout.LabelField($"[{i}]: {jsonArray[i]}");
            }
        }
    }
}

#endif