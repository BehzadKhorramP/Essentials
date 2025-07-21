using MadApper;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using System.Threading;

namespace BEH
{

    public class PoolCourier : Pool<Courier> { }

    public class Courier : MonoBehaviour, IPoolable
    {
        public enum Coord { XZ, XY }

        private Args args;
        private float t = 0;
        private bool isMoving;
        private bool isMovingLocalSpace;

        private Vector3 startPoint;
        private Vector3 controlPoint; // control point for cubic curve
        private Vector3 endPoint;


        public Transform TestTarget;
        public Params TestParameters;


        #region IPoolable
        public bool i_InPool { get; set; }
        public string i_PoolID { get; set; }
        public void i_OnSpawned(bool instantiated) { }
        public void Despawn() { PoolCourier.Despawn(this); }
        #endregion

        CancellationTokenSource cts;
        CancellationToken ctsToken;

        void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
            cts = null;
            isMoving = false;
        }

        public void Cancel()
        {
            Stop();
            Despawn();
        }


        [Button]
        public void Test()
        {
            if (TestParameters == null || TestTarget == null)
                return;

            var args = new Args()
            {
                StartPosition = transform.position,
                EndPosition = TestTarget.position,
                Parameters = TestParameters
            };

            Move(args);
        }

        public async void Move(Args args)
        {
            Stop();

            if (args.CancellationToken.HasValue)
            {
                ctsToken = args.CancellationToken.Value;
            }
            else
            {
                cts = new CancellationTokenSource();
                ctsToken = cts.Token;
            }

            try
            {
                await Init(args).AttachExternalCancellation(ctsToken);
            }
            catch (Exception) { }
        }


        async UniTask Init(Args args)
        {
            isMoving = false;

            this.args = args;

            t = 0;

            var rhs = args.Parameters.Coord == Coord.XZ ? Vector3.up : -Vector3.forward;

            startPoint = args.StartPosition + args.Parameters.StartOffset;
            endPoint = args.EndPosition + args.Parameters.EndOffset;

            var negate = args.Parameters.RandomizeSide == true ? Random.Range(0, 2) > 0 ? false : true : false;

            if (args.Parameters.ScreenBasedSide)
                if (!startPoint.x.AreApproximatelyEqual(endPoint.x, .1f))
                    negate = startPoint.x > endPoint.x ? true : false;

            var negateVector = Vector3.one;

            if (negate)
            {
                negateVector = args.Parameters.ManualNegateVector;
            }

            var curvuture = args.Parameters.Curvature;

            if (args.Parameters.RandomizeCurvuture) curvuture += Random.Range(-3, 4) / 20f;

            controlPoint = CourierExtentions.CalculateQuadraticControlPoint(
                start: startPoint,
                end: endPoint,
                rhs: rhs,
                manualPerpendicular: args.Parameters.ManualPerpendicular,
                curvature: curvuture,
                negateVector: negateVector,
                camera: args.Camera,
                localToWorldContext: args.LocalToWorldContext);

            if (args.Parameters.ManualPerpendicular != Vector3.zero) controlPoint += args.Parameters.ManualPerpendicular * args.Parameters.MidOffset;
            else controlPoint += rhs * args.Parameters.MidOffset;

            controlPoint += args.Parameters.ManualMidOffset;
            transform.position = args.StartPosition;

            args.OnStarted?.Invoke(this);

            if (args.Parameters.StartOffset.magnitude > 0)
            {
                transform.DOMove(startPoint, .25f);
                await UniTask.WaitForSeconds(.25f, cancellationToken: ctsToken);
            }

            if (args.Delay.HasValue) await UniTask.WaitForSeconds(args.Delay.Value, cancellationToken: ctsToken);

            isMovingLocalSpace = args.LocalToWorldContext != null;
            isMoving = true;
        }


