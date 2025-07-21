using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MadApper.Levels
{
    public static class LevelConsts
    {
        public const string k_LevelPrefix = "Level";
        public const string k_StagePrefix = "_Stage";
        public const string k_RandomID = "X";
        public const string k_LevelsFolder = "_Levels";
        public const string k_LevelSets = "Level Sets";


#if UNITY_EDITOR

        #region Path
        public static string GetMainPath() => Path.Combine(MADUtility.GetEssentialsFolder(), k_LevelsFolder);
        public static string GetLevelSetsPath(string levelSetUID) => Path.Combine(GetMainPath(), levelSetUID);
        public static string GetLevelsPath(string levelSetUID, string levelUID) => Path.Combine(GetLevelSetsPath(levelSetUID), levelUID);
        public static string GetAndEnsureLocalPath(string levelSetUID)
        {
            GetMainPath().EnsureFolderExists();
            var levelSetPath = GetLevelSetsPath(levelSetUID);
            levelSetPath.EnsureFolderExists();
            return levelSetPath;
        }
        #endregion  
#endif
    }
    public abstract class LevelSetsCollection<TLevelSet, TLevel, TStage> : ScriptableObject
       where TLevelSet : LevelSet<TLevel, TStage>
       where TLevel : Level<TStage>
       where TStage : Stage
    {
        [Space(10)][SerializeField] List<TLevelSet> levelSets = new List<TLevelSet>();

#if UNITY_EDITOR
        public void Retrieve()
        {
            levelSets = MADUtility.GetAllInstances_Editor<TLevelSet>().ToList();
            this.TrySetDirty();
        }
#endif


        public List<TLevelSet> GetLevelSets() => levelSets;
        public TLevelSet GetLevelSet(string setID) => levelSets.GetValueOfSetBasedData(setID);
    }



    public abstract class LevelSet<TLevel, TStage> : ScriptableObject, ICreatableSO, ISetBased
       where TLevel : Level<TStage>
       where TStage : Stage
    {

        [SerializeField] protected string uId;
        [SerializeField] protected string setId;
        [SerializeField] protected int loop = 1;
        [SerializeField] protected List<TLevel> levels = new List<TLevel>();
        [SerializeField, HideInInspector] protected bool isDirty;

        public string UID { get => uId; set => uId = value; }
        public string SetID { get => setId; set => setId = value; }

        [NonSerialized] HashSet<int> _excludeRepeat = null;
        HashSet<int> excludeRepeat
        {
            get
            {
                if (_excludeRepeat == null || _excludeRepeat.Count == 0) RebuildExcludeRepeat();
                return _excludeRepeat;
            }
        }
        void RebuildExcludeRepeat()
        {
            _excludeRepeat = new HashSet<int>();
            for (int i = 0; i < levels.Count; i++)
                if (!levels[i].IsRepeatable())
                    _excludeRepeat.Add(i);
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            RebuildExcludeRepeat();
        }


        // just to make it register a change to save and version control!
        public void SwitchIsDirty() => isDirty = !isDirty;

        public void SetLoop(int loop)
        {
            this.loop = loop;
            this.TrySetDirty();
        }

        [Button]
        public void Retrieve()
        {
            levels = MADUtility.GetAllInstances_Editor<TLevel>(inThisObjectsDirectory: this).ToList();
        }

        public void Refresh()
        {
            RefreshNulls();
            Reorder();
            RebuildExcludeRepeat();
        }
        public void RefreshNulls()
        {
            if (levels == null) levels = new List<TLevel>();
            for (int i = levels.Count - 1; i >= 0; i--)
            {
                var pack = levels[i];
                if (pack == null) levels.Remove(pack);
            }
            this.TrySetDirty();
        }

        public void Reorder()
        {
            if (levels == null || !levels.Any())
                return;

            levels.SortByTrailingNumber(x => x.UID);
            this.TrySetDirty();
        }

        [Button]
        public void TouchAllMeta()
        {
            foreach (var level in levels)
            {
                foreach (var stage in level.GetStages())
                    stage.TouchMeta();

                level.TouchMeta();
            }
            this.TouchMeta();
        }

        public void RenameByIndex()
        {
            if (levels == null || !levels.Any())
                return;

            var itemPrefix = $"{SetID}_{LevelConsts.k_LevelPrefix}";

            levels.RenameItemsAndSubItems((pack) => pack.GetStages(),
                onItemNameChanged: (level, newID) =>
                {
                    level.UID = newID;
                    level.SwitchIsDirty();
                    level.TrySetDirty();
                },
                onSubItemNameChanged: (stage, newID) =>
                {
                    stage.UID = newID;
                    stage.SwitchIsDirty();
                    stage.TrySetDirty();
                },
                itemPrefix: itemPrefix, subItemPrefix: LevelConsts.k_StagePrefix);

            this.SwitchIsDirty();
            this.TrySetDirty();
        }
        public void AddLevel(TLevel level)
        {
            Refresh();

            if (levels.Any(x => x.UID == level.UID)) return;
            levels.Add(level);
            this.TrySetDirty();
        }
        public void AddLevelRegular(TLevel level)
        {
            levels.Add(level);
            this.TrySetDirty();
        }

        public string GetNewLevelUID()
        {
            var index = levels.Count + 1;
            var indexedID = $"{LevelConsts.k_LevelPrefix} {index}";
            var levelUID = $"{UID}_{indexedID}";

            return levelUID;
        }
        public string GetLevelUID(int levelInt)
        {
            var indexedID = $"{LevelConsts.k_LevelPrefix} {levelInt}";
            var levelUID = $"{UID}_{indexedID}";

            return levelUID;
        }
        public void ClearLevels() => levels = new List<TLevel>();

        //[Button]
        //public void TestGetLevel(int to)
        //{
        //    for (int i = 1; i < to; i++)
        //    {
        //        var level = GetLevel(i);
        //        level.Log($"{levels.IndexOf(level) + 1} as {i}");
        //    }

        //}
#endif

        public List<TLevel> GetLevels() => levels;
        public TLevel GetLevel(int level)
            => levels.GetItemWhileLooped
            (index: level - 1,
            loopPoint: loop,
            excludedIndexes: excludeRepeat);



    }




    public abstract class Level<TStage> : ScriptableObject, ICreatableSO where TStage : Stage
    {
        [SerializeField] protected string uId;
        [SerializeField] protected List<TStage> stages = new List<TStage>();
        [SerializeField, HideInInspector] protected bool isDirty;
        [SerializeField] protected bool isRepeatable = true;

        public string UID { get => uId; set => uId = value; }

#if UNITY_EDITOR     

        // just to make it register a change to save and version control!
        public void SwitchIsDirty() => isDirty = !isDirty;
        public void SetIsRepeatable(bool isRepeatable)
        {
            this.isRepeatable = isRepeatable;
            this.TrySetDirty();
        }
        public void Refresh()
        {
            RefreshNulls();
            Reorder();
        }
        public void RefreshNulls()
        {
            if (stages == null) stages = new List<TStage>();
            for (int i = stages.Count - 1; i >= 0; i--)
            {
                var pack = stages[i];
                if (pack == null) stages.Remove(pack);
            }
            this.TrySetDirty();
        }

        public void Reorder()
        {
            if (stages == null || !stages.Any())
                return;

            stages.SortByTrailingNumber(x => x.UID);
            this.TrySetDirty();
        }
        public void RenameByIndex()
        {
            if (stages == null || !stages.Any())
                return;

            var prefix = $"{uId}{LevelConsts.k_StagePrefix}";

            stages.RenameItemsByIndex(prefix, onItemNameChanged: (stage, newID) =>
            {
                stage.UID = newID;
                stage.SwitchIsDirty();
                stage.TrySetDirty();
            });

            foreach (var stage in stages)
            {
                stage.TouchMeta();
            }

            this.SwitchIsDirty();
            this.TouchMeta();
            this.TrySetDirty();
        }



        public void AddStage(TStage stage)
        {
            Refresh();

            if (stages.Any(x => x.UID == stage.UID)) return;
            stages.Add(stage);
            this.TrySetDirty();
        }
        public void AddStageRegular(TStage stage)
        {
            stages.Add(stage);
            this.TrySetDirty();
        }
        public string GetNewStageUID()
        {
            var index = stages.Count + 1;
            var indexedID = $"{LevelConsts.k_StagePrefix} {index}";
            var stageUID = $"{UID}{indexedID}";

            return stageUID;
        }
        public string GetStageUID(int stageInt)
        {
            var indexedID = $"{LevelConsts.k_StagePrefix} {stageInt}";
            var stageUID = $"{UID}{indexedID}";

            return stageUID;
        }
        public void ClearStages() => stages = new List<TStage>();
#endif


        public bool IsRepeatable() => isRepeatable;
        public List<TStage> GetStages() => stages;
        public TStage GetStage(int stage)
        {
            if (stages == null || !stages.Any()) return null;

            var index = stage - 1;
            if (index >= stages.Count) return stages[^1];
            return stages[index];
        }

        public bool HasMoreStagesAfter(int value) => value - 1 < stages.Count;
        public bool IsHigherThanStagesExist(int value) => value - 1 >= stages.Count;
    }



    public abstract class Stage : ScriptableObject, ICreatableSO
    {
        public enum Difficulty { Normal, Hard, SuperHard }

        [SerializeField] protected string uId;
        [SerializeField] protected Difficulty difficulty;
        [SerializeField] protected List<GameObject> additionals;
        [SerializeField, HideInInspector] protected bool isDirty;

        public string UID { get => uId; set => uId = value; }
        List<GameObject> instancedAdditionals { get; set; }

#if UNITY_EDITOR

        public virtual void Refresh() { }

        // just to make it register a change to save and version control!
        public void SwitchIsDirty() => isDirty = !isDirty;

        public void SetAdditionals(List<GameObject> additionals)
        {
            if (additionals == null)
            {
                this.additionals = null;
                return;
            }

            this.additionals = new List<GameObject>(additionals);
        }

#endif

        public virtual void Initialize(string scene)
        {

        }
        public virtual void ResetScene(string scene)
        {
            if (instancedAdditionals != null)
            {
                for (int i = instancedAdditionals.Count - 1; i >= 0; i--)
                {
                    var item = instancedAdditionals[i];
                    if (item == null) continue;
                    Destroy(item.gameObject);
                }
                instancedAdditionals = null;
            }
        }
        public virtual void OnAdditionals()
        {
            if (additionals == null || !additionals.Any()) return;
            instancedAdditionals = new List<GameObject>();
            foreach (var item in additionals) instancedAdditionals.Add(Instantiate(item));
        }

    }





}
