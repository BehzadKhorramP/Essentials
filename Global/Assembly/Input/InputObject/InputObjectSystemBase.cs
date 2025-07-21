using BEH.Common;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Input
{
    public abstract class InputObjectSystemBase<TInputObject> : MonoBehaviour
    {
        public enum Coord { XZ, XY }

        [FoldoutGroup("Setup")][Space(10)][SerializeField] protected LayerMask m_Mask = -1;
        [FoldoutGroup("Setup")][Space(10)][SerializeField] protected float m_CastRadius = .01f;
        [FoldoutGroup("Setup")][Space(10)][SerializeField] protected Coord m_Coord = Coord.XZ;
        [FoldoutGroup("Drag")][Space(10)][SerializeField] protected float dragDelay = 0f;


        [NonSerialized] Camera _cam;
        protected Camera cam => _cam ??= Camera.main;

        [NonSerialized] Plane? _plane;
        protected Plane plane
        {
            get
            {
                if (!_plane.HasValue)
                {
                    switch (m_Coord)
                    {
                        case Coord.XZ: _plane = new Plane(Vector3.up, Vector3.zero); break;
                        case Coord.XY: _plane = new Plane(-Vector3.forward, Vector3.zero); break;
                        default: _plane = new Plane(Vector3.up, Vector3.zero); break;
                    }

                }
                return _plane.Value;
            }
        }

        protected QBool isActive { get; set; } = new QBool();
        protected IDraggable draggable;

        Tween dragDelayTween;

        RaycastHit[] buffer = new RaycastHit[10];

        protected Dictionary<int, HitData> mapping = new Dictionary<int, HitData>();

        public static Action<bool, string> onInputObjectSystemInteractibility;

        protected virtual void OnEnable()
        {
            if (mapping == null) mapping = new Dictionary<int, HitData>();

            OnReset();

            onInputObjectSystemInteractibility += OnInteractability;

            InputManager.Instance.Pressed(Pressed);
            InputManager.Instance.Dragged(Dragged);
            InputManager.Instance.Released(Released);
            InputManager.Instance.Tapped(Tapped);
            InputManager.Instance.Swiped(Swiped);
        }

        protected virtual void OnDisable()
        {
            onInputObjectSystemInteractibility -= OnInteractability;

            InputManager.Instance?.PressedUnsubscribe(Pressed);
            InputManager.Instance?.DraggedUnsubscribe(Dragged);
            InputManager.Instance?.ReleasedUnsubscribe(Released);
            InputManager.Instance?.TappedUnsubscribe(Tapped);
            InputManager.Instance?.SwipedUnsubscribe(Swiped);
        }

        protected virtual void Update()
        {
            OnTryDragging();
        }


        private void OnInteractability(bool active, string tag)
        {
            if (active) UnLock(tag);
            else Lock(tag);
        }

        public bool IsActiveAndNotOnUI(Vector2 pos) => isActive == true && !pos.IsClickedOnRect();

        public void Lock(string tag) => isActive.Lock(tag);
        public void UnLock(string tag) => isActive.Unlock(tag);
        public void Clear() => isActive.Clear();

        protected virtual void OnReset()
        {
            mapping.Clear();
            Clear();
        }

        private void Pressed(SwipeInput input)
        {
            var pos = input.EndPosition;

            if (!IsActiveAndNotOnUI(pos)) return;

            if (mapping.ContainsKey(input.InputId)) mapping.Remove(input.InputId);

            cam.GetSphereCastWorldPosition(m_Mask, input.EndPosition, m_CastRadius, out RaycastHit rayHit, out Ray ray, buffer);

            if (rayHit.collider == null) return;

            if (rayHit.collider.TryGetComponent(out TInputObject @object))
            {
                mapping[input.InputId] = new HitData() { Object = @object, RaycastHit = rayHit };
                OnPressed(@object);
                TrySetDraggable(@object);
            }
        }

        private void Dragged(SwipeInput input)
        {
            var pos = input.StartPosition;

            if (!IsActiveAndNotOnUI(pos)) return;

            if (mapping.ContainsKey(input.InputId)) return;

            Vector2 worldPrevious = cam.ScreenToWorldPoint(input.PreviousPosition);
            Vector2 worldCurrent = cam.ScreenToWorldPoint(input.EndPosition);

            var lineCast = Physics.Linecast(worldPrevious, worldCurrent, out RaycastHit rayHit, m_Mask);

            if (rayHit.transform == null)
                return;

            if (rayHit.transform.TryGetComponent(out TInputObject @object))
            {
                mapping[input.InputId] = new HitData() { Object = @object, RaycastHit = rayHit };
                OnDragged(@object);
                TrySetDraggable(@object);
            }
        }
        private void Released()
        {
            foreach (var item in mapping)
            {
                var @object = item.Value.Object;

                if (@object != null)
                    OnReleased(@object);
            }

            OnReleased();
            TryReleaseDraggable();

            mapping.Clear();
        }

        private void Tapped(TapInput input)
        {
            var pos = input.ReleasePosition;

            if (!IsActiveAndNotOnUI(pos)) return;

            foreach (var item in mapping)
            {
                var hitData = item.Value;
                var obj = hitData.Object;

                if (obj != null)
                {
                    var tapData = new TapData() { HitData = hitData, TapInput = input };

                    OnTapped(obj);
                    OnTapped(tapData);
                }
            }
        }


        private void Swiped(SwipeInput input)
        {
            if (!IsActiveAndNotOnUI(input.StartPosition)) return;

            if (!mapping.TryGetValue(input.InputId, out HitData hitData)) return;

            var obj = hitData.Object;

            var dir = GetAbsDir(input.SwipeDirection);
            var swipeData = new SwipeData(obj, dir);

            OnSwiped(swipeData);
        }

        Vector2Int GetAbsDir(Vector2 inputDir)
        {
            var absxDiff = Mathf.Abs(inputDir.x);
            var absyDiff = Mathf.Abs(inputDir.y);

            if (absxDiff >= absyDiff)
            {
                if (inputDir.x < 0)
                    return new Vector2Int(-1, 0);
                else
                    return new Vector2Int(1, 0);
            }
            else
            {
                if (inputDir.y < 0)
                    return new Vector2Int(0, 1);
                else
                    return new Vector2Int(0, -1);
            }
        }

        #region Virtuals

        protected virtual void OnPressed(TInputObject @object) { }
        protected virtual void OnDragged(TInputObject @object) { }
        protected virtual void OnReleased() { }
        protected virtual void OnReleased(TInputObject @object) { }

#if UNITY_EDITOR
        [Obsolete("use OnTapped(TapData tapData)")]
#endif
        protected virtual void OnTapped(TInputObject @object) { }
        protected virtual void OnTapped(TapData tapData) { }

        protected virtual void OnSwiped(SwipeData data) { }

        #endregion



        #region Draggable
        protected virtual void TrySetDraggable(TInputObject target)
        {
            if (draggable != null) return;
            if (target is not IDraggable iDraggable) return;
            if (dragDelayTween != null) dragDelayTween.Kill();

            if (dragDelay > 0) dragDelayTween = DOVirtual.DelayedCall(dragDelay, Set);
            else Set();

            void Set()
            {
                draggable = iDraggable;
                draggable.Pressed(GetProjectedWorldPosition());
            }
        }
        protected virtual void TryReleaseDraggable()
        {
            if (dragDelayTween != null) { dragDelayTween.Kill(); dragDelayTween = null; }
            if (draggable == null) return;

            draggable.Released(GetProjectedWorldPosition());
            draggable = null;
        }
        protected virtual void OnTryDragging()
        {
            if (isActive == false) return;
            if (draggable == null) return;

            draggable.Dragging(GetProjectedWorldPosition());
        }

        /// <summary>
        /// e.g. when player is dragging a piece and loses/wins or other cancel scenarios
        /// </summary>
        public virtual void ForceReleaseDraggable()
        {
            if (draggable == null) return;

            draggable.ReleaseForced(GetProjectedWorldPosition());
            draggable = null;
        }
        #endregion



        #region Inspector

        public void z_ForceReleaseDraggable()
        {
            ForceReleaseDraggable();
        }

        #endregion


        public virtual Vector3 GetProjectedWorldPosition()
        {
            var touchPos = InputManager.Instance.GetPointerPosition();
            Ray ray = cam.ScreenPointToRay(touchPos);
            plane.Raycast(ray, out float distanceToPlane);
            return ray.GetPoint(distanceToPlane);
        }


        public struct TapData
        {
            public HitData HitData;
            public TapInput TapInput;
        }
        public struct HitData
        {
            public TInputObject Object;
            public RaycastHit RaycastHit;
        }
        public struct SwipeData
        {
            public TInputObject Object;
            public Vector2Int Dir;

            public SwipeData(TInputObject @object, Vector2Int dir)
            {
                Object = @object;
                Dir = dir;
            }
        }
    }
}
