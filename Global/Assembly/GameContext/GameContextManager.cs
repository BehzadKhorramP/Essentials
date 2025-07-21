using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MadApper;
using System;
using System.Linq;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using System.Reflection;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;

#endif


namespace BEH
{
    public abstract class GameContextManager : ScriptableObject
    {
        [SerializeField][Space(10)] protected List<GameContext> m_Contexts;

        [ValueDropdown(nameof(m_Contexts))]
        [SerializeField][Space(20)] protected GameContext m_ActiveContext;

#if UNITY_EDITOR

        [Title("Builder")]
        [SerializeField] protected DebugSettings m_DebugSettings;
        [FoldoutGroup("Builder")]
        [SerializeField] protected List<ContextItem<string>> m_GameName;
        [FoldoutGroup("Builder")]
        [SerializeField] protected List<ContextItem<string>> m_PackageID;
        [FoldoutGroup("Builder")]
        [SerializeField] protected List<ContextItem<Version>> m_BundleVersion;
        [FoldoutGroup("Builder")]
        [SerializeField] protected List<ContextItem<Icon>> m_Icon;

#if TINYSAUCE_ENABLED
        [FoldoutGroup("Builder")]
        [SerializeField] protected List<ContextItem<ScriptableObject>> m_TinySauceSettings;
#endif
        [PropertyOrder(10)]
        [Title("References")]      
        [FoldoutGroup("References")]
        [SerializeField][ReadOnly][AutoGetOrCreateSO] protected DebugSettingsSO m_DebugSettingsSO;

#if TINYSAUCE_ENABLED
        [SerializeField] protected ScriptableObject m_TinySauceSettingsInAction;
#endif

        #region Editor Sample
        //[MenuItem("GameContext/Edit", false, 100)]
        //static void EditSettings()
        //{
        //    Selection.activeObject = MADUtility.GetOrCreateSOAtEssentialsFolder<GameContextSystem>();
        //}
        #endregion

        private void OnValidate()
        {
            if (Application.isPlaying) return;

            m_Contexts = MADUtility.GetAllInstances_Editor<GameContext>().ToList();

            ValidateContextsLists();

            RefreshSwitches(triggredFromEditor: true);

            EditorUtility.SetDirty(this);
        }

        #region Validate Contexts Lists
        void ValidateContextsLists()
        {
            var fields = this.GetType()
               .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
               .Where(f => f.FieldType.IsGenericType &&
                       f.FieldType.GetGenericTypeDefinition() == typeof(List<>) &&
                       f.FieldType.GetGenericArguments()[0].IsGenericType &&
                       f.FieldType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(ContextItem<>));

            foreach (var field in fields)
            {
                var list = field.GetValue(this) as IList;

                if (list == null)
                {
                    Debug.LogError($"Field {field.Name} is not initialized.");
                    continue;
                }

                // Check & fix missing contexts
                if (!CheckAndFixContextCoverage(list, field.FieldType.GetGenericArguments()[0]))
                {
                    Debug.LogWarning($"List {field.Name} was missing contexts — fixed automatically!");
                }
            }
        }
        private bool CheckAndFixContextCoverage(IList list, Type contextItemType)
        {
            bool allContextsCovered = true;

            // Extract the 'Context' field info from the ContextItem<T> type
            var contextField = contextItemType.GetField("Context");

            // Extract existing contexts in the list (dynamically reading each item’s context)
            var presentContexts = new HashSet<GameContext>(
                list.Cast<object>().Select(item => (GameContext)contextField.GetValue(item))
            );

            // Check each required context
            foreach (var context in m_Contexts)
            {
                if (!presentContexts.Contains(context))
                {
                    Debug.LogWarning($"Context '{context}' is missing in list — adding default item.");

                    // Create a new ContextItem<T> instance with default values
                    var newItem = Activator.CreateInstance(contextItemType);

                    // Set the 'Context' field manually
                    contextField.SetValue(newItem, context);

                    // Add the newly created item to the list
                    list.Add(newItem);

                    allContextsCovered = false;
                }
            }

            return allContextsCovered;
        }