        private void Update()
        {
            if (!isMoving) return;

            // Increment t based on the duration
            t += Time.deltaTime / args.Parameters.Duration;
            t = Mathf.Clamp01(t);

            float easedT = DOVirtual.EasedValue(0, 1, t, args.Parameters.Ease);

            var endPos = args.TargetTransform != null ? args.TargetTransform.position : endPoint;

            var pos = CourierExtentions.GetQuadraticBezierPoint(
                t: easedT,
                p0: startPoint,
                p1: controlPoint,
                p2: endPos);

            if (isMovingLocalSpace) transform.localPosition = pos;
            else transform.position = pos;

            args.OnUpdate?.Invoke(this, t);

            if (t >= 1.0f)
            {
                isMoving = false;
                args.OnEnded?.Invoke(this);

                Cancel();
            }
        }



        [Serializable]
        public class Params
        {
            public Coord Coord = Coord.XZ;
            public Vector3 ManualPerpendicular = Vector3.zero;
            public Vector3 ManualNegateVector = Vector3.one;
            public float Duration = .5f;
            [Range(0f, 1f)]
            public float Curvature = .5f;
            public Vector3 StartOffset = Vector3.zero;
            public float MidOffset = 0f;
            public Vector3 ManualMidOffset = Vector3.zero;
            public Vector3 EndOffset = Vector3.zero;
            public bool RandomizeSide = true;
            public bool RandomizeCurvuture = false;
            public bool ScreenBasedSide = false;
            public Ease Ease = Ease.Linear;
        }

