using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;

#if GAMEANALYTICSSDK_ENABLED
using GameAnalyticsSDK;
#endif

#if TINYSAUCE_ENABLED
using Voodoo.Tiny.Sauce.Internal.Analytics;
#endif

namespace MadApper
{
    public class ResourcesExtraSettings : SingletonScriptable<ResourcesExtraSettings>, IPreBuildValidate
    {
        [Space(10)] public ResourceItemSO[] m_AllResources;
        [Space(10)] public ResourcePlacementSO[] m_AllPlacements;


        const string k_GeneratedClassName = "GeneratedResourcesExtra";
        const string k_EnumResourcesName = "ResourcesEnum";
        const string k_EnumPlacementsName = "PlacementsEnum";


#if RESOURCESEXTRA_ENABLED
        public GeneratedResourcesExtra.ResourcesEnum m_ResourcesEnum;
        public GeneratedResourcesExtra.PlacementsEnum m_PlacementsEnum;
#endif

       

#if UNITY_EDITOR

        [MenuItem("MAD/Resources/Extra Settings", false, 100)]
        static void Edit()
        {
            Selection.activeObject = GetSO();
        }
      

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            var length = 0;
            var setDirty = false;

            if (m_AllResources != null)
                length = m_AllResources.Length;

            m_AllResources = m_AllResources.GetAllInstances_Editor();

            if (length != m_AllResources.Length)
                setDirty = true;

            length = 0;

            if (m_AllPlacements != null)
                length = m_AllPlacements.Length;

            m_AllPlacements = m_AllPlacements.GetAllInstances_Editor();

            if (length != m_AllPlacements.Length)
                setDirty = true;

            if (setDirty)
                EditorUtility.SetDirty(this);

        }



        List<string> GetResourcesUnique()
        {
            var uniqueItems = new List<string>();
            foreach (var item in m_AllResources)
                if (!uniqueItems.Contains(item.m_ID))
                    uniqueItems.Add(item.m_ID);

            return uniqueItems;
        }
        List<string> GetPlacementsUnique()
        {
            var uniqueItems = new List<string>();
            foreach (var item in m_AllPlacements)
                if (!uniqueItems.Contains(item.m_ID))
                    uniqueItems.Add(item.m_ID);

            return uniqueItems;
        }

        public void GenerateClass()
        {
            OnValidate();
           
            StringBuilder result = new StringBuilder();

            AddClassHeader(result);

            var uniqueItems = GetResourcesUnique();

            CreateEnum(result, enumName: k_EnumResourcesName, items: uniqueItems);

            uniqueItems = GetPlacementsUnique();

            CreateEnum(result, enumName: k_EnumPlacementsName, items: uniqueItems);

            AddClassFooter(result);

            // var soPath = AssetDatabase.GetAssetPath(so).Replace(so.name + ".asset", "");
          //  var classPath = GetClassPath().Replace("Assets", "");

            var path = MADUtility.GetEssentialsFolder().Replace("Assets", "");
            string scriptPath = Application.dataPath + path + $"/{k_GeneratedClassName}.cs";


            File.WriteAllText(scriptPath, result.ToString());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
          

        }

        public void CreateEnum(StringBuilder result, string enumName, List<string> items)
        {
            OpenEnum(result, enumName);

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                AddEnum(result, item);

                if (i != items.Count - 1)
                {
                    result.Append(",");
                }
            }

            CloseEnum(result);
        }



        static void AddClassHeader(StringBuilder result)
        {
            result.Append(@"namespace MadApper 
                          {
                           public class ");

            result.Append($"{k_GeneratedClassName}");
            result.Append(@"{");
        }

        static void OpenEnum(StringBuilder result, string enumName)
        {
            result.Append($"public enum {enumName}");
            result.Append(@" {");

        }

        static void AddEnum(StringBuilder result, string id)
        {
            result.Append($"{id}");
        }
        static void CloseEnum(StringBuilder result)
        {
            result.Append("}");
        }

        static void AddClassFooter(StringBuilder result)
        {
            result.Append(@"
                  }
              }");
        }

        public static string GetClassPath()
        {
            var path = MADUtility.GetClassPath().Replace(nameof(ResourcesExtraSettings) + ".cs", "").Replace("\\", "/");

            if (path.Last().Equals('/'))
                path = path.Remove(path.Length - 1);

            var dataPath = Application.dataPath;

            path = path.Replace(dataPath, "");
            path = "Assets" + path;

            return path;
        }




        public void RefreshGameAnalyticsItems()
        {
#if GAMEANALYTICSSDK_ENABLED
            var uniqueItems = GetResourcesUnique();

            GameAnalytics.SettingsGA.ResourceCurrencies = new List<string>();

            foreach (var uitem in uniqueItems)
            {
                if (!GameAnalytics.SettingsGA.ResourceCurrencies.Contains(uitem))
                    GameAnalytics.SettingsGA.ResourceCurrencies.Add(uitem);
            }

            uniqueItems = GetPlacementsUnique();

            GameAnalytics.SettingsGA.ResourceItemTypes = new List<string>();

            foreach (var uitem in uniqueItems)
            {
                if (!GameAnalytics.SettingsGA.ResourceItemTypes.Contains(uitem))
                    GameAnalytics.SettingsGA.ResourceItemTypes.Add(uitem);
            }

            EditorUtility.SetDirty(GameAnalytics.SettingsGA);
#endif

        }

        public void RefreshTinySauceItems()
        {
#if TINYSAUCE_ENABLED
            var uniqueItems = GetResourcesUnique();

            var settings = CurrencySettings.Load();

            settings.currencies = new List<string>();

            foreach (var uitem in uniqueItems)
            {
                if (!settings.currencies.Contains(uitem))
                    settings.currencies.Add(uitem);
            }

            uniqueItems = GetPlacementsUnique();
                        
            settings.itemTypes = new List<string>();

            foreach (var uitem in uniqueItems)
            {
                if (!settings.itemTypes.Contains(uitem))
                    settings.itemTypes.Add(uitem);
            }

            EditorUtility.SetDirty(settings);
#endif

        }

#endif
    }

}