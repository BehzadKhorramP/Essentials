using ExcelDataReader;
using MadApperEditor.Common;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace MadApper.Levels.Editor
{
    public abstract class LevelExcelReader<TLevelSetsCollection, TLevelSet, TLevel, TStage> : ScriptableObject
         where TLevelSetsCollection : LevelSetsCollection<TLevelSet, TLevel, TStage>
         where TLevelSet : LevelSet<TLevel, TStage>
         where TLevel : Level<TStage>
         where TStage : Stage
    {

        public const string k_LevelSetTag = "LevelSet_";
        public const string k_NotRepatableTag = "NoRepat";
        public const string k_LoopTag = "Loop";


        [FoldoutGroup("Collection")]
        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        [SerializeField] TLevelSetsCollection _collection;
        protected TLevelSetsCollection levelSetsCollection => _collection ??= MADUtility.GetOrCreateSOAtEssentialsFolder<TLevelSetsCollection>();

        [NonSerialized] EditorCommonAssets _assets;
        protected EditorCommonAssets assets => _assets ??= EditorCommonAssets.Get();

        [TitleGroup("Excel")]
        [SerializeField] protected Object excel;

        [TitleGroup("Lookup")]
        [PropertySpace(0, 10)][AutoGetOrCreateSO][SerializeField] protected LookUpSO lookup;


        [FoldoutGroup("Columns")][SerializeField] protected int tagsCol = 20;
        [FoldoutGroup("Columns")][SerializeField] protected int additionalsCol = 31;


        #region Open e.g.

        //[MenuItem("MAD/Levels/Excel Reader", false, 10000)]
        //public static void Open()
        //{
        //    var obj = MADUtility.GetOrCreateSOAtEssentialsFolder<LevelExcelReader>();
        //    InspectWindow.ShowWindow(obj, "Excel Reader");
        //    obj.Initialize();          
        //} 


        #endregion


        public virtual void Initialize()
        {
            RefreshLevelSetsCollection();

            Selection.activeObject = GetActiveObject();
        }


        [PropertySpace(20)]
        [Button(ButtonSizes.Large)]
        public virtual void Generate()
        {
            MADUtility.ClearConsole();

            if (excel == null)
            {
                Debug.LogError("Excel not set!");
                return;
            }

            string originalPath = GetExcelPath();
            string tempPath = Path.Combine(Path.GetTempPath(), "read excel temp.xlsx");
            File.Copy(originalPath, tempPath, overwrite: true);

            using FileStream stream = File.Open(tempPath, FileMode.Open, FileAccess.Read);
            using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet();

            PreGenerate(dataSet);

            foreach (DataTable table in dataSet.Tables)
            {
                var levelSet = GetOrCreateLevelSet(table);
                if (levelSet == null) continue;

                int rowsCount = table.Rows.Count;
                var columsCount = table.Columns.Count;
                var levelSetSOPath = levelSet.GetPath();

                string currentLvlIdentifier = null;
                var levelInt = 0;
                var stageInt = 1;

                TLevel level = null;

                for (int i = 1; i < rowsCount; i++)
                {
                    var cell = table.Rows[i].TryGetStringAtColumn(0);

                    if (string.IsNullOrEmpty(cell)) continue;

                    var rowIndex = i;
                    var dataRow = table.Rows[i];

                    GetLevelAndStageInt(cell, ref currentLvlIdentifier, ref levelInt, ref stageInt, out bool levelChanged);

                    if (levelChanged)
                    {
                        level = GetOrCreateLevel(levelSet, levelInt);
                        levelSet.AddLevelRegular(level);
                    }

                    TrySetLoop(dataRow, levelSet, levelInt);
                    TrySetNotRepeatable(dataRow, level, levelInt);

                    var stage = GetOrCreateStage(levelSet, level, stageInt);
                    level.AddStageRegular(stage);

                    var fromRowIndex = rowIndex + 1;
                    var toRowIndex = GetToRowIndex(table, fromRowIndex);

                    ParseHeader(dataRow, levelSet, level, stage, levelInt, stageInt);
                    ParseContent(table, fromRowIndex, toRowIndex, levelSet, level, stage, levelInt, stageInt);


                    EditorUtility.SetDirty(level);
                    EditorUtility.SetDirty(stage);
                }

                EditorUtility.SetDirty(levelSet);
                AssetDatabase.SaveAssets();

                RemoveExcess(levelSet);
            }

            PostGenerate(dataSet);

            EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }




        #region Abstracts
        public abstract Object GetActiveObject();
        #endregion


        #region Virtuals
        protected virtual void PreGenerate(DataSet dataSet) { }
        protected virtual void PostGenerate(DataSet dataSet) { }
        protected virtual void OnLevelSetCreated(TLevelSet levelSet)
        {
            levelSet.ClearLevels();
        }
        protected virtual void OnLevelCreated(TLevelSet levelSet, TLevel level, int levelInt)
        {
            level.ClearStages();
            level.SetIsRepeatable(true);
        }
        protected virtual void OnStageCreated(TLevelSet levelSet, TLevel level, TStage stage, int stageInt)
        {
            stage.SetAdditionals(null);
        }

        protected virtual void ParseHeader(DataRow dataRow,
            TLevelSet levelSet, TLevel level, TStage stage,
            int levelInt, int stageInt)
        {
            stage.SetAdditionals(GetAdditionals(dataRow));

        }
        protected virtual void ParseContent(DataTable table,
            int fromRowIndex, int toRowIndex,
            TLevelSet levelSet, TLevel level, TStage stage,
            int levelInt, int stageInt)
        {

        }

        #endregion


        void RefreshLevelSetsCollection() => levelSetsCollection.Retrieve();

        void TrySetLoop(DataRow row, TLevelSet levelSet, int levelInt)
        {
            var cell = row.TryGetStringAtColumn(tagsCol);
            if (string.IsNullOrEmpty(cell)) return;
            if (cell.ContainsIgnoreCase(k_LoopTag)) levelSet.SetLoop(levelInt);
        }
        void TrySetNotRepeatable(DataRow row, TLevel level, int levelInt)
        {
            var cell = row.TryGetStringAtColumn(tagsCol);
            if (string.IsNullOrEmpty(cell)) return;
            if (cell.ContainsIgnoreCase(k_NotRepatableTag)) level.SetIsRepeatable(false);
        }

        void RemoveExcess(TLevelSet levelSet)
        {
            var allLevels = MADUtility.GetAllInstances_Editor<TLevel>(inThisObjectsDirectory: levelSet).ToList();
            var allStages = MADUtility.GetAllInstances_Editor<TStage>(inThisObjectsDirectory: levelSet).ToList();

            var activeLevels = levelSet.GetLevels();
            var activeStages = activeLevels.SelectMany(level => level.GetStages()).ToList();

            var removesLevels = allLevels.Except(activeLevels).ToList();
            var removeStages = allStages.Except(activeStages).ToList();

            for (var i = removesLevels.Count - 1; i >= 0; i--)
            {
                var item = removesLevels[i];
                if (item == null) continue;
                var path = AssetDatabase.GetAssetPath(item);
                AssetDatabase.DeleteAsset(path);
            }

            for (var i = removeStages.Count - 1; i >= 0; i--)
            {
                var item = removeStages[i];
                if (item == null) continue;
                var path = AssetDatabase.GetAssetPath(item);
                AssetDatabase.DeleteAsset(path);
            }
        }



        #region Getters

        string GetExcelPath() => AssetDatabase.GetAssetPath(excel);


        public int? TryGetIntAt(DataRow row, int colIndex)
        {
            var cell = row.TryGetStringAtColumn(colIndex);
            if (string.IsNullOrEmpty(cell)) return null;
            if (int.TryParse(cell, out int parsed)) return parsed;
            return null;
        }

        TLevelSet GetOrCreateLevelSet(DataTable table)
        {
            var tableName = table.ToString();

            if (!string.IsNullOrEmpty(tableName) && tableName.ContainsIgnoreCase(k_LevelSetTag))
            {
                var levelset = tableName.Replace(k_LevelSetTag, "");
                var levelSetPath = LevelConsts.GetAndEnsureLocalPath(levelset);

                TLevelSet levelSet = MADUtility.GetOrCreateSONew<TLevelSet>(levelset, levelSetPath);
                levelSet.SetID = levelset;
                OnLevelSetCreated(levelSet);

                return levelSet;
            }
            return null;
        }
        TLevel GetOrCreateLevel(TLevelSet levelSet, int levelInt)
        {
            var levelSetPath = LevelConsts.GetAndEnsureLocalPath(levelSet.UID);
            var levelUID = levelSet.GetLevelUID(levelInt);

            TLevel level = MADUtility.GetOrCreateSONew<TLevel>(levelUID, levelSetPath);
            OnLevelCreated(levelSet, level, levelInt);

            return level;
        }
        TStage GetOrCreateStage(TLevelSet levelSet, TLevel level, int stageInt)
        {
            var levelSetPath = LevelConsts.GetAndEnsureLocalPath(levelSet.UID);
            var stageUID = level.GetStageUID(stageInt);

            TStage stage = MADUtility.GetOrCreateSONew<TStage>(stageUID, levelSetPath);
            OnStageCreated(levelSet, level, stage, stageInt);

            return stage;
        }

        void GetLevelAndStageInt(string cell, ref string currentLevelId, ref int levelInt, ref int stageInt, out bool levelChanged)
        {
            levelChanged = false;
            var cellSplit = cell.Split('_');

            if (cellSplit.Length > 1)
            {
                var lvlIdentifier = cellSplit[0];

                if (lvlIdentifier.Equals(currentLevelId, StringComparison.OrdinalIgnoreCase))
                {
                    stageInt++;
                }
                else
                {
                    currentLevelId = lvlIdentifier;
                    levelInt++;
                    stageInt = 1;
                    levelChanged = true;
                }
            }
            else
            {
                currentLevelId = null;
                levelInt++;
                stageInt = 1;
                levelChanged = true;
            }
        }
        int GetToRowIndex(DataTable table, int fromRowIndex)
        {
            int rowsCount = table.Rows.Count;
            int toRowIndex = fromRowIndex;
            for (int r = fromRowIndex; r < rowsCount; r++)
            {
                toRowIndex = r;
                var cell = table.Rows[r].TryGetStringAtColumn(0);
                if (!string.IsNullOrEmpty(cell)) break;
            }
            return toRowIndex;
        }



        public List<GameObject> GetAdditionals(DataRow dataRow)
        {
            List<GameObject> res = null;
            var cell = dataRow.TryGetStringAtColumn(additionalsCol);
            if (string.IsNullOrEmpty(cell)) return res;
            var split = cell.Split(',');

            foreach (var item in split)
            {
                if (string.IsNullOrEmpty(item)) continue;
                var go = item.FindPrefabWithName<GameObject>();

                if (go == null)
                {
                    var rowIndex = dataRow.GetRowIndex();
                    LogError($"[{item}] couldnt be found at row [{rowIndex + 1}]", dataRow.Table);                  
                }
                else
                {
                    if (res == null) res = new List<GameObject>();
                    res.Add(go);
                }
            }
            return res;
        }


        #endregion


        public void LogError(string message, DataTable table)
        {
            this.LogError($"{message} | {table}");
        }


    }


}
