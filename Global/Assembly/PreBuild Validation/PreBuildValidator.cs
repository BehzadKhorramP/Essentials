using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MadApper
{

    public interface IPreBuildValidate { }

#if UNITY_EDITOR

    public class PreBuildValidator
    {

        [MenuItem("PreBuild/Pre Build Validate", false, 100)]
        static void Validate()
        {

            var scriptableobjects = MADUtility.FindInterfacesSO_Editor<IPreBuildValidate>();
            var prefabs = MADUtility.FindInterfacesPrefab_Editor<IPreBuildValidate>();

            var all = scriptableobjects.Union(prefabs);

            int windowCount = 0;

            foreach (var item in all)
            {
                if (item is not Object obj)
                    continue;

                var inspectorWindow = EditorWindow.CreateWindow<CustomObjectInspectorWindow>(obj.name);

                inspectorWindow.position = new Rect(500 + windowCount * 200, 100 + windowCount * 100, 300, 400);

                inspectorWindow.SetTargetObject(obj);
                inspectorWindow.Show();

                windowCount++;
            }
        }

    }


    public class CustomObjectInspectorWindow : EditorWindow
    {
        private Object targetObject;
        private Editor objectEditor;
        private Vector2 scrollPosition;

        public void SetTargetObject(Object obj)
        {
            targetObject = obj;
            objectEditor = Editor.CreateEditor(targetObject);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (targetObject != null && objectEditor != null)
                objectEditor.OnInspectorGUI();
            else
                EditorGUILayout.LabelField("No target object assigned.");

            EditorGUILayout.EndScrollView();
        }



    }
#endif


}