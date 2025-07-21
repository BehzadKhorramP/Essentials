
using MadApper.Bridge;
using MadApperEditor.Common;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MadApper.Levels.Editor
{

    public abstract class LevelEditor<TLevelSetsCollection, TLevelSet, TLevel, TStage> : OdinMenuEditorWindow
        where TLevelSetsCollection : LevelSetsCollection<TLevelSet, TLevel, TStage>
        where TLevelSet : LevelSet<TLevel, TStage>
        where TLevel : Level<TStage>
        where TStage : Stage

    {


        [NonSerialized] TLevelSetsCollection _collection;
        TLevelSetsCollection collection => _collection ??= MADUtility.GetOrCreateSOAtEssentialsFolder<TLevelSetsCollection>();


        [NonSerialized] EditorCommonAssets _assets;
        protected EditorCommonAssets assets => _assets ??= MADUtility.GetOrCreateSOAtEssentialsFolder<EditorCommonAssets>();

        [NonSerialized] LevelSetSettingsSO _levelSetSettings;
        protected LevelSetSettingsSO levelSetSettings => _levelSetSettings ??= MADUtility.GetOrCreateSOAtEssentialsFolder<LevelSetSettingsSO>();



        LevelSetCollectionMenuItem collectionMenuItem;

        object lastSelectedObject;


        #region Abstracts Properties
        public abstract ILevelsDatabase i_LevelDatabase { get; }

        #endregion


        #region Open Menu e.g.

        //  [MenuItem("MAD/Levels/Editor", false, 10000)]
        public static void Open<TLevelEditor>() where TLevelEditor : OdinMenuEditorWindow
        {
            var window = GetWindow<TLevelEditor>();

            window.titleContent = new GUIContent("Level Editor");
            window.Show();
        }
        #endregion


        #region Abstracts Methods
        protected abstract void OnSelectionChangedInternal(object obj);

        #endregion


        protected override void OnDisable()
        {
            base.OnDisable();
            if (this.MenuTree != null) this.MenuTree.Selection.SelectionChanged -= OnSelectionChanged;
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            if (collection == null)
                return null;

            var tree = new OdinMenuTree();

            RefreshCollection();
            EnsureControlSetExists();
            RefreshLevelItems();

            OdinMenuItem odinCollectionMenuItem = new(tree, LevelConsts.k_LevelSets, collectionMenuItem);
            tree.AddMenuItemAtPath("", odinCollectionMenuItem);

            foreach (var levelSet in collection.GetLevelSets())
            {
                var levelSetID = levelSet.SetID;
                LevelSetMenuItem levelSetMenuItem = new LevelSetMenuItem(assets, levelSet);
                OdinMenuItem odinLevelSetMenuItem = new(tree, levelSetID, levelSetMenuItem);
                tree.AddMenuItemAtPath("Level Sets", odinLevelSetMenuItem);

                string levelPath = $"{LevelConsts.k_LevelSets}/{levelSetID}";
                int levelIndex = 1;

                foreach (var level in levelSet.GetLevels())
                {
                    string levelName = $"{LevelConsts.k_LevelPrefix} {levelIndex}";
                    LevelMenuItem levelMenuItem = new LevelMenuItem(assets, level, levelSet);
                    OdinMenuItem odinLevelMenuItem = new(tree, levelName, levelMenuItem);
                    tree.AddMenuItemAtPath(levelPath, odinLevelMenuItem);

                    string stagePath = $"{LevelConsts.k_LevelSets}/{levelSetID}/{levelName}";
                    int stageIndex = 1;

                    foreach (var stage in level.GetStages())
                    {
                        string stageName = $"Stage {stageIndex}";
                        StageMenuItem stageMenuItem = new StageMenuItem(assets, stage, level, levelSet, collection);
                        OdinMenuItem odinStageMenuItem = new(tree, stageName, stageMenuItem);
                        tree.AddMenuItemAtPath(stagePath, odinStageMenuItem);

                        stageIndex++;
                    }

                    levelIndex++;
                }
            }

            tree.Selection.SelectionChanged += OnSelectionChanged;

            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            if (this.MenuTree == null) return;

            OdinMenuTreeSelection selected = this.MenuTree.Selection;

            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUILayout.FlexibleSpace();

                if (SirenixEditorGUI.ToolbarButton("Select"))
                    if (selected.SelectedValue is Object obj) SelectObject(obj);
                    else if (selected.SelectedValue is ICustomMenuItem customItem) SelectObject(customItem.GetObject());

                if (SirenixEditorGUI.ToolbarButton("Duplicate"))
                    if (selected.SelectedValue is ICustomMenuItem customItem) DuplicateObject(selected);

                if (SirenixEditorGUI.ToolbarButton("Refresh"))
                    ForceMenuTreeRebuild();

                if (SirenixEditorGUI.ToolbarButton("Delete"))
                    if (selected.SelectedValue is Object obj) DeleteObject(obj);
                    else if (selected.SelectedValue is ICustomMenuItem customItem) DeleteObject(customItem.GetObject());


            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }


        void OnSelectionChanged(SelectionChangedType selection)
        {
            if (selection != SelectionChangedType.ItemAdded) return;

            OdinMenuTreeSelection selected = this.MenuTree.Selection;
            var selectedValue = selected.SelectedValue;

            if (lastSelectedObject == selectedValue) return;
            lastSelectedObject = selectedValue;

            SetLevelDatabase(selectedValue);

            SetLevelSetSettings(selectedValue);

            OnSelectionChangedInternal(selectedValue);
        }

        void SetLevelDatabase(object selectedValue)
        {
            if (selectedValue == null) return;

            if (selectedValue is StageMenuItem stageItem)
            {
                var levelSet = stageItem.GetLevelSet();
                var level = stageItem.GetLevel();
                var stage = stageItem.Item;
                if (levelSet == null || level == null || stage == null) return;
                var levelIndex = levelSet.GetLevels().IndexOf(level);
                var stageIndex = level.GetStages().IndexOf(stage);

                i_LevelDatabase.SetCurrentLevel(levelIndex + 1);
                i_LevelDatabase.SetCurrentStage(stageIndex + 1);
                i_LevelDatabase.SetLastWonLevel(levelIndex);
                i_LevelDatabase.SetLastWonStage(stageIndex);
            }
        }

        void SetLevelSetSettings(object selectedValue)
        {
            if (selectedValue == null) return;

            TLevelSet levelSet = null;

            if (selectedValue is LevelSetMenuItem levelSetItem) levelSet = levelSetItem.Item;
            else if (selectedValue is LevelMenuItem levelItem) levelSet = levelItem.GetLevelSet();
            else if (selectedValue is StageMenuItem stageItem) levelSet = stageItem.GetLevelSet();

            if (levelSet == null || string.IsNullOrEmpty(levelSet.SetID)) return;

            levelSetSettings.Switch(new LevelSetSettings() { SetID = levelSet.SetID });
            levelSetSettings.TrySetDirty();
        }

        public static void SelectObject(Object obj)
        {
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        private void DuplicateObject(OdinMenuTreeSelection selection)
        {
            object selectedValue = selection.SelectedValue;

            if (selectedValue == null) return;

            var changed = false;

            if (selectedValue is LevelSetMenuItem levelSetMenuItem)
            {
                PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero),
                 new TextInputPopup(onSubmit: value =>
                 {
                     if (!string.IsNullOrEmpty(value))
                     {
                         var find = collection.GetLevelSets().Where(x => x.SetID.Equals(value, StringComparison.OrdinalIgnoreCase));
                         var valid = find == null || find.Count() == 0;

                         if (!valid)
                         {
                             EditorUtility.DisplayDialog("Error", $"A Level Set with the same name [{value}] already exists", "OK");
                             return;
                         }

                         changed = true;

                         var levelSet = levelSetMenuItem.Item;

                         DuplicateLevelSet(levelSet, value);
                     }
                 }));
            }

            if (selectedValue is LevelMenuItem levelMenuItem)
            {
                changed = true;

                var level = levelMenuItem.Item;
                var levelSet = levelMenuItem.GetLevelSet();

                DuplicateLevel(level, levelSet);
            }
            else if (selectedValue is StageMenuItem stageMenuItem)
            {
                changed = true;

                var stage = stageMenuItem.Item;
                var level = stageMenuItem.GetLevel();
                var levelSet = stageMenuItem.GetLevelSet();

                DuplicateStage(stage, level, levelSet);
            }

            if (!changed) return;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            RefreshCollection();
            EnsureControlSetExists();
            RefreshLevelItems();
            ForceMenuTreeRebuild();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            void DuplicateLevelSet(TLevelSet sourcelevelSet, string newLevelSetId)
            {
                var path = LevelConsts.GetAndEnsureLocalPath(newLevelSetId);
                var duplicate = Instantiate(sourcelevelSet);
                duplicate.UID = newLevelSetId;
                duplicate.SetID = newLevelSetId;
                duplicate.ClearLevels();

                var newPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, newLevelSetId + ".asset"));
                AssetDatabase.CreateAsset(duplicate, newPath);

                var sourceLevels = sourcelevelSet.GetLevels();

                for (int i = 0; i < sourceLevels.Count; i++)
                {
                    var sourceLevel = sourceLevels[i];
                    DuplicateLevel(sourceLevel, duplicate);
                }


                SelectObject(duplicate);
            }

            void DuplicateLevel(TLevel sourceLevel, TLevelSet levelSet)
            {
                var uid = levelSet.GetNewLevelUID();
                var path = LevelConsts.GetAndEnsureLocalPath(levelSet.UID);

                var duplicateLevel = Instantiate(sourceLevel);

                duplicateLevel.UID = uid;
                levelSet.AddLevel(duplicateLevel);
                duplicateLevel.ClearStages();

                var newPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, uid + ".asset"));
                AssetDatabase.CreateAsset(duplicateLevel, newPath);

                var sourceStages = sourceLevel.GetStages();

                for (int i = 0; i < sourceStages.Count; i++)
                {
                    var sourceStage = sourceStages[i];
                    DuplicateStage(sourceStage, duplicateLevel, levelSet);
                }


                SelectObject(duplicateLevel);
            }

            void DuplicateStage(TStage sourceStage, TLevel level, TLevelSet levelSet)
            {
                var uid = level.GetNewStageUID();
                var path = LevelConsts.GetAndEnsureLocalPath(levelSet.UID);

                var duplicateStage = Instantiate(sourceStage);

                duplicateStage.UID = uid;
                level.AddStage(duplicateStage);

                var newPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, uid + ".asset"));
                AssetDatabase.CreateAsset(duplicateStage, newPath);

                SelectObject(duplicateStage);
            }
        }


        void DeleteObject(Object obj)
        {
            var path = AssetDatabase.GetAssetPath(obj);

            if (obj is TLevelSet levelSet)
            {
                DeleteLevelSet(levelSet);
            }
            else if (obj is TLevel level)
            {
                DeleteLevel(level);
            }
            else if (obj is TStage stage)
            {
                DeleteStage(stage);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            RefreshCollection();
            EnsureControlSetExists();
            RefreshLevelItems();
            ForceMenuTreeRebuild();

            void DeleteLevelSet(TLevelSet levelSet)
            {
                if (levelSet == null) return;
                var levels = levelSet.GetLevels();

                for (int i = levels.Count - 1; i >= 0; i--)
                    DeleteLevel(levels[i]);

                var path = AssetDatabase.GetAssetPath(levelSet);
                AssetDatabase.DeleteAsset(path);
            }

            void DeleteLevel(TLevel level)
            {
                if (level == null) return;
                var stages = level.GetStages();

                for (int i = stages.Count - 1; i >= 0; i--)
                    DeleteStage(stages[i]);

                var path = AssetDatabase.GetAssetPath(level);
                AssetDatabase.DeleteAsset(path);
            }

            void DeleteStage(TStage stage)
            {
                if (stage == null) return;
                var path = AssetDatabase.GetAssetPath(stage);
                AssetDatabase.DeleteAsset(path);
            }
        }

        void RefreshLevelItems()
        {
            collection.GetLevelSets()?.ForEach(levelSet =>
            {
                levelSet.Refresh();
                levelSet.GetLevels()?.ForEach(level =>
                {
                    level.Refresh();

                    level.GetStages()?.ForEach(stage =>
                    {
                        stage.Refresh();
                    });
                });
            });
        }


        void RefreshCollection() => collection.Retrieve();
        void EnsureControlSetExists()
        {
            if (collectionMenuItem == null) collectionMenuItem = new LevelSetCollectionMenuItem(assets, collection);

            //var controlSet = collection.GetLevelSet(ISetBased.k_Control);

            //if (controlSet == null)
            //{
            //    collectionMenuItem.LevelSetID = ISetBased.k_Control;
            //    collectionMenuItem.CreateNewLevelSet();
            //    ForceMenuTreeRebuild();
            //}
        }



        #region Custom Menu Item
        public interface ICustomMenuItem
        {
            public Object GetObject();
        }
        public abstract class CustomMenuItem<TItem> : ICustomMenuItem where TItem : Object
        {
            [PropertyOrder(0)]
            [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
            public TItem Item;

            protected EditorCommonAssets assets;

            public CustomMenuItem(EditorCommonAssets assets, TItem item)
            {
                this.assets = assets;
                this.Item = item;
            }
            public Object GetObject() => Item;
        }
        public class LevelSetCollectionMenuItem : CustomMenuItem<TLevelSetsCollection>
        {
            public LevelSetCollectionMenuItem(EditorCommonAssets assets, TLevelSetsCollection item) : base(assets, item) { }


            [PropertySpace(50)]
            [PropertyOrder(0)]
            public string LevelSetID = ISetBased.k_Control;

            [PropertyOrder(1)]
            [Button(ButtonSizes.Large)]
            public void CreateNewLevelSet()
            {
                if (string.IsNullOrEmpty(LevelSetID))
                {
                    EditorUtility.DisplayDialog("Error", "LevelSetID is empty", "OK");
                    return;
                }

                var levelSetPath = LevelConsts.GetAndEnsureLocalPath(LevelSetID);

                TLevelSet levelSet = MADUtility.GetOrCreateSONew<TLevelSet>(LevelSetID, levelSetPath);
                levelSet.SetID = LevelSetID;
                levelSet.TrySetDirty();

                SelectObject(levelSet);
            }
        }
        public class LevelSetMenuItem : CustomMenuItem<TLevelSet>
        {
            public LevelSetMenuItem(EditorCommonAssets assets, TLevelSet item) : base(assets, item) { }

            [PropertySpace(20, 0)]
            [PropertyOrder(0)]
            [Button(ButtonSizes.Medium)]
            void RenameAndRefresh()
            {
                if (Item == null)
                {
                    EditorUtility.DisplayDialog("Error", "Level Set is null", "OK");
                    return;
                }

                Item.RenameByIndex();
            }

            [OnInspectorGUI]
            void Inspector()
            {
                GUILayout.Space(40);

                assets.DrawAddButton(64, 64, () => CreateNewLevel());
                assets.DrawLabel("New Level", 64, 64);
            }
            public TLevel CreateNewLevel()
            {
                if (Item == null)
                {
                    EditorUtility.DisplayDialog("Error", "Level Set is null", "OK");
                    return null;
                }

                var levelSetPath = LevelConsts.GetAndEnsureLocalPath(Item.UID);
                var levelUID = Item.GetNewLevelUID();
                var stageUID = $"{levelUID}{LevelConsts.k_StagePrefix} 1";

                TLevel level = MADUtility.GetOrCreateSONew<TLevel>(levelUID, levelSetPath);
                TStage stage = MADUtility.GetOrCreateSONew<TStage>(stageUID, levelSetPath);

                level.AddStage(stage);
                Item.AddLevel(level);

                SelectObject(level);

                return level;
            }

        }


        public class LevelMenuItem : CustomMenuItem<TLevel>
        {
            [SerializeField, HideInInspector] TLevelSet levelSet;
            public LevelMenuItem(EditorCommonAssets assets, TLevel item, TLevelSet levelSet) : base(assets, item)
            {
                this.levelSet = levelSet;
            }


            public TLevelSet GetLevelSet() => levelSet;

            [PropertySpace(20, 0)]
            [PropertyOrder(0)]
            [Button(ButtonSizes.Medium)]
            void RenameAndRefresh()
            {
                if (Item == null)
                {
                    EditorUtility.DisplayDialog("Error", "Level is null", "OK");
                    return;
                }
                Item.RenameByIndex();
            }


            [OnInspectorGUI]
            void Inspector()
            {
                GUILayout.Space(40);
                assets.DrawAddButton(64, 64, () => CreateNewStage());
                assets.DrawLabel("New Stage", 64, 64);
            }
            public TStage CreateNewStage()
            {
                if (Item == null)
                {
                    EditorUtility.DisplayDialog("Error", "Level is null", "OK");
                    return null;
                }

                var levelSetPath = LevelConsts.GetAndEnsureLocalPath(levelSet.UID);
                var stageUID = Item.GetNewStageUID();

                TStage stage = MADUtility.GetOrCreateSONew<TStage>(stageUID, levelSetPath);
                Item.AddStage(stage);

                SelectObject(stage);

                return stage;
            }
        }
        public class StageMenuItem : CustomMenuItem<TStage>
        {
            [SerializeField, HideInInspector] TLevelSetsCollection levelSetsCollection;
            [SerializeField, HideInInspector] TLevelSet levelSet;
            [SerializeField, HideInInspector] TLevel level;

            public TLevelSetsCollection GetLevelSetsCollection() => levelSetsCollection;
            public TLevelSet GetLevelSet() => levelSet;
            public TLevel GetLevel() => level;

            public StageMenuItem(EditorCommonAssets assets, TStage item, TLevel level, TLevelSet levelset, TLevelSetsCollection collection) : base(assets, item)
            {
                this.levelSetsCollection = collection;
                this.levelSet = levelset;
                this.level = level;
            }
        }

        #endregion
    }

}
