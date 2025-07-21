using Cysharp.Threading.Tasks;
using DG.Tweening;
using MadApper;
using MadApper.Singleton;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BEH.Reward
{
    public class RewardSystem : PersistentSingleton<RewardSystem>, IActiveableSystem
    {
        const string k_PrefabName = "RewardSystem";

        public string i_PrefabName => k_PrefabName;
        public enum Type { Layout, ScreenSpace }

        [SerializeField][AutoGetInChildren][ReadOnly] Canvas m_CanvasRect;
        [Space(10)] public Transform m_Blocker;

        [FoldoutGroup("Parents")]
        [SerializeField] RectTransform m_LayOutParent;
        [FoldoutGroup("Parents")]
        [SerializeField] RectTransform m_ScreenParent;
        [FoldoutGroup("Parents")]
        [SerializeField] RectTransform m_TempParent;

        [FoldoutGroup("Appear Events")]
        [SerializeField] UnityEventDelayList m_OnAppear;
        [FoldoutGroup("Appear Events")]
        [SerializeField] UnityEventDelayList m_OnDisAppear;
        [FoldoutGroup("Appear Events")]
        [SerializeField] UnityEventDelayList m_OnBlackAppear;
        [FoldoutGroup("Appear Events")]
        [SerializeField] UnityEventDelayList m_OnBlackDisAppear;

        [FoldoutGroup("Action Events")][SerializeField] UnityEventDelayList m_OnModifyLayout;
        [FoldoutGroup("Action Events")][SerializeField] UnityEventDelayList m_CloseShopAction;
        [FoldoutGroup("Action Events")][SerializeField] UnityEventDelayList m_RewardLayoutOpened;
        [FoldoutGroup("Action Events")][SerializeField] UnityEventDelayList m_RewardLayoutClosed;

        [Space(10)] public UnityEventDelay m_TapToClaim;

        [Space(10)] public List<UnityEventDelay> m_OnRewardToOpen;

        List<Sequence> moveSQs = new();
        List<DuplicatedData> duplicatedRewards;
        List<Action> onAllRewardsClaimed;

        CancellationTokenSource cts;

        static Action<RewardsToClaimData> onPutRewardsToLayout;
        static Action<IEnumerable<RewardsToClaimData>> onPutCollectionOfRewardsToLayout;
        static Action<RewardsWorldSpaceData> onPutRewardToScreenPos;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            if (!IActiveableSystem.IsActive(k_PrefabName))
                return;


            MADUtility.TryLoadAndInstantiate<RewardSystem>(k_PrefabName);
        }
        private void OnEnable()
        {
            onPutRewardsToLayout += PutRewardsToLayout;
            onPutCollectionOfRewardsToLayout += PutCollectionOfRewardsToLayout;
            onPutRewardToScreenPos += PutRewardToScreenPos;
        }
        private void OnDisable()
        {
            onPutRewardsToLayout -= PutRewardsToLayout;
            onPutCollectionOfRewardsToLayout -= PutCollectionOfRewardsToLayout;
            onPutRewardToScreenPos -= PutRewardToScreenPos;
        }


        protected override void Awake()
        {
            base.Awake();

            m_Blocker.gameObject.SetActive(false);
        }

        protected override void OnDestroy()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }

            KillMoveSequences();

            base.OnDestroy();
        }




        public static void OnPutRewardsToLayout(RewardsToClaimData data)
        {
            onPutRewardsToLayout?.Invoke(data);
        }
        void PutRewardsToLayout(RewardsToClaimData data)
        {
            onAllRewardsClaimed = new List<Action>() { data.OnAllRewardsClaimed };

            var merger = new List<MergedData>();
            MergeDuplicates(data.Rewards, ref merger);
            PutRewardsToLayoutImpl(merger);
        }


        public static void OnPutCollectionOfRewardsToLayout(IEnumerable<RewardsToClaimData> data)
        {
            onPutCollectionOfRewardsToLayout?.Invoke(data);
        }
        private void PutCollectionOfRewardsToLayout(IEnumerable<RewardsToClaimData> listData)
        {
            onAllRewardsClaimed = new List<Action>();
            var merger = new List<MergedData>();

            foreach (var item in listData)
            {
                onAllRewardsClaimed.Add(item.OnAllRewardsClaimed);
                MergeDuplicates(item.Rewards, ref merger);
            }

            PutRewardsToLayoutImpl(merger);
        }


        async void PutRewardsToLayoutImpl(List<MergedData> merger)
        {
            PrepareLayout();

            await UniTask.DelayFrame(1);

            var sizesDict = new Dictionary<Rewardable, Vector2>();
            var rewardParents = new List<Transform>();
            var moveAmount = 20f;

            foreach (var item in merger)
            {
                GameObject rewardParentGo = new GameObject("Reward Parent", typeof(RectTransform));

                rewardParentGo.transform.SetParent(m_LayOutParent);
                rewardParentGo.transform.localScale = Vector3.zero;
                rewardParentGo.transform.localPosition = Vector3.zero;

                var dupReward = Instantiate(item.Rewardable, rewardParentGo.transform);
                dupReward.transform.localPosition = Vector3.zero;

                rewardParents.Add(rewardParentGo.transform);

                if (item.Rewardable.TryGetComponent(out RectTransform rectt))
                {
                    var size = rectt.rect.size;
                    moveAmount = size.x / 12f;
                    sizesDict.Add(dupReward, size);
                }

                dupReward.Prepare(item.Amount);

                duplicatedRewards.Add(new DuplicatedData()
                {
                    Rewardable = dupReward
                });
            }

            m_OnModifyLayout?.Invoke();

            await UniTask.DelayFrame(1);

            foreach (var item in sizesDict)
            {
                var dupRectSize = item.Key.transform.parent.GetComponent<RectTransform>().rect.size;
                var scale = dupRectSize.x / item.Value.x;
                item.Key.transform.localScale = Vector3.one * scale;
            }

            Appear();

            MakeRewardsMove(moveAmount);

            MakeRewardsScaleToOne(rewardParents, onComplete: () => { m_TapToClaim?.Invoke(); });

            m_CloseShopAction?.Invoke();
            m_RewardLayoutOpened?.Invoke();
        }

        void MergeDuplicates(IEnumerable<Rewardable> rewards, ref List<MergedData> merger)
        {
            foreach (var item in rewards)
            {
                var m = merger.Find(x => x.Rewardable.IsEqual(item));

                if (m == null)
                {
                    m = new MergedData() { Rewardable = item };

                    merger.Add(m);
                }
                m.Amount += item.m_RewardData.Amount;
            }
        }

        #region Old ResourceTrackingDup

        ////public IEnumerable<IResourceTracking> GetIResourceTrackingDuplicates(IEnumerable<IResourceTracking> originals)
        ////{
        ////    var dupTrackings = new List<IResourceTracking>();

        ////    if (originals != null)
        ////        foreach (var original in originals)
        ////        {
        ////            var isDestroyable = original.IsDestroyable;

        ////            // no need to duplicate tracking if it is not destroyable!
        ////            if (!isDestroyable)
        ////            {
        ////                dupTrackings.Add(original);
        ////                continue;
        ////            }

        ////            var go = new GameObject("IResourceTracking - Duplicate");

        ////            var type = original.GetType();
        ////            var copy = go.AddComponent(type);
        ////            original.CopyTo(copy);

        ////            if (copy != null)
        ////                dupTrackings.Add(copy as IResourceTracking);
        ////        }

        ////    return dupTrackings;
        ////} 
        #endregion

        async void MakeRewardsMove(float moveAmount)
        {
            foreach (var dItem in duplicatedRewards)
            {
                var item = dItem.Rewardable;
                var sq = DOTween.Sequence();

                sq.Append(item.transform.DOLocalMoveY(moveAmount, .5f).SetEase(Ease.Linear));
                sq.Append(DOVirtual.DelayedCall(.2f, null));
                sq.Append(item.transform.DOLocalMoveY(-moveAmount, 1f).SetEase(Ease.Linear));
                sq.Append(DOVirtual.DelayedCall(.2f, null));
                sq.Append(item.transform.DOLocalMoveY(0, .5f).SetEase(Ease.Linear));

                sq.SetEase(Ease.Linear).SetLoops(-1);

                moveSQs.Add(sq);

                await UniTask.Delay(TimeSpan.FromSeconds(.1f), cancellationToken: cts.Token).SuppressCancellationThrow();
            }
        }

        async void MakeRewardsScaleToOne(List<Transform> parents, Action onComplete)
        {
            foreach (var item in parents)
            {
                item.transform.DOScale(Vector3.one, .3f).SetEase(Ease.OutBack);

                await UniTask.Delay(TimeSpan.FromSeconds(.1f), cancellationToken: cts.Token).SuppressCancellationThrow();
            }

            onComplete?.Invoke();
        }

        void PrepareLayout()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }

            cts = new CancellationTokenSource();

            KillMoveSequences();

            var childCount = m_LayOutParent.childCount;

            for (int i = childCount - 1; i >= 0; i--)
            {
                var item = m_LayOutParent.GetChild(i);

                Destroy(item.gameObject);
            }

            var schildCount = m_ScreenParent.childCount;

            for (int i = schildCount - 1; i >= 0; i--)
            {
                var item = m_ScreenParent.GetChild(i);
                Destroy(item.gameObject);
            }

            foreach (var item in m_OnRewardToOpen)
                item.Invoke();

            duplicatedRewards = new List<DuplicatedData>();
        }

        private void Appear(bool blackBG = true)
        {
            m_OnAppear?.Invoke();
            if (blackBG) m_OnBlackAppear?.Invoke();
        }
        public void DisAppear()
        {
            KillMoveSequences();

            m_OnDisAppear?.Invoke();
            m_OnBlackDisAppear?.Invoke();
        }

        void KillMoveSequences()
        {
            if (moveSQs != null)
            {
                foreach (var item in moveSQs)
                    item.Kill();
                moveSQs.Clear();
            }
        }

        async void ClaimRewards(Type type, bool blocker = true)
        {
            KillMoveSequences();

            m_Blocker.gameObject.SetActive(blocker);

            var tasks = duplicatedRewards.Count;

            foreach (var dItem in duplicatedRewards)
            {
                var item = dItem.Rewardable;
                item.transform.localPosition = Vector3.zero;

                item.TryGiveReward(onRewardingCompleted: () =>
                {
                    tasks--;
                    dItem.OnRewardClaimed?.Invoke();
                });
            }

            DisAppear();

            await UniTask.WaitUntil(() => tasks <= 0);

            if (onAllRewardsClaimed != null)
                foreach (var item in onAllRewardsClaimed)
                    item?.Invoke();

            onAllRewardsClaimed = null;

            m_Blocker.gameObject.SetActive(false);

            if (type == Type.Layout)
                m_RewardLayoutClosed?.Invoke();
        }




        #region Screen Reward
        public static void OnPutRewardToScreenPos(RewardsWorldSpaceData data)
        {
            onPutRewardToScreenPos?.Invoke(data);
        }
        private async void PutRewardToScreenPos(RewardsWorldSpaceData data)
        {
            // onRewardsClaimed = new List<Action>() { data.OnRewardClaimed };
            onAllRewardsClaimed = null;

            PrepareLayout();

            await UniTask.DelayFrame(1);

            duplicatedRewards = new List<DuplicatedData>();

            var worldCamera = Camera.main;
            var canvasCamera = m_CanvasRect.worldCamera;
            Transform parent = m_ScreenParent;
            Vector3 screenPos = Vector3.zero;

            if (data.SelfPos.HasValue)
            {
                screenPos = data.SelfPos.Value;
            }
            else if (data.ScreenPos.HasValue)
            {
                screenPos = canvasCamera.ScreenToWorldPoint(data.ScreenPos.Value);
            }
            else if (data.WorldPos.HasValue)
            {
                parent = m_TempParent;
                screenPos = worldCamera.WorldToScreenPoint(data.WorldPos.Value);
                screenPos = canvasCamera.ScreenToWorldPoint(screenPos);
            }

            screenPos.z = 0;

            var rewardParentGo = new GameObject("Reward Parent", typeof(RectTransform));
            rewardParentGo.transform.SetParent(parent);
            rewardParentGo.transform.localScale = Vector3.one;

            rewardParentGo.transform.position = screenPos;
            var dupReward = Instantiate(data.Rewardable, rewardParentGo.transform);
            dupReward.transform.localPosition = Vector3.zero;

            if (data.ManualTarget != null)
                dupReward.SetTargetManual(data.ManualTarget);

            dupReward.Prepare(null);

            if (data.WorldPos.HasValue && dupReward.i_Targetable != null)
            {
                var targetPos = dupReward.m_TargetPos;
                var targetScreenPos = worldCamera.WorldToScreenPoint(targetPos);
                targetScreenPos = canvasCamera.ScreenToWorldPoint(targetScreenPos);
                targetScreenPos.z = 0;
                dupReward.m_TargetPos = targetScreenPos;
            }


            var dupData = new DuplicatedData()
            {
                Rewardable = dupReward,
                OnRewardClaimed = data.OnRewardClaimed
            };

            duplicatedRewards.Add(dupData);

            Appear(blackBG: false);

            ClaimRewards(type: Type.ScreenSpace, blocker : data.BlockScreen);
        }



        #endregion


        public Transform PutRewardInTempParent(Transform target)
        {
            target.SetParent(m_TempParent);

            var rewardParentGo = new GameObject("Reward Parent", typeof(RectTransform));
            rewardParentGo.transform.SetParent(m_TempParent);
            rewardParentGo.transform.localScale = Vector3.one;
            rewardParentGo.transform.position = target.transform.position;

            target.SetParent(rewardParentGo.transform);

            return rewardParentGo.transform;
        }

        public Transform GetTempParent() => m_TempParent;



        #region Actions

        public void z_Close()
        {
            DisAppear();
        }

        public void z_ClaimReward()
        {
            ClaimRewards(type: Type.Layout);
        }

        #endregion




        public class DuplicatedData
        {
            public Rewardable Rewardable;
            public Action OnRewardClaimed;
        }

        public class MergedData
        {
            public Rewardable Rewardable;
            public int Amount;
        }

        public struct RewardsToClaimData
        {
            public IEnumerable<Rewardable> Rewards;
            public Action OnAllRewardsClaimed;
        }

        public struct RewardsWorldSpaceData
        {
            public Rewardable Rewardable;
            public Vector3? WorldPos;
            public Vector3? ScreenPos;
            public Vector3? SelfPos;
            public bool BlockScreen;
            public RewardTarget ManualTarget;
            public Action OnRewardClaimed;
        }

    }
}