        public struct Args
        {
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            // for local movement
            public Transform LocalToWorldContext;
            public Transform TargetTransform;
            public float? Delay;
            public Camera Camera;
            public Params Parameters;
            public CancellationToken? CancellationToken;
            public Action<Courier> OnStarted;
            public Action<Courier, float> OnUpdate;
            public Action<Courier> OnEnded;
        }
    }




    public static class CourierExtentions
    {
        const string k_CourierID = "k_Courier";

        public static Courier GetCourier()
        {
            return PoolCourier.Get(id: k_CourierID, CreateCourier, parent: null);
        }
        public static Courier CreateCourier(Transform parent)
        {
            return new GameObject(k_CourierID).AddComponent<Courier>();
        }

        public static void Move(this Courier.Args args)
        {
            var courier = GetCourier();
            courier.Move(args);
        }
        public static void Move(this Courier.Args args, Transform obj,
           Action<Courier> onStarted = null, Action<Courier> onEnded = null, Action<Courier, float> onUpdated = null)
        {
            var courier = GetCourier();

            Action<Courier> onstarted = (co) =>
            {
                obj.SetParent(co.transform);
                obj.transform.localPosition = Vector3.zero;
                onStarted?.Invoke(co);
            };

            Action<Courier> onended = (co) =>
            {
                obj.SetParent(null);
                onEnded?.Invoke(co);
            };

            args.OnStarted = onstarted;
            args.OnEnded = onended;
            args.OnUpdate = onUpdated;

            courier.Move(args);
        }

        public static void Move(this Courier.Args args, VFX vfx, VFX.Args vfxArgs, Transform obj,
            Action<Courier> onStarted = null, Action<Courier> onEnded = null, Action<Courier, float> onUpdate = null)
        {
            var courier = GetCourier();

            Action<Courier> onstarted = (co) =>
            {
                obj.SetParent(co.transform);
                obj.transform.localPosition = Vector3.zero;

                if (vfx != null)
                {
                    vfx.transform.SetParent(co.transform);
                    vfx.transform.localPosition = Vector3.zero;
                    vfx.OnStarted(vfxArgs);
                }

                onStarted?.Invoke(co);
            };

            Action<Courier> onended = (co) =>
            {
                if (vfx != null)
                {
                    vfx.transform.SetParent(null);
                    vfx.OnEnded();
                }

                obj.SetParent(null);
                onEnded?.Invoke(co);
            };

            args.OnStarted = onstarted;
            args.OnEnded = onended;
            args.OnUpdate = onUpdate;

            courier.Move(args);
        }


        /// <summary>       
        /// <para>
        /// rhs is the vector to caculate cross product with      
        /// </para>             
        /// </summary>      
        public static void CalculateCurvedControlPoints(Vector3 start, Vector3 end, Vector3 rhs, float curvature, out Vector3 p1, out Vector3 p2)
        {
            float distance = Vector3.Distance(start, end);
            float controlDist = distance * curvature;  // Adjust control distance based on curvature factor

            Vector3 midPoint = (start + end) / 2;  // Midpoint for reference
            Vector3 direction = (end - start).normalized;

            // Offset control points perpendicular to the line from start to end
            Vector3 perpendicular = Vector3.Cross(direction, rhs).normalized;

            p1 = midPoint + perpendicular * controlDist;
            p2 = midPoint - perpendicular * controlDist;
        }
        public static Vector3 GetCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
        }

        /// <summary>       
        /// <para>
        /// rhs is the vector to caculate cross product with      
        /// </para>             
        /// </summary>      
        public static Vector3 CalculateQuadraticControlPoint(Vector3 start, Vector3 end, Vector3 rhs, Vector3 manualPerpendicular,
            float curvature, Vector3 negateVector, Camera camera, Transform localToWorldContext)
        {
            float maxCurvature = curvature;
            float distance = Vector3.Distance(start, end);

            Vector3 midpoint = (start + end) / 2;
            Vector3 direction = (end - start).normalized;

            Vector3 perpendicular = manualPerpendicular;
            // Find a perpendicular direction for the control point to create an arc
            if (perpendicular == Vector3.zero)
                perpendicular = Vector3.Cross(direction, rhs).normalized;

            perpendicular.x *= negateVector.x;
            perpendicular.y *= negateVector.y;
            perpendicular.z *= negateVector.z;

            Vector3 controlPoint = midpoint + perpendicular * distance * curvature;

            if (camera == null)
                return controlPoint;

            while (true)
            {
                // Get the Bezier midpoint at t = 0.5
                Vector3 bezierMidpoint = GetQuadraticBezierPoint(0.5f, start, controlPoint, end);

                if (localToWorldContext != null) bezierMidpoint = localToWorldContext.TransformPoint(bezierMidpoint);

                Vector3 screenPoint = camera.WorldToScreenPoint(bezierMidpoint);

                // Check if the midpoint is within screen bounds
                if (screenPoint.x >= 0 && screenPoint.x <= Screen.width &&
                    screenPoint.y >= 0 && screenPoint.y <= Screen.height)
                {
                    // If it is within bounds, return the current control point
                    return controlPoint;
                }

                // Otherwise, decrease the curvature slightly to bring the midpoint closer
                maxCurvature *= 0.9f;
                controlPoint = midpoint + perpendicular * maxCurvature * Vector3.Distance(start, end);

                // Break if the curvature is too small (no suitable control point found within limits)
                if (maxCurvature < 0.001f)
                {
                    Debug.Log("Could not find a control point within screen bounds.");
                    return controlPoint;
                }
            }
        }

        public static Vector3 GetQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            return u * u * p0 + 2 * u * t * p1 + t * t * p2;
        }


        public static Vector3 MirrorAcrossPlane(Vector3 point, Vector3 planeNormal)
        {
            Vector3 n = planeNormal.normalized;
            return point - 2f * Vector3.Dot(point, n) * n;
        }

        public static float GetAdjustedMultiplier(Vector3 start, Vector3 end, Vector3 controlPoint, float multiplier)
        {
            Vector3 midpointPosition = GetQuadraticBezierPoint(0.5f, start, controlPoint, end);

            Camera cam = Camera.main;

            if (cam == null)
                return multiplier;

            Vector3 viewportPosition = cam.WorldToViewportPoint(midpointPosition);

            if (viewportPosition.x < 0 || viewportPosition.x > 1 || viewportPosition.y < 0 || viewportPosition.y > 1)
                return multiplier * -1;

            return multiplier;
        }

    }
}
