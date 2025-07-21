using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BEH.Objective
{
    public interface IObjectivable
    {
        public string ID { get; }
        public int i_ObjectivablePriority { get; }
        public bool i_ObjectivableMovesToUI { get; }
        public bool i_ObjectivableIsEqual(string comparer);
        public Sprite GetIcon();
        public Color? GetColor();

    }

    public interface IObjectivableExtra
    {
        public void OnObjectiveStartedMovingToUI();
        public void OnObjectiveEndedMovingToUI();

    }



    [Serializable]
    public class Need : ISerializationCallbackReceiver
    {
        [SerializeField] Object IObject;

        public IObjectivable IObjectivable;

        public int NeededCount;
        public int ToBeCollectedCount;
        public int CollectedCount;

        public Need(IObjectivable iObjectivable, int neededCount)
        {
            IObjectivable = iObjectivable;
            NeededCount = neededCount;
            ToBeCollectedCount = 0;
            CollectedCount = 0;
        }
        public Need(Object @object, int neededCount)
        {
            IObject = @object;
            NeededCount = neededCount;
            ToBeCollectedCount = 0;
            CollectedCount = 0;
        }

        public bool TryConvert()
        {
            if (IObject == null)
                return false;

            if (IObject is IObjectivable icollectable)
            {
                IObjectivable = icollectable;
                return true;
            }

            return false;
        }

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize()
        {
            if (TryConvert())
                return;

            Debug.LogWarning($"[{IObject}] Object is not an IObjectivale!");

            IObject = null;
        }
        public void OnAfterDeserialize()
        {
            TryConvert();
        }

        public IObjectivable GetIObjectiveConverted()
        {
            if (TryConvert())
                return IObjectivable;

            return null;
        }

        #endregion


    }

    [Serializable]
    public class ObjectiveNeedsCollection
    {
        [Space(10)] public List<Need> m_Needs;
        [Space(10)] public bool IsFinished;
        [Space(10)] public bool IsPendingFinished;

        private Events events;
        public struct Events
        {
            public Action<string, int> onNeedUpdated;
            public Action<string> onNeedCompleted;
            public Action onFinished;
        }
        public ObjectiveNeedsCollection(List<Need> needs, Events events)
        {
            this.events = events;

            m_Needs = new List<Need>();

            foreach (var item in needs)
            {
                if (item.TryConvert())
                    m_Needs.Add(item);
            }

            IsFinished = false;
            IsPendingFinished = false;

            OnStart();
        }


        public void OnStart()
        {
            if (m_Needs == null || m_Needs.Count == 0)
                return;

            IsFinished = false;
            IsPendingFinished = false;

            //var highPrios = m_Needs.Where(x => x.ICollectable.Priority > 0).Select(x => x).OrderByDescending(x => x.NeededCount);
            //var lowPrios = m_Needs.Where(x => x.ICollectable.Priority <= 0).Select(x => x).OrderByDescending(x => x.NeededCount);

            //m_Needs = new List<Need>();

            //foreach (var item in highPrios)
            //    m_Needs.Add(item);
            //foreach (var item in lowPrios)
            //    m_Needs.Add(item);

            m_Needs = m_Needs.OrderBy(x => x.IObjectivable.i_ObjectivablePriority).ThenByDescending(x => x.NeededCount).ToList();

            OnResetNeeds();

            //   m_Needs = m_Needs.OrderBy(x => x.ICollectable.Priority).ToList();
        }
        public void OnResetNeeds()
        {
            foreach (var n in m_Needs)
            {
                n.ToBeCollectedCount = 0;
                n.CollectedCount = 0;
            }
        }


        public Need GetNeededObjective(string id, bool? isNeeded, int count)
        {
            if (IsFinished)
                return null;

            if (isNeeded == null)
            {
                if (!IsNeeded_TempCollect(id, count))
                    return null;
            }
            else if (isNeeded.Value == false)
                return null;


            return GetNeed_Collected(id);
        }

        public bool IsNeeded_Simple(string id)
        {
            var needs = GetNeeds_ToBeCollected(id);

            if (needs == null || needs.Count() == 0)
                return false;

            return true;
        }

        public bool IsNeeded_TempCollect(string ID, int count)
        {
            var needs = GetNeeds_ToBeCollected(ID);

            if (needs == null || needs.Count() == 0)
                return false;

            foreach (var n in needs)
                n.ToBeCollectedCount += count;

            CheckPendingFinished();

            return true;
        }
        Need GetNeed_Collected(string ID)
        {
            var need = m_Needs.Find(x => x.IObjectivable.i_ObjectivableIsEqual(ID) && x.NeededCount > x.CollectedCount);
            return need;
        }
        IEnumerable<Need> GetNeeds_ToBeCollected(string ID)
        {
            var need = m_Needs.Where(x => x.IObjectivable.i_ObjectivableIsEqual(ID) && x.NeededCount > x.ToBeCollectedCount);
            return need;
        }
        IEnumerable<Need> GetNeeds_Collected(string ID)
        {
            var need = m_Needs.Where(x => x.IObjectivable.i_ObjectivableIsEqual(ID) && x.NeededCount > x.CollectedCount);
            return need;
        }

        public Need GetRandomNeed()
        {
            var need = m_Needs.Find(x => x.NeededCount > x.ToBeCollectedCount);
            return need;
        }
        public Need GetNotSameNeed(string id)
        {
            var need = m_Needs.FirstOrDefault(x => x.NeededCount > x.ToBeCollectedCount && !x.IObjectivable.i_ObjectivableIsEqual(id));
            return need;
        }

        public void OnCollected(string ID, int count)
        {
            var needs = GetNeeds_Collected(ID);

            if (needs != null)
            {
                foreach (var n in needs)
                {
                    n.CollectedCount += count;

                    var need = n.NeededCount - n.CollectedCount;

                    if (need <= 0)
                        need = 0;

                    events.onNeedUpdated?.Invoke(n.IObjectivable.ID, need);

                    var done = need == 0;

                    if (done)
                        events.onNeedCompleted?.Invoke(n.IObjectivable.ID);
                }
            }

            var isfinished = IsDone();

            if (isfinished)
            {
                events.onFinished?.Invoke();
                IsFinished = true;
            }

        }
        bool IsDone()
        {
            foreach (var item in m_Needs)
            {
                if (item.NeededCount > item.CollectedCount)
                    return false;
            }
            return true;
        }
        void CheckPendingFinished()
        {
            foreach (var item in m_Needs)
            {
                if (item.NeededCount > item.ToBeCollectedCount)
                {
                    IsPendingFinished = false;
                    return;
                }
            }

            IsPendingFinished = true;
        }



        public int GetRemainingObjectivesCount()
        {
            var res = 0;

            if (m_Needs == null || m_Needs.Count == 0)
                return res;

            foreach (var need in m_Needs)
            {
                var remaining = need.NeededCount - need.ToBeCollectedCount;

                if (remaining < 0)
                    remaining = 0;

                res += remaining;
            }

            return res;
        }





        public void ChangeNeed(string fromID, IObjectivable iCollectable)
        {
            var need = m_Needs.Find(x => x.IObjectivable.ID.Equals(fromID));

            if (need != null)
            {
                need.NeededCount--;

                if (need.NeededCount == 0)
                    m_Needs.Remove(need);
            }

            var av = m_Needs.Find(x => x.IObjectivable.ID.Equals(iCollectable.ID));

            if (av != null)
                av.NeededCount++;
            else
                m_Needs.Add(new Need(iCollectable, 1));

        }
    }
}
