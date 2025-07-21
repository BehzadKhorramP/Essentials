using BEH.Common;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Threading;
using UnityEngine;

namespace MadApper.Levels
{
    [Serializable]
    public class LevelSetSettings
    {
        public string SetID = ISetBased.k_Control;
    }

    public abstract class LevelsSystem<TLevelSetsCollection, TLevelSet, TLevel, TStage> : MonoBehaviour
       where TLevelSetsCollection : LevelSetsCollection<TLevelSet, TLevel, TStage>
       where TLevelSet : LevelSet<TLevel, TStage>
       where TLevel : Level<TStage>
       where TStage : Stage
    {
        [SerializeField][AutoGetOrCreateSO][ReadOnly] protected LevelSetSettingsSO levelSetSettingsSO;
        [ShowInInspector][ReadOnly] static QBool s_OngoingTasks = new();
        protected abstract Helper helper { get; }
        public abstract ILevelsDatabase i_LevelDatabase { get; }
        public abstract ILevelWinChecker i_LevelWinChecker { get; set; }
        public abstract ILevelLoseChecker i_LevelLoseChecker { get; set; }

        protected static LevelArgs? s_LevelArgs;

        protected CancellationTokenSource cts;
        protected CancellationTokenSource checkersCts;


        public static Action<LevelArgs> s_OnStageInitialized;
        public static Action<LevelArgs> s_OnStageWon;
        public static Action<LevelArgs> s_OnStageLost;
        public static Action<LevelArgs> s_OnRevivied;


        #region XX Important | Implement this :

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        //static void InitializeOnLoad()
        //{
        //    InitializeOnLoad(database)
        //} 

        #endregion

        protected static void InitializeOnLoad(ILevelsDatabase database)
        {
            var lastWonStage = database.GetLastWonStage();
            database.SetCurrentStage(lastWonStage + 1);
        }




        private void Start()
        {
            new SceneChangeSubscriber.Builder()
                .SetOnSceneActivatedInitialize(SceneInitialized)
                .SetOnSceneToBeChangedReset(SceneReset)
                .AddGameObjectScene(gameObject)
                .Build();
        }


        protected void SceneInitialized(string scene)
        {
            Stop();
            cts = new CancellationTokenSource();

            try
            {
                Execute().AttachExternalCancellation(cts.Token);
            }
            catch (Exception) { }


            async UniTask Execute()
            {
                s_LevelArgs = null;

                PreInitialize();

                await UniTask.WaitForSeconds(.1f, cancellationToken: cts.Token);

                s_OngoingTasks = new QBool();

                var data = await helper.z_Initialize(system: this, cToken: cts.Token);
                if (!data.HasValue)
                {
                    this.LogError("initialize-couldnt fetch levelArgs!");
                    return;
                }

                var args = data.Value;
                s_LevelArgs = args;

                var stage = args.Stage;
                stage.ResetScene(scene);

                Prepare(stage);

                stage.Initialize(scene);

                s_OnStageInitialized?.Invoke(args);

                stage.OnAdditionals();
            }
        }
        protected virtual void SceneReset(string scene)
        {
            Stop();
            StopCheckers();

            if (s_LevelArgs.HasValue)
            {
                var args = s_LevelArgs.Value;
                args.Stage?.ResetScene(scene);
            }
        }

        protected void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }

