#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MadApper
{
    [InitializeOnLoad]
    public static class ActiveableSystemsHandler
    {
        static ActiveableSystemsHandler()
        {
            EditorApplication.hierarchyChanged += TryHandle;
            Selection.selectionChanged += TryHandle;
        }

        private static void TryHandle()
        {
            if (Application.isPlaying)
                return;

            var selectedObject = Selection.activeGameObject;
          
            if (selectedObject != null)
            {
                var monoBehaviours = selectedObject.GetComponents<MonoBehaviour>();

                if (monoBehaviours != null)
                {

                    foreach (var behaviour in monoBehaviours)
                    {
                        Handle(behaviour);
                    }
                }
            }

            var selectedAsset = Selection.activeObject as ScriptableObject;

            if (selectedAsset != null)
            {
                Handle(selectedAsset);
            }
        }

        private static void Handle(Object obj)
        {
            if (obj == null)
                return;

            if (obj is not IActiveableSystem activeableSystem)
                return;

            ActiveSystemsSettings.TryAdd(activeableSystem.i_PrefabName);

        }
    }

}

#endif