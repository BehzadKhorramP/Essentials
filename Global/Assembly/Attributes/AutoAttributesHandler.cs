#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using Object = UnityEngine.Object;
using System.IO;

namespace MadApper
{
    [InitializeOnLoad]
    public static class AutoAttributesHandler
    {
        static AutoAttributesHandler()
        {
            EditorApplication.hierarchyChanged += OnHandleAutoAttributes;
            Selection.selectionChanged += OnHandleAutoAttributes;
        }

        private static void OnHandleAutoAttributes()
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
        private static void Handle(Object uObj)
        {
            if (uObj == null)
                return;

            var fields = uObj.GetType()?.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (fields == null)
                return;

            foreach (var field in fields)
            {
                var fieldType = field.FieldType;

                if (uObj is MonoBehaviour mono)
                {
                    var autoGetorAdd = field.GetCustomAttribute<AutoGetOrAddAttribute>();
                    if (autoGetorAdd != null)
                    {
                        var componentType = field.FieldType;
                        if (typeof(Component).IsAssignableFrom(componentType))
                        {
                            var currentComponent = field.GetValue(mono) as Component;

                            if (currentComponent == null)
                            {
                                var component = mono.GetComponent(componentType);

                                if (component == null)
                                    component = mono.gameObject.AddComponent(componentType);

                                field.SetValue(mono, component);
                                EditorUtility.SetDirty(mono);
                            }
                        }
                    }

                    var autoFind = field.GetCustomAttribute<AutoFindAttribute>();
                    if (autoFind != null)
                    {
                        var componentType = field.FieldType;
                        if (typeof(Component).IsAssignableFrom(componentType))
                        {
                            var currentComponent = field.GetValue(mono) as Component;

                            if (currentComponent == null)
                            {
                                var component = Object.FindObjectOfType(componentType);

                                if (component == null)
                                    mono.LogWarning($"{fieldType} is null");

                                field.SetValue(mono, component);
                                EditorUtility.SetDirty(mono);
                            }
                        }
                    }

                    var autoInChildren = field.GetCustomAttribute<AutoGetInChildrenAttribute>();
                    if (autoInChildren != null)
                    {

                        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var elementType = fieldType.GetGenericArguments()[0];
                            if (typeof(Component).IsAssignableFrom(elementType))
                            {
                                var list = field.GetValue(mono) as IList;

                                if (list == null)
                                {
                                    list = (IList)Activator.CreateInstance(fieldType);
                                    field.SetValue(mono, list);
                                }

                                list.Clear();

                                var components = mono.GetComponentsInChildren(elementType, true);
                                foreach (var component in components)
                                {
                                    list.Add(component);
                                }

                                EditorUtility.SetDirty(mono);
                            }
                        }
                        else if (typeof(Component).IsAssignableFrom(fieldType))
                        {
                            var currentComponent = field.GetValue(mono) as Component;

                            if (currentComponent == null)
                            {
                                var component = mono.GetComponentInChildren(fieldType, true);

                                if (component == null)
                                {
                                    if (autoInChildren.Mandatory)
                                        mono.LogWarning($"{fieldType} is null");
                                }
                                else
                                {
                                    field.SetValue(mono, component);
                                    EditorUtility.SetDirty(mono);
                                }
                            }
                        }
                    }

                    var autoInParent = field.GetCustomAttribute<AutoGetInParentAttribute>();
                    if (autoInParent != null)
                    {
                        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var elementType = fieldType.GetGenericArguments()[0];
                            if (typeof(Component).IsAssignableFrom(elementType))
                            {
                                var list = field.GetValue(mono) as IList;

                                if (list == null)
                                {
                                    list = (IList)Activator.CreateInstance(fieldType);
                                    field.SetValue(mono, list);
                                }

                                list.Clear();

                                var components = mono.GetComponentsInParent(elementType, true);
                                foreach (var component in components)
                                {
                                    list.Add(component);
                                }

                                EditorUtility.SetDirty(mono);
                            }
                        }
                        else if (typeof(Component).IsAssignableFrom(fieldType))
                        {
                            var currentComponent = field.GetValue(mono) as Component;

                            if (currentComponent == null)
                            {
                                var component = mono.GetComponentInParent(fieldType, true);

                                if (component == null)
                                {
                                    if (autoInParent.Mandatory)
                                        mono.LogWarning($"{fieldType} is null");
                                }
                                else
                                {
                                    field.SetValue(mono, component);
                                    EditorUtility.SetDirty(mono);
                                }

                            }
                        }
                    }
                }