            cts = null;
        }
        protected void StopCheckers()
        {
            i_LevelWinChecker?.Stop();
            i_LevelLoseChecker?.Stop();

            if (checkersCts != null)
            {
                checkersCts.Cancel();
                checkersCts.Dispose();
            }
            checkersCts = null;

        }

        #region Abstracts      
        protected abstract void PreInitialize();
        protected abstract void Prepare(TStage stage);

        #endregion



        #region Virtuals
        protected virtual void CheckWinLose()
        {
            StopCheckers();

            checkersCts = new CancellationTokenSource();

            try
            {
                Run().AttachExternalCancellation(cancellationToken: checkersCts.Token);
            }
            catch (Exception) { }


            async UniTask Run()
            {
                await UniTask.Delay(1, cancellationToken: checkersCts.Token);
                await i_LevelWinChecker.Check();
                await UniTask.Delay(1, cancellationToken: checkersCts.Token);
                await i_LevelLoseChecker.Check();
            }
        }

        public virtual async void OnWon()
        {
            StopCheckers();
            i_LevelLoseChecker.Kill();

            this.LogGreen("won");

            if (!s_LevelArgs.HasValue)
            {
                this.LogWarning("won-doesnt have levelArgs!");

                var data = await helper.z_Initialize(system: this, cToken: cts.Token);

                if (!data.HasValue)
                {
                    this.LogError("won-couldnt fetch levelArgs!");
                    return;
                }

                s_LevelArgs = data.Value;
            }

            var args = s_LevelArgs.Value;

            s_OnStageWon?.Invoke(args);

            IncrementStage();
        }
        public virtual void OnLost()
        {
            StopCheckers();
            i_LevelLoseChecker.Kill();

            this.LogRed("lost");

            s_OnStageLost?.Invoke(s_LevelArgs.Value);
        }


        public virtual void TryRevive()
        {
            if (i_LevelWinChecker.HasWon) return;
            if (!i_LevelLoseChecker.IsKilled) return;

            this.LogBlue("revivied");

            i_LevelLoseChecker.Revive();
            s_OnRevivied?.Invoke(s_LevelArgs.Value);
        }

        #endregion

        #region Getters/Setters

        public static LevelArgs? GetCurrentLevelArgs() => s_LevelArgs;

        public static void AddTask(string tag) => s_OngoingTasks.Lock(tag);
        public static void RemoveTask(string tag) => s_OngoingTasks.Unlock(tag);
        public static QBool GetTasks() => s_OngoingTasks;


        public void IncrementStage()
        {
            var args = s_LevelArgs.Value;
            var stageInt = args.StageInt;
            var levelInt = args.LevelInt;

            i_LevelDatabase.SetLastWonStage(stageInt);

            stageInt++;

            var hasMoreStages = args.HasMoreStagesAfter();

            if (!hasMoreStages)
            {
                i_LevelDatabase.SetLastWonLevel(levelInt);

                stageInt = 1;
                levelInt++;

                i_LevelDatabase.SetCurrentLevel(levelInt);
                i_LevelDatabase.SetLastWonStage(0);
            }

            i_LevelDatabase.SetCurrentStage(stageInt);
        }


        #endregion



        #region Helper       

        [Serializable]
        public abstract class Helper
        {
            [SerializeField][AutoGetOrCreateSO][ReadOnly] protected LevelSetSettingsSO levelSetSettingsSO;
            [SerializeField][AutoGetOrCreateSO][ReadOnly] protected TLevelSetsCollection levelSetsCollection;

            [FoldoutGroup("First Level")][SerializeField] UnityEventDelayList onFirstLevel;
            [FoldoutGroup("First Level")][SerializeField] UnityEventDelayList onNotFirstLevel;

            public abstract ILevelsDatabase i_LevelDatabase { get; }

            public async UniTask<string> GetSetID(CancellationToken cToken)
            {
                await UniTask.Delay(1, cancellationToken: cToken);

                return levelSetSettingsSO.Value.SetID;
            }

            public async UniTask<LevelArgs?> z_Initialize(LevelsSystem<TLevelSetsCollection, TLevelSet, TLevel, TStage> system, CancellationToken cToken)
            {
                var setID = await GetSetID(cToken);
                var levelSet = levelSetsCollection.GetLevelSet(setID);

                if (levelSet == null) return null;

                var levelInt = i_LevelDatabase.GetCurrentLevel();
                var stageInt = i_LevelDatabase.GetCurrentStage();

                var level = levelSet.GetLevel(levelInt);

                if (level == null) return null;

                var higherExists = level.IsHigherThanStagesExist(stageInt);

                if (higherExists)
                {
                    levelInt++;
                    stageInt = 1;

                    i_LevelDatabase.SetCurrentStage(stageInt);
                    i_LevelDatabase.SetCurrentLevel(levelInt);
                    i_LevelDatabase.SetLastWonStage(0);

                    level = levelSet.GetLevel(levelInt);
                }

                var stage = level.GetStage(stageInt);

                if (stage == null) return null;

                var lastWonLevel = i_LevelDatabase.GetLastWonLevel();
                var lastWonStage = i_LevelDatabase.GetLastWonStage();


                var data = new LevelArgs()
                {
                    LevelInt = levelInt,
                    StageInt = stageInt,
                    LastWonLevelInt = lastWonLevel,
                    LastWonStageInt = lastWonStage,
                    LevelSet = levelSet,
                    Level = level,
                    Stage = stage,
                    LevelsSystem = system
                };

                return data;
            }
            public void z_TryGoToFirstLevelDirectly()
            {
                var currentLevel = i_LevelDatabase.GetCurrentLevel();

                if (currentLevel != 1)
                {
                    onNotFirstLevel?.Invoke();
                    return;
                }
                onFirstLevel?.Invoke();
            }
        }

        #endregion


        #region Listener

        [Serializable]
        public abstract class Listener
        {
            public virtual void OnEnable()
            {
                s_OnStageInitialized += OnStageInitialized;
                s_OnStageWon += OnStageWon;
                s_OnStageLost += OnStageLost;
                s_OnRevivied += OnRevived;
            }
            public virtual void OnDisable()
            {
                s_OnStageInitialized -= OnStageInitialized;
                s_OnStageWon -= OnStageWon;
                s_OnStageLost -= OnStageLost;
                s_OnRevivied -= OnRevived;
            }


            public Action<LevelArgs> onStageInitializedCallback;
            [SerializeField] protected UnityEventDelayList<LevelArgs> onStageInitialized;
            protected virtual void OnStageInitialized(LevelArgs args)
            {
                onStageInitializedCallback?.Invoke(args);
                onStageInitialized?.Invoke(args);
            }


            public Action<LevelArgs> onStageWonCallback;
            [SerializeField] protected UnityEventDelayList<LevelArgs> onStageWon;
            protected virtual void OnStageWon(LevelArgs args)
            {
                onStageWonCallback?.Invoke(args);
                onStageWon?.Invoke(args);
            }


            public Action<LevelArgs> onStageLostCallback;
            [SerializeField] protected UnityEventDelayList<LevelArgs> onStageLost;
            protected virtual void OnStageLost(LevelArgs args)
            {
                onStageLostCallback?.Invoke(args);
                onStageLost?.Invoke(args);
            }


            public Action<LevelArgs> onRevivedCallback;
            [SerializeField] protected UnityEventDelayList<LevelArgs> onRevived;
            private void OnRevived(LevelArgs args)
            {
                onRevivedCallback?.Invoke(args);
                onRevived?.Invoke(args);
            }
        }

        #endregion



        public struct LevelArgs
        {
            public int LevelInt;
            public int StageInt;
            public int LastWonLevelInt;
            public int LastWonStageInt;
            public TLevelSet LevelSet;
            public TLevel Level;
            public TStage Stage;

            public LevelsSystem<TLevelSetsCollection, TLevelSet, TLevel, TStage> LevelsSystem;

            public bool HasMoreStagesAfter()
            {
                return Level.HasMoreStagesAfter(StageInt + 1);
            }
        }
    }








}