        #endregion

        #region Builder

        [TitleGroup("Validation", order: -1)]
        [Button(ButtonSizes.Large)]
        [PropertySpace(SpaceAfter = 20)]
        public void Validate()
        {
            OnValidate();

            if (m_ActiveContext == null)
                return;

            m_DebugSettingsSO.Switch(m_DebugSettings);

            SetProductName();
            SetApplicationIdentifier();
            SetVersion();
            SetTinySauce();
            SetIcons();
            SetAndroidKeys();
        }

        void SetProductName()
        {
            var product = GetContextItem(m_GameName);

            if (product == null)
                return;

            PlayerSettings.productName = product.Item;
        }
        void SetApplicationIdentifier()
        {
            BuildTargetGroup[] targetGroups = { BuildTargetGroup.Android, BuildTargetGroup.iOS };
            var package = GetContextItem(m_PackageID);

            if (package == null || string.IsNullOrEmpty(package.Item))
                return;

            foreach (var item in targetGroups)
                PlayerSettings.SetApplicationIdentifier(item, package.Item);
        }
        void SetVersion()
        {
            var version = GetContextItem(m_BundleVersion);

            if (version == null)
                return;

            PlayerSettings.bundleVersion = version.Item.BundleVersion;
            PlayerSettings.iOS.buildNumber = version.Item.BuildNumber.ToString();
            PlayerSettings.Android.bundleVersionCode = version.Item.BuildNumber;
        }
        void SetTinySauce()
        {
#if TINYSAUCE_ENABLED
           var tinySauceSettings = GetContextItem(m_TinySauceSettings);

           if (tinySauceSettings == null || m_TinySauceSettingsInAction == null)
               return;

           tinySauceSettings.Item.CopyShallowTo(m_TinySauceSettingsInAction);
           EditorUtility.SetDirty(m_TinySauceSettingsInAction);
#endif
        }

        void SetAndroidKeys()
        {
#if UNITY_ANDROID
            SettingsService.OpenProjectSettings("Project/Player");
#endif
        }
        void SetIcons()
        {
            var icon = GetContextItem(m_Icon);

            if (icon == null)
                return;

            Texture2D[] storeIcon = new Texture2D[1];
            storeIcon[0] = icon.Item.Texture2D;
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, storeIcon, IconKind.Any);

            Texture2D[] iconsIOS = new Texture2D[4];

            for (int i = 0; i < iconsIOS.Length; i++)
                iconsIOS[i] = icon.Item.Texture2D;

            PlayerSettings.SetIcons(NamedBuildTarget.iOS, iconsIOS, IconKind.Application);
            PlayerSettings.SetIcons(NamedBuildTarget.iOS, iconsIOS, IconKind.Settings);
            PlayerSettings.SetIcons(NamedBuildTarget.iOS, iconsIOS, IconKind.Notification);
            PlayerSettings.SetIcons(NamedBuildTarget.iOS, iconsIOS, IconKind.Spotlight);
            PlayerSettings.SetIcons(NamedBuildTarget.iOS, storeIcon, IconKind.Store);
        }
        #endregion


#endif

        public GameContext GetContext() => m_ActiveContext;
        public void SetContext(GameContext ctx) => m_ActiveContext = ctx;
        public abstract void RefreshSwitches(bool triggredFromEditor = false);



        [Serializable]
        public class ContextItem<T>
        {
            public GameContext Context;
            public T Item;
            // Default constructor (needed for Activator.CreateInstance)
            public ContextItem() { }

            // Custom convenience constructor
            public ContextItem(GameContext context, T item = default)
            {
                Context = context;
                Item = item;
            }


        }

        [Serializable]
        public class Version
        {
            public string BundleVersion;
            public int BuildNumber;
        }


        [Serializable]
        public class Icon
        {
            [PreviewField] public Texture2D Texture2D;
        }


        public ContextItem<T> GetContextItem<T>(List<ContextItem<T>> list)
        {
            return list.Find(x => x.Context == GetContext());
        }
    }
  


}
