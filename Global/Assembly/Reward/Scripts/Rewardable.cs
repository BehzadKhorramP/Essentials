using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using System.Threading;
using System.Linq;
using MadApper;
using Sirenix.OdinInspector;

namespace BEH.Reward
{
    public class Rewardable : SerializedMonoBehaviour, ITargetSeeker
    {
        [SerializeField] internal RewardData m_RewardData;
        [PropertySpace(0, 10)][SerializeField] internal ResourceItemSO.TextRule m_TextRule;


        [FoldoutGroup("Visuals")]
        [Space(10)][SerializeField] List<TextMeshProUGUI> m_AmountText;
        [FoldoutGroup("Visuals")]
        [Space(10)][SerializeField] internal VFX m_VFXPrefab;
        [FoldoutGroup("Visuals")]
        [Space(10)][SerializeField] internal Transform m_GenerateRewardPrefab;

        [FoldoutGroup("Events")]
        [Space(10)][SerializeField] UnityEventDelayList<RewardData> m_OnPreparingReward;
        [FoldoutGroup("Events")]
        [Space(10)][SerializeField] UnityEventDelayList<RewardData> m_TryStartGivingReward;
        [FoldoutGroup("Events")]
        [Space(10)][SerializeField] UnityEventDelayList<RewardData> m_OnStartedGiveRewardEvent;
        [FoldoutGroup("Events")]
        [Space(10)][SerializeField] UnityEventDelayList<RewardData> m_OnRewardGivenEvent;

        [FoldoutGroup("Params")]
        [Space(10)][SerializeField] internal EffectsData m_EffectsData;
        [FoldoutGroup("Params")]
        [Space(10)][SerializeField] internal Courier.Params m_CourierParams;
        [FoldoutGroup("Params")]
        [Space(10)][SerializeField][AutoGetOrAdd] internal CameraSeeker m_CameraSeeker;

        public ResourceItemSO m_ResourceSO => m_RewardData.ResourceSO;
        public string m_CourierTag => m_ResourceSO.m_ID + "Courier";


        [SerializeField] List<IResourceTracking> m_IResourceTrackings;

        CancellationTokenSource cts;
        Action onRewardingCompleted;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying)
                return;

            var iResourceTrackings = TryGetIResourceTrackings();
            if (iResourceTrackings != null && iResourceTrackings.Any())
            {
                if (m_IResourceTrackings == null || m_IResourceTrackings.Count != iResourceTrackings.Count())
                {
                    m_IResourceTrackings = new List<IResourceTracking>(iResourceTrackings);
                    this.TrySetDirty();
                }
            }
            else
            {
                if (m_IResourceTrackings != null)
                {
                    m_IResourceTrackings = null;
                    this.TrySetDirty();
                }             
            }

            if (m_AmountText != null && m_AmountText.Any() && IsRewardDataValid())
            {
                var currText = m_AmountText[0].text;
                var itemText = m_ResourceSO.GetAmountText(m_RewardData.Amount, m_RewardData.ResourceType, textRule: m_TextRule);

                if (currText == itemText)
                    return;

                m_AmountText.ForEach(x => x.SetText(itemText));
                m_AmountText.ForEach(x => x.TrySetDirty());
            }
        }
