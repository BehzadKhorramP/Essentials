using Cysharp.Threading.Tasks;
using MadApper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BEH.Objective
{
    public class ObjectiveSystemCollection : StaticCollection<ObjectiveSystem>
    {
        public static int s_OngoingTask;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnInitializeOnLoad()
        {
            OnInitializeOnLoadImpl(onSceneChanged: (scene) =>
            {
                s_OngoingTask = 0;
            });
        }
    }

    public class ObjectiveSystem : MonoBehaviour
    {
        const string k_ObjectiveTag = "Objective";

        [Space(10)]
        public int m_SystemPriority;

        [Space(10)]
        [SerializeField]
        ObjectiveSystemUI m_UI;
        [Space(10)]
        [SerializeField]
        VFX m_VFX;
        [Space(10)]
        [SerializeField]
        Courier.Params m_CourierParams;
        [Space(10)]
        [SerializeField]
        List<Need> m_Need;

        public ObjectiveNeedsCollection m_NeedsCollection;

        private ObjectiveNeedsCollection.Events? extraEvents;

        Camera cam;
        Camera m_Camera
        {
            get
            {
                if (cam == null)
                    cam = Camera.main;
                return cam;
            }
        }

        private void Start()
        {
            new SceneChangeSubscriber.Builder()
                .SetOnSceneToBeChangedReset(Reset)
                .AddGameObjectScene(gameObject)
                .Build();
        }

        // for manual testings
        public void z_InitManual() =>
            Init(m_Need);

        // it is called from OnSceneActivatedInitialize of LevelSystem
        public void Init(List<Need> needs)
        {
            if (needs == null || needs.Count == 0)
                return;

            ObjectiveSystemCollection.Add(this);
            ObjectiveSystemCollection.s_Collection = ObjectiveSystemCollection.s_Collection.OrderByDescending(x => x.m_SystemPriority).ToList();

            var events = new ObjectiveNeedsCollection.Events
            {
                onNeedUpdated = OnNeedUpdated,
                onNeedCompleted = OnNeedCompleted,
                onFinished = OnNeedsFinished
            };

            m_NeedsCollection = new ObjectiveNeedsCollection(needs: needs, events: events);

            if (m_UI != null)
                m_UI.Init(this);
        }

        public void SetExtraEvents(ObjectiveNeedsCollection.Events events) =>
            extraEvents = events;
        private void Reset(string obj)
        {
            if (m_UI != null)
                m_UI.OnReset();

            ObjectiveSystemCollection.Remove(this);

            m_NeedsCollection = null;
        }

        public bool IsFinished()
        {
            if (m_NeedsCollection == null)
                return false;

            return m_NeedsCollection.IsFinished;
        }
        public bool IsPendingFinished()
        {
            if (m_NeedsCollection == null)
                return false;

            return m_NeedsCollection.IsPendingFinished;
        }

        #region CollectorEvents
        private void OnNeedUpdated(string id, int remaining)
        {
            if (m_UI != null)
                m_UI.OnUpdateUI(id, remaining);

            if (extraEvents.HasValue)
                extraEvents.Value.onNeedUpdated?.Invoke(id, remaining);
        }
        private void OnNeedCompleted(string id)
        {
            if (extraEvents.HasValue)
                extraEvents.Value.onNeedCompleted?.Invoke(id);
        }
        private void OnNeedsFinished()
        {
            if (extraEvents.HasValue)
                extraEvents.Value.onFinished?.Invoke();
        }
        #endregion

        public bool IsNeeded_TempCollect(IObjectivable iObjective, int count) =>
            m_NeedsCollection != null && m_NeedsCollection.IsNeeded_TempCollect(iObjective.ID, count);
        public bool IsNeeded_Simple(IObjectivable iObjective) =>
            m_NeedsCollection != null && m_NeedsCollection.IsNeeded_Simple(iObjective.ID);

        public static ObjectiveSystem GetIObjectiveIsNeededSystem(IObjectivable iObjective, int count)
        {
            if (ObjectiveSystemCollection.s_Collection == null)
                return null;

            foreach (var item in ObjectiveSystemCollection.s_Collection)
            {
                var isNeeded = item.IsNeeded_TempCollect(iObjective, count);

                if (isNeeded)
                    return item;
            }

            return null;
        }

        public static bool IsIObjectiveNeeded(IObjectivable iObjective)
        {
            if (ObjectiveSystemCollection.s_Collection == null)
                return false;

            foreach (var item in ObjectiveSystemCollection.s_Collection)
            {
                var isNeeded = item.IsNeeded_Simple(iObjective);

                if (isNeeded)
                    return item;
            }

            return false;
        }

        public static bool IsAllObjectivesFinished()
        {
            if (ObjectiveSystemCollection.s_Collection == null)
                return false;

            foreach (var item in ObjectiveSystemCollection.s_Collection)
                if (!item.IsFinished())
                    return false;

            return true;
        }
        public static bool IsAllObjectivesPendingFinished()
        {
            if (ObjectiveSystemCollection.s_Collection == null)
                return false;

            foreach (var item in ObjectiveSystemCollection.s_Collection)
                if (!item.IsPendingFinished())
                    return false;

            return true;
        }

        Transform GetObjectiveUI(string id)
        {
            if (m_UI == null)
                return null;

            var ui = m_UI.GetObjectiveUI(id);

            if (ui == null)
                return null;

            return ui.GetTarget();
        }

        public async void OnCollectIObjectivable(IObjectivable iObjectivable, Transform obj, int count, float delay = 0, Action onComplete = null)
        {
            var id = iObjectivable.ID;
            var need = m_NeedsCollection.GetNeededObjective(id, true, count);
            var objectiveUI = GetObjectiveUI(need.IObjectivable.ID);
            var movesToUI = iObjectivable.i_ObjectivableMovesToUI;

            if (need == null)
            {
                onComplete?.Invoke();
                return;
            }

            ObjectiveSystemCollection.s_OngoingTask++;

            if (!movesToUI || objectiveUI == null)
            {
                m_NeedsCollection.OnCollected(id, count);
                onComplete?.Invoke();
                ObjectiveSystemCollection.s_OngoingTask--;
                return;
            }

            if (delay > 0)
                await UniTask.WaitForSeconds(delay);


            var extras = obj.GetComponentsInChildren<IObjectivableExtra>();

            VFX vfx = null;
            VFX.Args vfxArgs = default;

            if (m_VFX != null)
            {
                vfx = PoolVFX.Get(id: $"{iObjectivable.ID}_{k_ObjectiveTag}", prefab: m_VFX);
                vfxArgs = new VFX.Args()
                {
                    Color = iObjectivable.GetColor(),
                    Delay = .25f
                };
            }

            Action<Courier> onStarted = (courier) =>
            {
                if (extras != null)
                    foreach (var item in extras)
                        item.OnObjectiveStartedMovingToUI();
            };

            Action<Courier> onEnded = (courier) =>
            {
                m_NeedsCollection.OnCollected(id, count);

                if (extras != null)
                    foreach (var item in extras)
                        item.OnObjectiveEndedMovingToUI();

                onComplete?.Invoke();
                ObjectiveSystemCollection.s_OngoingTask--;
            };

            var courierArgs = new Courier.Args
            {
                StartPosition = obj.position,
                EndPosition = objectiveUI.position,
                Parameters = m_CourierParams,
                Camera = m_Camera
            };

            courierArgs.Move(vfx: vfx, vfxArgs: vfxArgs, obj: obj, onStarted: onStarted, onEnded: onEnded);
        }
    }
}