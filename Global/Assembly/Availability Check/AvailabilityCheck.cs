using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation; 
#endif
using UnityEngine;

namespace MadApper
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class AvailabilityCheckTrigger
    {
        static AvailabilityCheckTrigger()
        {
            var allChecks = Resources.LoadAll<AvailabilityCheck>("");

            foreach (var item in allChecks)
            {
                item.Check();
            }
        }
    }
#endif

    [CreateAssetMenu(fileName = "AvailabilityCheck", menuName = "AvailabilityCheck/CheckSO")]
    public class AvailabilityCheck : ScriptableObject
    {
#if UNITY_EDITOR

        public string m_NameSpace;
        public string m_ScriptToCheck;
        public string m_Assembly;

        [Space(20)] public string m_DefineSymbol;


        [ContextMenu("Check")]
        public void Check()
        {

            if (string.IsNullOrEmpty(m_DefineSymbol))
                return;

            if (!string.IsNullOrEmpty(m_Assembly))
            {
                var isPresent = IsAssemblyPresent(m_Assembly);

                if (!isPresent)
                {
                    MADUtility.TryRemoveSymbol(m_DefineSymbol);
                    return;
                }

                MADUtility.TryAddSymbol(m_DefineSymbol);
            }

            if (!string.IsNullOrEmpty(m_NameSpace))
            {
                var isPresent = IsNamespacePresent(m_NameSpace);

                if (!isPresent)
                {
                    MADUtility.TryRemoveSymbol(m_DefineSymbol);
                    return;
                }

                MADUtility.TryAddSymbol(m_DefineSymbol);
            }

            else if (!string.IsNullOrEmpty(m_ScriptToCheck))
            {
                var isPresent = IsScriptPresent(m_ScriptToCheck);

                if (!isPresent)
                {
                    MADUtility.TryRemoveSymbol(m_DefineSymbol);
                    return;
                }

                MADUtility.TryAddSymbol(m_DefineSymbol);
            }

        }

        bool IsAssemblyPresent(string assembly)
        {
            Assembly[] availableAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);
            bool foundDOTween = false;

            for (int i = availableAssemblies.Length - 1; i >= 0; i--)
            {
                if (availableAssemblies[i].name.IndexOf(assembly, StringComparison.Ordinal) > -1)
                {
                    foundDOTween = true;
                    break;
                }
            }

            return foundDOTween;
        }
        bool IsScriptPresent(string scriptName)
        {
            string[] guids = AssetDatabase.FindAssets($"{scriptName} t:Script");
            return guids.Length > 0;
        }

        public static bool IsNamespacePresent(string namespaceName, string classname)
        {
            // Example type that you expect to be in the namespace
            string typeName = namespaceName + $".{classname}";

            var type = Type.GetType(typeName);

            if (type != null)
                return true;

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);

                if (type != null)
                    return true;
            }
            return false;
        }

        public static bool IsNamespacePresent(string namespaceName)
        {
            // Get all types in the current AppDomain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                // Get all types in the assembly
                var types = assembly.GetTypes();

                // Check if any type's namespace matches the desired namespace
                if (types.Any(t => t.Namespace != null && t.Namespace.Equals(namespaceName, StringComparison.Ordinal)))
                {
                    return true;
                }
            }

            // Namespace not found
            return false;
        }



#endif

    }

}