#endif
        private void Start()
        {
            cts = new CancellationTokenSource();

            if (!IsRewardDataValid())
            {
                Debug.LogWarning($"{name} : Rewardable's data is not valid!");
            }
        }

        private void OnDestroy()
        {
            Stop();
        }

        void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
            cts = null;
        }

        #region ITargetSeeker

        public Vector3 m_TargetPos { get; set; }
        public TargetableBase i_Targetable { get; set; }
        public void TryFindTarget()
        {
            if (i_Targetable != null)
                return;

            this.TryFindTargetExt();
        }


        public void SetTargetable<T>(TargetableBase<T> target) where T : ITargetSeeker
        {
            i_Targetable = target.TryConverTargetTo(target);

            if (i_Targetable != null)
                m_TargetPos = i_Targetable.GetTargetTransform().position;
        }
        public void SetTargetManual(RewardTarget target)
        {
            i_Targetable = target;

            if (i_Targetable != null)
                m_TargetPos = i_Targetable.GetTargetTransform().position;
        }
        #endregion

        public bool IsRewardDataValid() => m_RewardData != null && m_RewardData.ResourceSO != null;

        public bool IsEqual(Rewardable compare)
        {
            if (!compare.IsRewardDataValid() || !this.IsRewardDataValid())
                return false;

            if (!compare.m_ResourceSO.m_UID.Equals(m_ResourceSO.m_UID))
                return false;

            if (compare.m_RewardData.ResourceType != m_RewardData.ResourceType)
                return false;

            return true;
        }


        public void SetModifiedAmount(int amount)
        {
            m_RewardData.Amount = amount;
        }
        public void TryRefreshText()
        {
            var text = m_ResourceSO.GetAmountText(m_RewardData.Amount, m_RewardData.ResourceType, textRule: m_TextRule);
            m_AmountText.ForEach(x => x.SetText(text));
        }

        public void SetRewardData(RewardData data)
        {
            m_RewardData = data;
            TryRefreshText();
        }


        [Button]
        public void PrepareAndGiveTest()
        {
            Prepare(null);
            TryGiveReward(null);
        }

        public void Prepare(int? mergedAmount)
        {
            if (mergedAmount.HasValue)
                SetModifiedAmount(mergedAmount.Value);

            TryRefreshText();
            m_OnPreparingReward?.Invoke(m_RewardData);
            TryFindTarget();
        }
        public void TryGiveReward(Action onRewardingCompleted)
        {
            this.onRewardingCompleted = onRewardingCompleted;
            m_TryStartGivingReward?.Invoke(m_RewardData);
        }
        public void OnStartedGivingRewardEvent()
        {
            m_OnStartedGiveRewardEvent?.Invoke(m_RewardData);
        }
        public void OnRewardingCompleted()
        {
            m_OnRewardGivenEvent?.Invoke(m_RewardData);
            TrackDataSource();
            onRewardingCompleted?.Invoke();
        }




        IResourceTracking[] TryGetIResourceTrackings() => GetComponents<IResourceTracking>();
        public IEnumerable<IResourceTracking> GetIResourceTrackings() => m_IResourceTrackings;

        public void TrackDataSource()
        {
            if (m_IResourceTrackings == null || !m_IResourceTrackings.Any())
                return;

            var resource = m_RewardData.ResourceSO;
            var amount = m_RewardData.Amount;
            var type = m_RewardData.ResourceType;

            foreach (var track in m_IResourceTrackings)
                track?.Source(resource: resource, amount: amount, resourceType: type);
        }




        #region Inspector Events

        public void z_DestroySelf()
        {
            if (gameObject != null)
            {
                Stop();
                Destroy(gameObject, .1f);
            }
        }
        public void z_MoveSelfToTarget()
        {
            this.MoveToTarget();
        }
        public void z_MoveMultipleGeneratedsToTarget()
        {
            this.MoveMultipleGeneratedsToTarget();
        }
        public void z_PopSelfIfTargetNotFound()
        {
            this.PopSelfIfTargetNotFound();
        }

        //        public async void z_StayAndFadeIcon(float delay)
        //        {
        //            if (m_Icon == null)
        //                return;

        //            if (m_Target != null)
        //                m_Icon.transform.SetParent(m_Target.GetTargetTransform());

        //            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: m_cts.Token).SuppressCancellationThrow();

        //#if DOTWEEN_ENABLED
        //            m_Icon?.DOFade(0, .3f); 
        //#endif
        //        }

        //        public async void z_StayAndFadeText(float delay)
        //        {
        //            if (m_TextCG == null)
        //                return;

        //            if (m_Target != null)
        //            {
        //                m_TextCG.transform.SetParent(m_Target.GetTargetTransform());
        //                var locPos = m_TextCG.transform.localPosition;
        //                locPos.z -= 305;
        //                m_TextCG.transform.localPosition = locPos;
        //            }

        //            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cts.Token).SuppressCancellationThrow();

        //#if DOTWEEN_ENABLED
        //            m_TextCG?.DOFade(0, .3f);
        //#endif
        //        }
        //public async void z_MoveUpText(float timer)
        //{
        //    await UniTask.Delay(TimeSpan.FromSeconds(.1f), cancellationToken: cts.Token).SuppressCancellationThrow();

        //    if (m_TextCG == null)
        //        return;

        //    m_TextCG.transform.DOMoveY(.5f, timer).SetEase(Ease.InBack);
        //}






        #endregion
    }




    public static class RewardableExtention
    {

        public static void MoveToTarget(this Rewardable rewardable)
        {
            var itemSO = rewardable.m_ResourceSO;
            var amount = rewardable.m_RewardData.Amount;
            var type = rewardable.m_RewardData.ResourceType;
            var courierTag = rewardable.m_CourierTag;


            if (rewardable.i_Targetable == null)
            {
                rewardable.PopSelfIfTargetNotFound();
                return;
            }

            rewardable.OnStartedGivingRewardEvent();

            Action onExecuted = () =>
            {
                itemSO.Modify(amount, type);
                rewardable.i_Targetable.OnReachedToTarget();
                rewardable.OnRewardingCompleted();
            };

            var delay = .3f;

            //      var courier = PoolVFXCourier.Get(courierTag, prefab: courierPrefab);

            var rewardTempParent = RewardSystem.Instance?.GetTempParent();

            //var data = new VFXCourier.Data
            //{
            //    Obj = rewardable.transform,
            //    Target = rewardable.i_Targetable.GetTargetTransform(),
            //    NewParent = rewardTempParent,
            //    SetAsTargetChild = true,
            //    Speed = 1.2f,
            //    Delay = delay,
            //    MidOffset = 1f,
            //    OrgScale = rewardable.transform.localScale,
            //    MidOffsetType = VFXCourier.Data.MiddleOffsetType.Horizontal,
            //    OnNearTarget = () => { rewardable.transform.DOScale(1, .3f); },
            //    OnCompleted = onExecuted
            //};

            //    courier.OnExecute(data);
        }

        public async static void MoveMultipleGeneratedsToTarget(this Rewardable rewardable)
        {
            var itemSO = rewardable.m_ResourceSO;
            var amount = rewardable.m_RewardData.Amount;
            var type = rewardable.m_RewardData.ResourceType;
            var courierTag = rewardable.m_CourierTag;
            var vfxPrefab = rewardable.m_VFXPrefab;
            var generatePrefab = rewardable.m_GenerateRewardPrefab;
            var effectsDat = rewardable.m_EffectsData;
            var courierParams = rewardable.m_CourierParams;
            var cameraseeker = rewardable.m_CameraSeeker;
            var targetPos = rewardable.m_TargetPos;

            if (generatePrefab == null || rewardable.i_Targetable == null)
            {
                rewardable.PopSelfIfTargetNotFound();
                return;
            }

            rewardable.OnStartedGivingRewardEvent();

            if (amount == 0)
                return;

            var generateds = new List<Transform>();
            var count = Random.Range(effectsDat.CountLimit.x, effectsDat.CountLimit.y);
            count = Mathf.Min(count, amount);

            var tasks = count;

            var portionAmount = amount / count;

            var camera = await cameraseeker.GetCamera();
            var target = rewardable.i_Targetable.GetTargetTransform();

            for (int i = 0; i < count; i++)
            {
                var inc = UnityEngine.Object.Instantiate(generatePrefab);

                inc.transform.SetParent(target);

                var locScale = inc.transform.localScale;

                inc.transform.SetParent(rewardable.transform);

                var xBound = effectsDat.SplashBounds[0];
                var yBound = effectsDat.SplashBounds[1];
                var zBound = effectsDat.SplashBounds[2];

                inc.transform.localPosition = new Vector3(
                    xBound.GetValueMultiplier(),
                    yBound.GetValueMultiplier(),
                    zBound.GetValueMultiplier());

                locScale = locScale * effectsDat.ScaleRandomizer.GetValueDivided();

                inc.transform.localScale = locScale;

                var delay = effectsDat.DelayToStart.GetValueDivided();

                var vfx = PoolVFX.Get(courierTag, prefab: vfxPrefab);

                var rewardTempParent = RewardSystem.Instance?.GetTempParent();

                Action<Courier> onStarted = (courier) =>
                {
                    courier.transform.SetParent(rewardTempParent);
                    courier.transform.SetAsFirstSibling();
                };

                Action<Courier> onExecuted = (courier) =>
                {
                    itemSO.Modify(portionAmount, type);
                    rewardable.i_Targetable.OnReachedToTarget();

                    if (inc != null)
                        GameObject.Destroy(inc.gameObject);

                    tasks--;
                };
                var vfxArgs = new VFX.Args();

                var courierArgs = new Courier.Args
                {
                    StartPosition = inc.transform.position,
                    EndPosition = targetPos,
                    Delay = delay,
                    Parameters = courierParams,
                    Camera = camera
                };

                courierArgs.Move(vfx: vfx, vfxArgs: vfxArgs, obj: inc, onStarted: onStarted, onEnded: onExecuted);

                generateds.Add(inc);

                await UniTask.WaitForSeconds(effectsDat.MultipleIntervalDelay);
            }

            await UniTask.WaitUntil(() => tasks <= 0);

            // if a difference caused by dividing to portionamount
            var diff = amount - (count * portionAmount);

            if (diff > 0)
                itemSO.Modify(diff, type);

            rewardable.OnRewardingCompleted();
        }


        public static void PopSelfIfTargetNotFound(this Rewardable rewardable)
        {
            var itemSO = rewardable.m_ResourceSO;
            var amount = rewardable.m_RewardData.Amount;
            var type = rewardable.m_RewardData.ResourceType;
            var transform = rewardable.transform;

            Action onExecuted = () =>
            {
                itemSO.Modify(amount, type);
                rewardable.OnRewardingCompleted();
            };

            var rewardSys = RewardSystem.Instance;

            if (rewardSys != null)
                transform = rewardSys.PutRewardInTempParent(transform);

            var sq = DOTween.Sequence();

            sq.Append(transform.DOScale(Vector3.one * 1.2f, .35f).SetEase(Ease.InBack));
            sq.Append(DOVirtual.DelayedCall(.15f, null));
            sq.Append(transform.DOScale(Vector3.zero, .2f));

            sq.OnComplete(() =>
            {
                transform.DOKill();

                onExecuted?.Invoke();

                if (transform != null)
                    GameObject.Destroy(transform.gameObject, .15f);
            });
        }

    }


    [Serializable]
    public class RewardData
    {
        public ResourceItemSO ResourceSO;
        public ResourceType ResourceType;
        public int Amount;

        public void GiveReward()
        {
            if (ResourceSO == null)
                return;

            ResourceSO.Modify(Amount, type: ResourceType);
        }
    }

    [Serializable]
    public class EffectsData
    {
        public Vector2Int CountLimit = new Vector2Int(6, 11);

        public Randomizer[] SplashBounds = new Randomizer[]
        {
           new Randomizer(new Vector2( -5, 6), 6),
           new Randomizer(new Vector2( -5, 6), 6),
           new Randomizer(new Vector2( -5, 6), 6)
        };

        public Randomizer ScaleRandomizer = new Randomizer(new Vector2(6, 8), 6);
        public Randomizer DelayToStart = new Randomizer(new Vector2(.1f, .2f), 1);

        public float MultipleIntervalDelay = .1f;

        [Serializable]
        public class Randomizer
        {
            public Vector2 Range;
            public float Factor;

            public Randomizer(Vector2 range, float factor)
            {
                Range = range;
                Factor = factor;
            }

            public float GetValueDivided() => Random.Range(Range.x, Range.y) / Factor;
            public float GetValueMultiplier() => Random.Range(Range.x, Range.y) * Factor;
        }
    }
}