                HandleAutoSO(field: field, uObj: uObj, obj: uObj);
            }
        }

        static void HandleAutoSO(FieldInfo field, Object uObj, object obj)
        {
            var fieldType = field.FieldType;

            HandleGenericFields(field, uObj, obj);
            HandleNestedFileds(field, uObj, obj);

            HandleAutoGetOrCreateSO(field, uObj, obj, fieldType);
            HandleAutoGetSOInDirectory(field, uObj, obj, fieldType);
        }
        static void HandleAutoGetOrCreateSO(FieldInfo field, Object uObj, object obj, Type fieldType)
        {
            var autoGetOrCreateSO = field.GetCustomAttribute<AutoGetOrCreateSOAttribute>();
            if (autoGetOrCreateSO != null && typeof(ScriptableObject).IsAssignableFrom(fieldType))
            {
                var currentValue = field.GetValue(obj) as ScriptableObject;

                if (currentValue == null)
                {
                    var asset = MADUtility.GetOrCreateSOAtEssentialsFolder(fieldType);
                    field.SetValue(obj, asset);
                    EditorUtility.SetDirty(uObj);
                }
            }
        }
        static void HandleAutoGetSOInDirectory(FieldInfo field, Object uObj, object obj, Type fieldType)
        {
            var autoGetInDir = field.GetCustomAttribute<AutoGetSOInDirectoryAttribute>();
            if (autoGetInDir != null && fieldType.IsGenericType && typeof(IList).IsAssignableFrom(fieldType))
            {
                var elementType = fieldType.GetGenericArguments()[0];
                if (typeof(ScriptableObject).IsAssignableFrom(elementType))
                {
                    var existingList = field.GetValue(obj) as IList;
                    if (existingList == null)
                    {
                        existingList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                        field.SetValue(obj, existingList);
                    }

                    string directoryPath = null;

                    if (autoGetInDir.UseAssetDirectory && uObj != null)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(uObj);
                        directoryPath = Path.GetDirectoryName(assetPath).Replace("\\", "/");
                    }
                    else if (!string.IsNullOrEmpty(autoGetInDir.PathOverride))
                    {
                        directoryPath = autoGetInDir.PathOverride;
                    }

                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        string[] guids = AssetDatabase.FindAssets($"t:{elementType.Name}", new[] { directoryPath });
                        var newList = new List<ScriptableObject>();

                        foreach (string guid in guids)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(guid);
                            var asset = AssetDatabase.LoadAssetAtPath(path, elementType) as ScriptableObject;
                            if (asset != null)
                                newList.Add(asset);
                        }

                        // Compare lists before replacing
                        bool hasChanged = existingList.Count != newList.Count ||
                                          !existingList.Cast<ScriptableObject>().SequenceEqual(newList);

                        if (hasChanged)
                        {
                            existingList.Clear();
                            foreach (var asset in newList)
                                existingList.Add(asset);

                            field.SetValue(obj, existingList);
                            EditorUtility.SetDirty(uObj);
                        }
                    }
                }
            }
        }


        static void HandleNestedSerializableFields(FieldInfo field, UnityEngine.Object uObj, object obj, HashSet<object> visited)
        {
            var fieldType = field.FieldType;

            // Skip primitive, enums, string, and Unity types
            if (fieldType.IsPrimitive || fieldType.IsEnum
                || fieldType == typeof(string) || typeof(Object).IsAssignableFrom(fieldType))
                return;

            // Skip arrays and generic collections
            if (fieldType.IsArray || typeof(IEnumerable).IsAssignableFrom(fieldType))
                return;

            // Check if it's marked as [Serializable] (non-Unity objects)
            if (!fieldType.IsSerializable)
                return;

            var nestedInstance = field.GetValue(obj);
            if (nestedInstance == null && !fieldType.IsAbstract)
            {
                try
                {
                    nestedInstance = Activator.CreateInstance(fieldType);
                    field.SetValue(obj, nestedInstance);
                    EditorUtility.SetDirty(uObj);
                }
                catch (MissingMethodException e)
                {
                    return;
                }
            }
            if (nestedInstance == null || visited.Contains(nestedInstance))
                return;

            visited.Add(nestedInstance);

            var nestedFields = fieldType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var nestedField in nestedFields)
            {
                HandleAutoSO(nestedField, uObj, nestedInstance);
            }
        }
        static void HandleGenericFields(FieldInfo genericField, Object uObj, object obj)
        {
            if (!genericField.FieldType.IsGenericType)
                return;

            var genericObj = genericField.GetValue(obj);

            if (genericObj == null)
                return;

            var fields = genericObj.GetType()?.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var genField in fields)
            {
                HandleAutoSO(genField, uObj: uObj, obj: genericObj);
            }
        }
        static void HandleNestedFileds(FieldInfo genericField, Object uObj, object obj)
        {
            if (!genericField.FieldType.IsNested)
                return;

            var nested = genericField.GetValue(obj);

            if (nested == null)
                return;

            var fields = nested.GetType()?.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var genField in fields)
            {
                HandleAutoSO(genField, uObj: uObj, obj: nested);
            }
        }

        private static MethodInfo FindSuitableStaticMethod(System.Type type)
        {
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var item in methods)
            {
                $"{item}".Log();
            }

            // Find a static method in the type that returns a ScriptableObject and takes no parameters
            return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                   .FirstOrDefault(method =>
                    method.ReturnType == type &&
                    method.GetParameters().Length == 0 &&
                    typeof(ScriptableObject).IsAssignableFrom(method.ReturnType));
        }
    }


}

#endif