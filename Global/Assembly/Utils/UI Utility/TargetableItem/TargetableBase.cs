using MadApper;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BEH
{
    public interface ITargetSeeker
    {
        public TargetableBase i_Targetable { get; set; }
        public void SetTargetable<T>(TargetableBase<T> target) where T : ITargetSeeker;
    }

    [DefaultExecutionOrder(-1000)]
    public static class TargetablesManager
    {
        static List<TargetableBase> list;
        static List<TargetableBase> s_List
        {
            get
            {
                if (list == null) list = new List<TargetableBase>();
                return list;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            list = new List<TargetableBase>();

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Reset();
            }
        }
#endif
        static void Reset()
        {
            list = null;

#if UNITY_EDITOR

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif     
        }


        public static void TryAddTargetablesManager(this TargetableBase target)
        {
            if (s_List.Contains(target)) return;
            s_List.Add(target);
        }
        public static void TryRemoveFromTargetablesManager(this TargetableBase target)
        {
            if (!s_List.Contains(target)) return;
            s_List.Remove(target);
        }

        public static void TryFindTargetExt<T>(this T seeker) where T : ITargetSeeker
        {
            var targets = s_List.Where(x => x.IsTarget(seeker));

            if (targets == null || !targets.Any())
            {
                var tempTarget = s_List.Where(x => x.IsTempTarget(seeker));

                if (tempTarget != null && tempTarget.Any())
                {
                    seeker.SetTargetable(tempTarget.First() as TargetableBase<T>);
                }
            }
            else
            {
                targets = targets.OrderByDescending(x => x.m_Priority);

                var visibiles = targets.Where(x => x.IsVisible());

                if (visibiles != null && visibiles.Any())
                {
                    seeker.SetTargetable(visibiles.First() as TargetableBase<T>);
                }
            }
        }

        public static G TryConverTargetTo<T, G>(this TargetableBase<T> targetbase, G targetTemp) where T : ITargetSeeker where G : TargetableBase
        {
            if (targetbase is G rTarget)
                return rTarget;

            return null;
        }

    }

    public abstract class TargetableBase : MonoBehaviour
    {
        [SerializeField] internal int m_Priority;
        [SerializeField] protected Transform m_TargetTransform;
        [AutoGetInParent][SerializeField] public List<CanvasGroup> m_CanvasGroups;
        [SerializeField] protected UnityEventDelayList m_OnReachedToTarget;

        public virtual void OnEnable()
        {
            this.TryAddTargetablesManager();
        }
        public virtual void OnDisable()
        {
            this.TryRemoveFromTargetablesManager();
        }

        public abstract bool IsTarget<T>(T seeker) where T : ITargetSeeker;
        public abstract bool IsTempTarget<T>(T seeker) where T : ITargetSeeker;

        public bool IsVisible()
        {
            if (m_CanvasGroups == null) return true;

            foreach (var item in m_CanvasGroups)
            {
                if (item == null) continue;
                if (item.alpha == 0) return false;
            }

            return true;
        }

        public virtual void OnReachedToTarget() => m_OnReachedToTarget?.Invoke();
        public Transform GetTargetTransform() => m_TargetTransform != null ? m_TargetTransform : transform;
    }



    public abstract class TargetableBase<T> : TargetableBase where T : ITargetSeeker
    {

        public override bool IsTarget<G>(G generic)
        {
            if (generic is not T seeker) return false;
            return IsTargetExtra(seeker);
        }

        public virtual bool IsTargetExtra(T seeker) => true;


        public override bool IsTempTarget<G>(G generic)
        {
            if (generic is not T seeker) return false;
            return IsTempTargetExtra(seeker);
        }
        public virtual bool IsTempTargetExtra(T seeker) => true;

    }

}
