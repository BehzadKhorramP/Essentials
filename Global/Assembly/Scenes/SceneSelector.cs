using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace MadApper
{
    public class SceneSelector : SingletonScriptable<SceneSelector>
    {

        public List<SceneSO> m_ScenesSO;
        public SceneSO m_LoadingSO;

#if UNITY_EDITOR     

        public const string k_GeneratedClassName = "GeneratedSceneSelectorDropdowns";

        [MenuItem("MAD/Scenes/Settings", false, 100)]
        static void EditSettings()
        {
            Selection.activeObject = GetSO();
        }

        public static void Generate()
        {
            var so = GetSO();

            StringBuilder result = new StringBuilder();

            AddClassHeader(result);

            foreach (var scene in so.m_ScenesSO)
                AddCodeForDirectory(scene.m_SceneObject, result);

            AddClassFooter(result);

           // var soPath = AssetDatabase.GetAssetPath(so).Replace(so.name + ".asset", "").Replace("Assets", "");
            // var classPath = GetClassPath().Replace("Assets", "");

            var path = MADUtility.GetEssentialsFolder().Replace("Assets", "");
            string scriptPath = Application.dataPath + path + $"/{k_GeneratedClassName}.cs";

            File.WriteAllText(scriptPath, result.ToString());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static string GetClassPath()
        {
            var path = MADUtility.GetClassPath().Replace(nameof(SceneSelector) + ".cs", "").Replace("\\", "/");

            if (path.Last().Equals('/'))
                path = path.Remove(path.Length - 1);

            var dataPath = Application.dataPath;

            path = path.Replace(dataPath, "");
            path = "Assets" + path;

            return path;
        }


        static void AddClassHeader(StringBuilder result)
        {
            result.Append(@"#if UNITY_EDITOR
                           ");
            result.Append(@"using UnityEditor;
                             namespace MadApper  { 
                                               ");

            result.Append($"public class {k_GeneratedClassName}  {{");

        }

        static void AddCodeForDirectory(Object scene, StringBuilder resultt)
        {
            var filePath = AssetDatabase.GetAssetPath(scene);
            if (filePath.Contains(".unity"))
            {
                AddCodeForFile(filePath, scene, resultt);
            }

            void AddCodeForFile(string filePath, Object scene, StringBuilder resulttt)
            {
                string sceneName = scene.name.Replace(".unity", "");
                string functionName = filePath.Replace(".unity", "").Replace(" ", "").Replace("-", "");

                resulttt
                    .Append("        [MenuItem(\"Scenes/")
                    .Append(sceneName)
                    .Append("\")]")
                    .Append(Environment.NewLine);

                resulttt
                    .Append("        public static void Load")
                    .Append("_")
                    .Append(sceneName)
                    .Append("() { SceneSelector.OpenScene(\"")
                    .Append(filePath).Append("\"); }")
                    .Append(Environment.NewLine); ;
            }
        }

        static void AddClassFooter(StringBuilder result)
        {
            result.Append(@"
                  }
              }
              ");
            result.Append(@"#endif");
        }





        public static void OpenScene(string scenePath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }






        static T[] GetAllInstances<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }
        static T CreateSO<T>(string id, string path) where T : ScriptableObject
        {
            T so = CreateInstance<T>();
            var pathdir = path + "/" + id + ".asset";
            AssetDatabase.CreateAsset(so, pathdir);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"{so} is created!");

            return so;
        }

#endif  


    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SceneSelector))]
    public class SceneSelectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.Space(40);

            if (GUILayout.Button("Generate!", GUILayout.Height(40)))
            {
                SceneSelector.Generate();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
