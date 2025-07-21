using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using TMPro;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data;


#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif


namespace MadApper
{


    public interface ICreatableSO
    {
        public string UID { get; set; }
    }

    public interface IResetOnExitPlaySO
    {
        public void ResetOnExitPlay();
    }

    public interface IDontDestoryOnLoad
    {
        public void OnInstantiated(GameObject go)
        {
            Object.DontDestroyOnLoad(go);
        }
    }
    public interface ISetBased
    {
        public const string k_Control = "Control";
        public string SetID { get; set; }
    }


    public struct VirtualGridExtends
    {
        public Vector2 Center;
        public Vector2 TotalSize;
        public Vector2 TopRight;
        public Vector2 TopLeft;
        public Vector2 BottomRight;
        public Vector2 BottomLeft;
        public float XDifference;
        public float YDifference;
    }

    public static class MADUtility
    {
        #region RuntimeInitializeOnLoadMethod

        public static T TryLoadAndInstantiate<T>(string prefabName) where T : MonoBehaviour
        {
#if UNITY_EDITOR
            var re = Object.FindObjectOfType<T>();

            if (re != null)
            {
                return null;
            }
#endif

            var load = Resources.Load<T>(prefabName);

            if (load == null)
            {
                $"couldnt find {prefabName}!".Log();
                return null;
            }

            var obj = Object.Instantiate(load);

            obj.name = $"MAD-{prefabName}";

            if (obj is IDontDestoryOnLoad iDontDestoryOnLoad)
                iDontDestoryOnLoad.OnInstantiated(obj.gameObject);

            return obj;
        }

        #endregion

        #region Matrix
        public static bool[,] RotateMatrix(this bool[,] matrix)
        {
            var width = matrix.GetLength(1);
            var height = matrix.GetLength(0);

            bool[,] ret = new bool[width, height];

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    ret[i, j] = matrix[height - j - 1, i];
                }
            }

            return ret;
        }


        public static T[,] ConvertListOfListToMatrix<T>(this List<List<T>> list)
        {
            var cols = list[0].Count;
            var rows = list.Count;
            var res = new T[list[0].Count, list.Count];

            for (int col = 0; col < cols; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    res[col, row] = list[row][col];
                }
            }

            return res;
        }

        public static bool AreEqual(this bool[,] array1, bool[,] array2)
        {
            // Check if dimensions are the same
            if (array1.GetLength(0) != array2.GetLength(0) || array1.GetLength(1) != array2.GetLength(1))
            {
                return false;
            }

            // Compare each element
            for (int i = 0; i < array1.GetLength(0); i++)
            {
                for (int j = 0; j < array1.GetLength(1); j++)
                {
                    if (array1[i, j] != array2[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Math
        public static bool AreApproximatelyEqual(this float value1, float value2, float epsilon = 0.0001f)
        {
            return Mathf.Abs(value1 - value2) <= epsilon;
        }
        #endregion

        #region Enum
        public static TEnum GetNextEnum<TEnum>(this TEnum currentEnum) where TEnum : Enum
        {
            // Get all values of the enum
            TEnum[] states = (TEnum[])Enum.GetValues(typeof(TEnum));

            // Find the index of the current state and go to the next one, looping back to 0 if needed
            int nextIndex = (Array.IndexOf(states, currentEnum) + 1) % states.Length;

            return states[nextIndex];
        }
        #endregion

        #region Layers

        public static void SetLayerRecursive(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;

            foreach (Transform child in gameObject.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }
        public static void SetLayerRecursive(this GameObject gameObject, int layer, ref List<GameObject> gos)
        {
            if (gos == null)
                gos = new List<GameObject>();

            gameObject.layer = layer;

            gos.Add(gameObject);

            foreach (Transform child in gameObject.transform)
            {
                SetLayerRecursive(child.gameObject, layer, ref gos);
            }
        }
        public static bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            if (((1 << layer) & layerMask) != 0)
                return true;

            return false;
        }
        public static int ToLayer(int bitmask)
        {
            int result = bitmask > 0 ? 0 : 31;
            while (bitmask > 1)
            {
                bitmask = bitmask >> 1;
                result++;
            }
            return result;
        }

        public static LayerMask GetLayerMaskFromGameObject(this GameObject obj)
        {
            return 1 << obj.layer;
        }
        #endregion

        #region GetRandom

        public static T GetRandomEnum<T>()
        {
            Array values = Enum.GetValues(typeof(T));

            return (T)values.GetValue(Random.Range(0, values.Length));
        }

        public static T GetRandom<T>(this List<T> list)
        {
            return list[Random.Range(0, list.Count)];
        }
        public static T GetRandom<T>(this T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        public static T GetRandomBetween<T>(T a, T b)
        {
            return Random.Range(0, 2) > 0 ? a : b;
        }


        #endregion

        #region Raycast

        private static Camera mainCam;

        public static Camera s_MainCam
        {
            get
            {
                if (mainCam == null)
                    mainCam = Camera.main;

                return mainCam;
            }
        }

        public static Vector3 GetRayCastWorldPosition(Camera camera, LayerMask layerMask, Vector3 toRayPos, out bool hitted)
        {
            Ray ray = camera.ScreenPointToRay(toRayPos);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, layerMask))
            {
                hitted = true;
                return raycastHit.point;
            }
            else
            {
                hitted = false;
                return Vector3.zero;
            }
        }
        public static Vector3 GetRayCastWorldPosition(Camera camera, LayerMask layerMask, Vector3 toRayPos, out RaycastHit rayHit)
        {
            rayHit = default;
            Ray ray = camera.ScreenPointToRay(toRayPos);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, layerMask))
            {
                rayHit = raycastHit;
                return raycastHit.point;
            }
            else
                return Vector3.zero;

        }
        public static Vector3 GetRayCastWorldPosition(Camera camera, LayerMask layerMask, Vector3 toRayPos, out bool hitted, out RaycastHit rayHit)
        {
            rayHit = default;
            Ray ray = camera.ScreenPointToRay(toRayPos);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, layerMask))
            {
                hitted = true;
                rayHit = raycastHit;
                return raycastHit.point;
            }
            else
            {
                hitted = false;
                return Vector3.zero;
            }
        }
        public static int GetRayCast_2D(Camera camera, LayerMask layerMask, Vector3 toRayPos, RaycastHit2D[] results)
        {
            Ray ray = camera.ScreenPointToRay(toRayPos);
            return Physics2D.GetRayIntersectionNonAlloc(ray, results, 999f, layerMask);
        }

        public static Vector3 GetRayCastWorldPosition(Camera camera, LayerMask layerMask, Vector3 toRayPos, out bool hitted, out RaycastHit rayHit, out Ray ray)
        {
            rayHit = default;
            ray = camera.ScreenPointToRay(toRayPos);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, layerMask))
            {
                hitted = true;
                rayHit = raycastHit;
                return raycastHit.point;
            }
            else
            {
                hitted = false;
                return Vector3.zero;
            }
        }
        public static RaycastHit GetRayCastHit(Camera camera, LayerMask layerMask, Vector3 toRayPos, out bool hitted)
        {
            Ray ray = camera.ScreenPointToRay(toRayPos);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, layerMask))
            {
                hitted = true;
                return raycastHit;
            }
            else
            {
                hitted = false;
                return default;
            }
        }

        public static Vector3 GetRayCastWorldPosition(this Camera camera, LayerMask layerMask, Vector3 toRayPos, out RaycastHit rayHit, out Ray ray)
        {
            rayHit = default;
            ray = camera.ScreenPointToRay(toRayPos);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, layerMask))
            {
                rayHit = raycastHit;
                return raycastHit.point;
            }
            else
            {
                return Vector3.zero;
            }
        }

        public static Vector3 GetSphereCastWorldPosition(this Camera camera, LayerMask layerMask, Vector3 toRayPos, float radius, out RaycastHit rayHit, out Ray ray)
        {
            rayHit = default;
            ray = camera.ScreenPointToRay(toRayPos);

            var hits = Physics.SphereCastAll(ray, radius, 999f, layerMask);

            if (hits != null && hits.Length > 0)
            {
                var hDistanc = hits.OrderBy(x => Vector3.Distance(x.point, toRayPos));
                rayHit = hDistanc.ElementAt(0);
                return rayHit.point;
            }
            else
            {
                return Vector3.zero;
            }
        }
        public static Vector3 GetSphereCastWorldPosition(this Camera camera, LayerMask layerMask, Vector3 toRayPos, float radius,
            out RaycastHit rayHit, out Ray ray, RaycastHit[] hitBuffer)
        {
            rayHit = default;
            ray = camera.ScreenPointToRay(toRayPos);

            int hitCount = Physics.SphereCastNonAlloc(ray, radius, hitBuffer, 999f, layerMask);

            if (hitCount > 0)
            {
                // Find the closest hit to the screen point
                RaycastHit closestHit = hitBuffer[0];
                float closestDistance = Vector3.Distance(closestHit.point, toRayPos);

                for (int i = 1; i < hitCount; i++)
                {
                    float dist = Vector3.Distance(hitBuffer[i].point, toRayPos);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestHit = hitBuffer[i];
                    }
                }

                rayHit = closestHit;
                return rayHit.point;
            }
            else
            {
                return Vector3.zero;
            }
        }

        /// <summary>
        /// buffer should be big enough!
        /// </summary>
        public static void RayCastNonAlloc(Vector3 origin, Vector3 direction,
            RaycastHit[] hitBuffer, float maxDistance, LayerMask layerMask, out RaycastHit rayHit)
        {
            rayHit = default;

            int hitCount = Physics.RaycastNonAlloc(origin, direction, hitBuffer, maxDistance, layerMask);

            if (hitCount > 0)
            {
                Array.Sort(hitBuffer, 0, hitCount, s_RaycastHitComparer);
                rayHit = hitBuffer[0];
            }
        }

        static RaycastHitDistanceComparer s_RaycastHitComparer = new();
        class RaycastHitDistanceComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit a, RaycastHit b)
            {
                return a.distance.CompareTo(b.distance);
            }
        }

        #endregion

        #region ClickedOnUI

        private static readonly List<RaycastResult> s_RayResults = new List<RaycastResult>(16);
        private static readonly PointerEventData s_PointerEventData = new PointerEventData(EventSystem.current);
        private static readonly Dictionary<int, PointerEventData> s_PointerEventCache = new Dictionary<int, PointerEventData>();

        public static bool IsClickedOnRect(this Vector2 screenPoint)
        {
            return IsTouchingUI(screenPoint);
        }
        public static bool IsClickedOnRect(this Vector2 screenPoint, int pointerId)
        {
            return IsTouchingUI(screenPoint, pointerId);
        }

        public static bool IsTouchingUI(Vector2 screenPoint)
        {
            if (EventSystem.current == null)
                return false;

            s_PointerEventData.position = screenPoint;
            s_RayResults.Clear();

            EventSystem.current.RaycastAll(s_PointerEventData, s_RayResults);

            return s_RayResults.Count > 0;
        }

        public static bool IsTouchingUI(Vector2 screenPoint, int pointerId = 0)
        {
            if (EventSystem.current == null)
                return false;

            if (!s_PointerEventCache.TryGetValue(pointerId, out var pointerEventData))
            {
                pointerEventData = new PointerEventData(EventSystem.current);
                s_PointerEventCache[pointerId] = pointerEventData;
            }

            pointerEventData.position = screenPoint;
            pointerEventData.pointerId = pointerId;

            s_RayResults.Clear();
            EventSystem.current.RaycastAll(pointerEventData, s_RayResults);

            return s_RayResults.Count > 0;
        }

        public static bool IsClickedOnRect(List<RectTransform> Rects, Vector2 screenPoint, Camera cam = null)
        {
            foreach (var rect in Rects)
            {
                if (rect.gameObject.activeInHierarchy
                    && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, cam))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsClickedOnRect(RectTransform Rect, Vector2 screenPoint, Camera cam = null)
        {
            if (Rect.gameObject.activeInHierarchy
                 && RectTransformUtility.RectangleContainsScreenPoint(Rect, screenPoint, cam))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Vector
        public static bool IsVector2ApproximatelyEqual(this Vector2 v1, Vector2 v2, float tolerance = 0.0001f)
        {
            return (v1 - v2).sqrMagnitude < tolerance * tolerance;
        }
        public static Vector3 ClampVector(this Vector3 vector, float minMagnitude, float maxMagnitude)
        {
            float sqrMagnitude = vector.sqrMagnitude;
            if (sqrMagnitude > (maxMagnitude * maxMagnitude))
                return vector.normalized * maxMagnitude;
            else if (sqrMagnitude < (minMagnitude * minMagnitude) && sqrMagnitude > 0)
                return vector.normalized * minMagnitude;
            else
                return vector;
        }
        public static Vector3 ClampVectorMin(this Vector3 vector, float minMagnitude)
        {
            float sqrMagnitude = vector.sqrMagnitude;
            if (sqrMagnitude < (minMagnitude * minMagnitude) && sqrMagnitude > 0)
                return vector.normalized * minMagnitude;
            else
                return vector;
        }
        public static Vector3 ClampVectorMax(this Vector3 vector, float maxMagnitude)
        {
            float sqrMagnitude = vector.sqrMagnitude;
            if (sqrMagnitude > (maxMagnitude * maxMagnitude))
                return vector.normalized * maxMagnitude;
            else
                return vector;
        }
        /// <summary>
        /// Sets any x y z values of a Vector3
        /// </summary>
        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        }

        /// <summary>
        /// Adds to any x y z values of a Vector3
        /// </summary>
        public static Vector3 Add(this Vector3 vector, float x = 0, float y = 0, float z = 0)
        {
            return new Vector3(vector.x + x, vector.y + y, vector.z + z);
        }

        /// <summary>
        /// Returns a Boolean indicating whether the current Vector3 is in a given range from another Vector3
        /// </summary>
        /// <param name="current">The current Vector3 position</param>
        /// <param name="target">The Vector3 position to compare against</param>
        /// <param name="range">The range value to compare against</param>
        /// <returns>True if the current Vector3 is in the given range from the target Vector3, false otherwise</returns>
        public static bool InRangeOf(this Vector3 current, Vector3 target, float range)
        {
            return (current - target).sqrMagnitude <= range * range;
        }

        public static Vector3 GetPos_XY(Vector3 center, float radius, float angle)
        {
            Vector3 pos;
            pos.x = center.x + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            pos.y = center.y + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            pos.z = center.z;
            return pos;
        }

        public static Vector3 GetPos_XZ(Vector3 center, float angle)
        {
            Vector3 pos;
            pos.x = center.x + Mathf.Sin(angle * Mathf.Deg2Rad);
            pos.z = center.z + Mathf.Cos(angle * Mathf.Deg2Rad);
            pos.y = center.y;
            return pos;
        }
        public static Vector3 GetPos_XZ(Vector3 center, float radius, float angle)
        {
            Vector3 pos;
            pos.x = center.x + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            pos.z = center.z + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            pos.y = center.y;
            return pos;
        }
        public static Vector3 GetVectorFromAngle(int angle)
        {
            // angle = 0 -> 360
            float angleRad = angle * (Mathf.PI / 180f);
            return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }
        public static int GetAngleFromVector_XZ(Vector3 dir)
        {
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360;
            int angle = Mathf.RoundToInt(n);

            return angle;
        }

        public static Quaternion GetPerpendicularRotation(this Vector3 dir, Vector3 rhs)
        {
            var perpendicular = Vector3.Cross(dir, rhs);
            return Quaternion.LookRotation(perpendicular);
        }
        public static Vector2 GetTotalSizeOfGrid(Vector2Int dimensions, Vector2 cellSize, Vector2 padding)
        {
            return (cellSize * (Vector2)dimensions) + (padding * (Vector2)(dimensions - Vector2Int.one));
        }
        public static Vector2 GetCenter(Vector2Int dimensions, Vector2 cellSize, Vector2 padding)
        {
            Vector2 totalSize = GetTotalSizeOfGrid(dimensions, cellSize, padding);
            return (totalSize / 2) - (cellSize / 2);
        }
        public static VirtualGridExtends GetVirtualGridExtends(Vector2Int dimensions, Vector2 cellSize, Vector2 padding)
        {
            Vector2 totalSize = GetTotalSizeOfGrid(dimensions, cellSize, padding);
            Vector2 center = (totalSize / 2) - (cellSize / 2);
            Vector2 halfSize = totalSize / 2;
            Vector2 topLeft = new Vector2(center.x - halfSize.x + (cellSize.x / 2), center.y + halfSize.y - (cellSize.y / 2));
            Vector2 topRight = new Vector2(center.x + halfSize.x - (cellSize.x / 2), center.y + halfSize.y - (cellSize.y / 2));
            Vector2 bottomLeft = new Vector2(center.x - halfSize.x + (cellSize.x / 2), center.y - halfSize.y + (cellSize.y / 2));
            Vector2 bottomRight = new Vector2(center.x + halfSize.x - (cellSize.x / 2), center.y - halfSize.y + (cellSize.y / 2));

            float totalWidth = Mathf.Abs(topRight.x - topLeft.x);
            float totalHeight = Mathf.Abs(topLeft.y - bottomLeft.y);

            return new VirtualGridExtends()
            {
                TotalSize = totalSize,
                Center = center,
                TopLeft = topLeft,
                TopRight = topRight,
                BottomLeft = bottomLeft,
                BottomRight = bottomRight,
                XDifference = totalWidth,
                YDifference = totalHeight
            };
        }

        public static int ManhattanDistance(this Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }


        #endregion

        #region WorldText


        public const int sortingOrderDefault = 5000;

        public static TextMeshPro CreateWorldText(this string text, Transform parent = null, Vector3 localPosition = default(Vector3), Quaternion localRotation = default(Quaternion),
            float fontSize = 40, Color? color = null, TMP_FontAsset font = null, TextAlignmentOptions textAlignment = TextAlignmentOptions.Center, int sortingOrder = sortingOrderDefault)
        {
            if (color == null) color = Color.white;
            return CreateWorldText(parent, text, localPosition, localRotation, fontSize, (Color)color, font, textAlignment, sortingOrder);
        }
        public static TextMeshPro CreateWorldText(this Transform parent, string text, Vector3 localPosition, Quaternion localRotation,
            float fontSize, Color color, TMP_FontAsset font, TextAlignmentOptions textAlignment, int sortingOrder)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
            if (font != null)
                textMesh.font = font;
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            RectTransform rectT = gameObject.GetComponent<RectTransform>();
            rectT.sizeDelta = new Vector2(1f, 0f);

            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            return textMesh;
        }
        #endregion

        #region Scree/World Space

        public static void ConvertScreenToWorld(this List<Transform> @in, Camera camera, out List<Vector3> @out)
        {
            @out = new List<Vector3>();

            foreach (var item in @in)
            {
                if (item == null)
                    continue;

                var pos = camera.ScreenToWorldPoint(item.position);
                @out.Add(pos);
            }
        }

        public static void ConvertWordToScreen(this List<Transform> @in, Camera camera, out List<Vector3> @out)
        {
            @out = new List<Vector3>();

            foreach (var item in @in)
            {
                if (item == null)
                    continue;

                var pos = camera.WorldToScreenPoint(item.position);
                @out.Add(pos);
            }
        }

        #endregion

        #region GetCoreners of Screen

        public static Vector3 GetBottomLeft(this Camera camera)
        {
            return camera.ScreenToWorldPoint(new Vector3(0, 0, -camera.transform.position.z));
        }
        public static Vector3 GetBottomLeft(this Camera camera, float z)
        {
            return camera.ScreenToWorldPoint(new Vector3(0, 0, -z));
        }
        public static Vector3 GetTopRight(this Camera camera)
        {
            return camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, -camera.transform.position.z));
        }
        public static Vector3 GetTopRight(this Camera camera, float z)
        {
            return camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, -z));
        }
        public static Vector3 GetCenter(this Camera camera)
        {
            return camera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, -camera.transform.position.z));
        }
        #endregion

        #region CanvasGroup
        public static void ActiveCG(this CanvasGroup canvasGroup, bool active, float time, bool changeGOactivation = false, bool forceInitAlpha = false)
        {
            if (time > 0)
            {
                if (forceInitAlpha && active)
                    canvasGroup.alpha = 0;

                canvasGroup.gameObject.SetActive(true);
                //canvasGroup.DOFade(active ? 1 : 0, time).OnComplete(() =>
                //{
                //    canvasGroup.interactable = active;
                //    canvasGroup.blocksRaycasts = active;

                //    if (changeGOactivation)
                //        canvasGroup.gameObject.SetActive(active);
                //});
            }
            else
            {
                canvasGroup.alpha = active ? 1 : 0;
                canvasGroup.interactable = active;
                canvasGroup.blocksRaycasts = active;

                if (changeGOactivation)
                    canvasGroup.gameObject.SetActive(active);
            }
        }

        #endregion

        #region IEnumerables


        public static bool AreEqual<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            if (list1 == null && list2 == null) return false;
            if (list1 == null || list2 == null) return true;

            return list1.Count() == list2.Count() && list1.All(list2.Contains);
        }

        public static int IndexOf<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
        {
            for (int i = 0; i < dictionary.Count; ++i)
            {
                if (dictionary.ElementAt(i).Value.Equals(value))
                    return i;
            }
            return -1;
        }

        public static bool ContainClass<T, G>(this List<T> list)
        {
            return list.OfType<G>().FirstOrDefault() != null;
        }

        public static bool ContainClass<T, G>(this IEnumerable<T> list)
        {
            return list.OfType<G>().FirstOrDefault() != null;
        }
        public static bool ContainClass<T, G>(this List<T> list, G item)
        {
            var types = list.Select(x => x.GetType());
            return types.Any(type => type.IsAssignableFrom(item.GetType()));
        }
        public static G GetContainClass<T, G>(this List<T> list, G item) where G : class
        {
            if (list == null || list.Count <= 0)
                return null;

            var types = list.Select(x => x.GetType());
            var find = types.FirstOrDefault(type => type.IsAssignableFrom(item.GetType()));
            if (find != null)
            {
                var index = types.ToList().IndexOf(find);

                if (index >= 0 && index < list.Count)
                    return list[index] as G;
            }
            return null;
        }

        public static List<T> Shuffle<T>(this List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                T temp = list[i];
                int randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }

            return list;
        }


        public static List<List<T>> GroupDiamondOutward<T>(IEnumerable<T> list, Vector2Int startKey, Func<T, Vector2Int> keySelector)
        {
            return list
                .GroupBy(item => Mathf.Abs(keySelector(item).x - startKey.x) + Mathf.Abs(keySelector(item).y - startKey.y)) // Manhattan distance
                .OrderBy(group => group.Key) // Sort by increasing distance
                .Select(group => group.ToList()) // Convert groups to lists
                .ToList();
        }
        public static List<List<T>> GroupRows<T>(IEnumerable<T> list, Func<T, Vector2Int> keySelector)
        {
            return list
                .GroupBy(item => keySelector(item).y)
                .OrderBy(group => group.Key)
                .Select(group => group.ToList())
                .ToList();
        }


        public static T GetItemWhileLooped<T>(this List<T> list, int index, int loopPoint) where T : class
        {
            if (list == null || !list.Any()) return null;

            if (index < list.Count)
                return list[index];

            var loopPointIndex = loopPoint - 1;
            int loopRange = list.Count - loopPointIndex;

            if (loopRange <= 0)
            {
                $"LoopPoint must be less than List count. at Index {index}".LogWarning();
                return list[0];
            }

            int adjustedIndex = loopPointIndex + ((index - list.Count) % loopRange);
            return list[adjustedIndex];
        }
        public static T GetItemWhileLooped<T>(this List<T> list, int index, int loopPoint, IEnumerable<int> excludedIndexes) where T : class
        {
            if (list == null || !list.Any()) return null;

            if (index < list.Count) return list[index];

            if (loopPoint < 0 || loopPoint >= list.Count)
            {
                $"LoopPoint {loopPoint} must be within list bounds (0 to {list.Count - 1}). Index: {index}".LogWarning();
                return list[0];
            }

            var loopIndexes = Enumerable
                .Range(loopPoint, list.Count - loopPoint)
                .Where(i => excludedIndexes == null || !excludedIndexes.Contains(i))
                .ToList();

            if (loopIndexes.Count == 0)
            {
                $"No valid loop indexes after applying exclusions in range {loopPoint} to {list.Count - 1}".LogWarning();
                return list[0];
            }

            int loopOffset = (index - list.Count) % loopIndexes.Count;
            if (loopOffset < 0) loopOffset += loopIndexes.Count;

            int adjustedIndex = loopIndexes[loopOffset];
            return list[adjustedIndex];
        }

        public static (TKey key, int index)? FindKeyAndIndex<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TValue target)
        {
            foreach (var kvp in dict)
            {
                int index = kvp.Value.IndexOf(target);
                if (index != -1)
                {
                    return (kvp.Key, index);
                }
            }
            return null; // Not found
        }


        public static List<List<T>> GetSplits<T>(this IEnumerable<T> list, IEnumerable<int> sizes)
        {
            List<List<T>> allSubLists = new List<List<T>>();

            foreach (var size in sizes)
            {
                for (int i = 0; i <= list.Count() - size; i++)
                {
                    allSubLists.Add(list.Skip(i).Take(size).ToList());
                }
            }

            return allSubLists;
        }

        public static List<List<(T Value, int Index)>> GetSplitsWithIndex<T>
            (this IEnumerable<T> list, IEnumerable<int> sizes, IEnumerable<T> ignores = null)
        {
            var listWithIndexes = list
               .Select((value, index) => (Value: value, Index: index))
               .Where(item => ignores == null || !ignores.Contains(item.Value))
               .ToList();

            List<List<(T Value, int Index)>> allSubLists = new List<List<(T Value, int Index)>>();

            foreach (var size in sizes)
            {
                for (int i = 0; i <= listWithIndexes.Count - size; i++)
                {
                    allSubLists.Add(listWithIndexes.Skip(i).Take(size).ToList());
                }
            }

            return allSubLists;
        }

        public static IEnumerable<A> MatchByUID<A, B>(this List<A> listA, List<B> listB, Func<A, string> aKey, Func<B, string> bKey)
        {
            var bUIDs = new HashSet<string>(listB.Select(bKey));
            return listA.Where(a => bUIDs.Contains(aKey(a)));
        }
        public static IEnumerable<A> MatchByUIDWithDuplicatesDictBased<A, B>(this IEnumerable<A> listA, List<B> listB, Func<A, string> aKey, Func<B, string> bKey)
        {
            var aDict = listA.ToDictionary(aKey, a => a); // Create a dictionary for fast lookup
            return listB.Select(b => aDict.TryGetValue(bKey(b), out var match) ? match : default);
        }

        public static IEnumerable<A> MatchByUIDWithDuplicatesGroupBased<A, B>(this IEnumerable<A> listA, List<B> listB, Func<A, string> aKey, Func<B, string> bKey)
        {
            // Group listA items by UID, keeping multiple values in lists
            var aDict = listA.GroupBy(aKey).ToDictionary(g => g.Key, g => g.ToList());
            // Select only the first match per UID in listB
            return listB.Select(b => aDict.TryGetValue(bKey(b), out var matches) ? matches.FirstOrDefault() : default);
        }


        public static void SortByTrailingNumber<T>(this List<T> list, Func<T, string> stringSelector)
        {
            list.Sort((a, b) =>
            {
                int numA = ExtractLastNumber(stringSelector(a));
                int numB = ExtractLastNumber(stringSelector(b));
                return numA.CompareTo(numB);
            });
        }


        #endregion

        #region Find
        public static Transform FindRecursive(this Transform t, string target)
        {
            var associate = t.Find(target);

            if (associate != null)
                return associate;

            foreach (Transform child in t)
            {
                associate = child.FindRecursive(target);

                if (associate != null)
                    return associate;
            }

            return null;
        }
        public static IEnumerable<T> FindInterfaces<T>()
        {
            return Object.FindObjectsOfType<MonoBehaviour>(true).OfType<T>();
        }
        public static T FindInterface<T>()
        {
            var list = FindInterfaces<T>();
            if (list != null && list.Count() > 0)
                return list.First();
            return default;
        }
        #endregion

        #region Get_Runtime

        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class, IComparable<T>
        {
            List<T> objects = new List<T>();
            foreach (Type type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                objects.Add((T)Activator.CreateInstance(type, constructorArgs));
            }
            objects.Sort();
            return objects;
        }



        public static IEnumerable<T> GetAllDerivedInstances<T>()
        {
            return typeof(T)
                 .Assembly.GetTypes()
                 .Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract)
                 .Select(t => (T)Activator.CreateInstance(t));
        }
        public static IEnumerable<T> GetAllScriptableInstances<T>(this IEnumerable<T> list) where T : ScriptableObject
        {
            return typeof(T)
                 .Assembly.GetTypes()
                 .Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract)
                 .Select(t => (T)ScriptableObject.CreateInstance(t));
        }
        public static IEnumerable<T> GetAllDerivedInstances<T>(this IEnumerable<T> list)
        {
            return typeof(T)
                 .Assembly.GetTypes()
                 .Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract)
                 .Select(t => (T)Activator.CreateInstance(t));
        }
        public static IEnumerable<T> GetAllDerivedInstances<T>(this Type type)
        {
            return type.Assembly.GetTypes()
                 .Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract && !t.IsGenericType)
                 .Select(t => (T)Activator.CreateInstance(t));
        }


        public static IEnumerable<T> GetAllDerivedInstancesInAllAssemblies<T>()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var res = new List<T>();

            foreach (var assembly in assemblies)
            {
                res.AddRange(assembly
                    .GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract && !t.IsGenericType)
                    .Select(t => (T)Activator.CreateInstance(t)));
            }

            return res;
        }

        public static IEnumerable<T> GetAllDerivedInstancesInAllAssemblies<T>(this Object obj)
        {
            return GetAllDerivedInstancesInAllAssemblies<T>();
        }



        #endregion

        #region Copy

        public static void CopyTo(this object source, object target)
        {
            var type = source.GetType();
            var targettype = target.GetType();

            if (type != targettype)
                return;

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                field.SetValue(target, field.GetValue(source));
            }
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (prop.CanWrite)
                {
                    prop.SetValue(target, prop.GetValue(source));
                }
            }
        }
        public static void CopyTo<T>(this T source, T target)
        {
            var type = typeof(T);

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var value = field.GetValue(source);
                field.SetValue(target, TryClone(value, field.FieldType));
            }

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (prop.CanWrite && prop.CanRead)
                {
                    var value = prop.GetValue(source);
                    prop.SetValue(target, TryClone(value, prop.PropertyType));
                }
            }
        }
        private static object TryClone(object value, Type memberType)
        {
            if (value == null) return null;

            // Handle List<T>
            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var listType = memberType.GetGenericArguments()[0];
                var listClone = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));
                foreach (var item in (IEnumerable)value)
                {
                    listClone.Add(item); // shallow copy of list contents
                }
                return listClone;
            }

            // Leave other reference types shallow for now
            return value;
        }
        public static void CopyShallowTo(this Object src, Object dst, List<string> ignores = null)
        {
            var srcT = src.GetType();
            var dstT = dst.GetType();
            foreach (var f in srcT.GetFields())
            {
                if (Ignore(f.Name, ignores))
                    continue;

                var dstF = dstT.GetField(f.Name);

                if (dstF == null || dstF.IsLiteral)
                    continue;
                dstF.SetValue(dst, f.GetValue(src));
            }

            foreach (var f in srcT.GetProperties())
            {
                if (Ignore(f.Name, ignores))
                    continue;

                var dstF = dstT.GetProperty(f.Name);
                if (dstF == null)
                    continue;
                if (dstF.Name.Equals("name"))
                    continue;
                dstF.SetValue(dst, f.GetValue(src, null), null);
            }

            bool Ignore(string name, List<string> ignores)
            {
                if (ignores == null)
                    return false;

                if (ignores.Contains(name))
                    return true;

                return false;
            }
        }

        public static T DeepCopy<T>(this T source)
        {
            return (T)DeepCopyInternal(source, new Dictionary<object, object>());
        }

        private static object DeepCopyInternal(object source, Dictionary<object, object> visited)
        {
            if (source == null) return null;

            Type type = source.GetType();

            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type.IsValueType)
                return source;

            if (IsUnityObject(type))
                return source; // ?? Keep original Unity reference (or skip)

            if (visited.TryGetValue(source, out var existingCopy))
                return existingCopy;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = type.GetGenericArguments()[0];
                var listCopy = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                visited[source] = listCopy;

                foreach (var item in (IEnumerable)source)
                {
                    listCopy.Add(DeepCopyInternal(item, visited));
                }

                return listCopy;
            }

            var copy = CreateInstance(type); // Use hybrid method
            visited[source] = copy;

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                //if (IsUnityObject(field.FieldType)) continue;
                var value = field.GetValue(source);
                var copied = IsUnityObject(field.FieldType) ? value : DeepCopyInternal(value, visited);
                field.SetValue(copy, copied);
            }

            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!prop.CanRead || !prop.CanWrite || IsUnityObject(prop.PropertyType)) continue;
                var value = prop.GetValue(source);
                prop.SetValue(copy, DeepCopyInternal(value, visited));
            }

            return copy;


            object CreateInstance(Type type)
            {
                try
                {
                    return Activator.CreateInstance(type); // try standard
                }
                catch
                {
                    // fallback for no default constructor
                    return System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
                }
            }
        }
        private static bool IsUnityObject(this Type type)
        {
            return typeof(UnityEngine.Object).IsAssignableFrom(type);
        }
        #endregion

        #region String

        public static bool ContainsIgnoreCase(this string str, string substring) => str.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;

        private static int ExtractLastNumber(string input)
        {
            if (string.IsNullOrEmpty(input))
                return int.MaxValue;

            // Match the last number in the string
            var match = Regex.Match(input, @"(\d+)(?!.*\d)");
            if (match.Success && int.TryParse(match.Value, out int number))
                return number;

            return int.MaxValue;
        }
        //public static string ExtractNamePrefix(string input)
        //{
        //    if (string.IsNullOrWhiteSpace(input))
        //        return input;

        //    var match = Regex.Match(input, @"^(.*?)(\d+)(?!.*\d)");
        //    return match.Success ? match.Groups[1].Value.TrimEnd() : input;
        //}
        public static string ExtractNamePrefix(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Match the last group of digits at the end of the string
            var match = Regex.Match(input, @"^(.*?)(?:[_\s-]?)(\d+)$");
            return match.Success ? match.Groups[1].Value : input;
        }
        #endregion

        #region Reflections

        public static void SetValueViaReflection(this Type type, object classobject,
            string valuename, object newvalue, BindingFlags flags)
        {
            var filed = type.GetField(valuename, flags);

            if (filed != null)
            {
                // Set a new value
                filed.SetValue(classobject, newvalue);
            }
        }
        public static void SetValuesViaReflection(this Type type, object classobject,
           object newvalue, BindingFlags flags)
        {
            var fileds = type.GetFields(flags);

            if (fileds != null)
            {
                foreach (FieldInfo field in fileds)
                {
                    // Check if the field type is a class (excluding primitive types like int, string, etc.)
                    if (field.FieldType.IsNullable() && field.FieldType != typeof(string))
                    {
                        field.SetValue(classobject, newvalue);
                    }
                }
            }
        }

        public static void CallMethod(this Type type, string methodName, BindingFlags flags, object[] parameters)
        {
            MethodInfo method = type.GetMethod(methodName, flags);

            if (method != null)
            {
                // Invoke the method
                method.Invoke(null, parameters);
            }
            else
            {
                "Method not found!".LogWarning();
            }
        }



        #endregion

        #region DOTWEEN
        public static TweenerCore<Vector3, Vector3, VectorOptions> DOMoveInTargetLocalSpace(this Transform transform,
            Transform target,
            Vector3 targetLocalEndPosition,
            float duration)
        {
            var t = DOTween.To(
                () => transform.position - target.transform.position, // Value getter
                x => transform.position = x + target.transform.position, // Value setter
                targetLocalEndPosition,
                duration);
            t.SetTarget(transform);
            return t;
        }

        public static void AppendAction(this Sequence sequence, Action<Action> action)
        {
            Tweener tweener = DOTween.To(() => 0, x => { }, 1, 0.01f)
                              .OnStart(() =>
                              {
                                  sequence.Pause();
                                  action.Invoke(() => sequence.Play());
                              });


            sequence.Append(tweener);

            // just to make sure sequence paueses if there is no other tweener aftet this!
            sequence.Append(DOVirtual.DelayedCall(0.01f, null));

        }


        #endregion

        #region Actions

        public static Action<Action> GetAutoComlpete(this Action action)
        {
            return (x) => { action?.Invoke(); x?.Invoke(); };
        }

        #endregion

        #region Transform

        public static void ResetTransformToParent(this Transform target, Transform parent)
        {
            target.SetParent(parent);
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.Euler(Vector3.zero);
            target.localScale = Vector3.one;
        }


        public static (Vector3 LocalPivotPos, Vector3 WorldPivotPos) GetPivotFromBounds(
               this MeshFilter meshFilter,
               PivotType pivotType,
               PivotAxis pivotAxis,
               Vector3 customPivot = default)
        {
            // Get local-space bounds
            Bounds localBounds = meshFilter.sharedMesh.bounds;

            // Apply the object's local scale to the bounds extents
            Vector3 scaledExtents = Vector3.Scale(localBounds.extents, meshFilter.transform.localScale);

            // Initialize directional vectors (with applied scale)
            Vector3 topBottomDirection = Vector3.zero;
            Vector3 leftRightDirection = Vector3.zero;

            // Determine which axis defines top/bottom and left/right (with scale applied)
            switch (pivotAxis)
            {
                case PivotAxis.Forward:
                    topBottomDirection = meshFilter.transform.up * scaledExtents.y; // Top/Bottom along Up
                    leftRightDirection = meshFilter.transform.right * scaledExtents.x; // Left/Right along Right
                    break;

                case PivotAxis.Up:
                    topBottomDirection = meshFilter.transform.forward * scaledExtents.z; // Top/Bottom along Forward
                    leftRightDirection = meshFilter.transform.right * scaledExtents.x; // Left/Right along Right
                    break;

                case PivotAxis.Right:
                    topBottomDirection = meshFilter.transform.up * scaledExtents.y; // Top/Bottom along Up
                    leftRightDirection = meshFilter.transform.forward * scaledExtents.z; // Left/Right along Forward
                    break;

                default:
                    topBottomDirection = meshFilter.transform.up * scaledExtents.y; // Fallback to Up
                    leftRightDirection = meshFilter.transform.right * scaledExtents.x; // Fallback to Right
                    break;
            }

            // Calculate the world center of the bounds
            Vector3 worldCenter = meshFilter.transform.TransformPoint(localBounds.center);

            // Initialize the pivot position as the center of the bounds
            Vector3 worldPivot = worldCenter;

            // Compute pivot position based on pivotType
            switch (pivotType)
            {
                case PivotType.Center: worldPivot = worldCenter; break;
                case PivotType.Left: worldPivot = worldCenter - leftRightDirection; break;
                case PivotType.Right: worldPivot = worldCenter + leftRightDirection; break;
                case PivotType.TopCenter: worldPivot = worldCenter + topBottomDirection; break;
                case PivotType.TopLeft: worldPivot = worldCenter - leftRightDirection + topBottomDirection; break;
                case PivotType.TopRight: worldPivot = worldCenter + leftRightDirection + topBottomDirection; break;
                case PivotType.BottomCenter: worldPivot = worldCenter - topBottomDirection; break;
                case PivotType.BottomLeft: worldPivot = worldCenter - leftRightDirection - topBottomDirection; break;
                case PivotType.BottomRight: worldPivot = worldCenter + leftRightDirection - topBottomDirection; break;
                case PivotType.Custom: worldPivot = meshFilter.transform.TransformPoint(localBounds.min + Vector3.Scale(localBounds.size, customPivot)); break;
                default: worldPivot = worldCenter; break;
            }

            // Convert world pivot to local space
            Vector3 localPivot = meshFilter.transform.InverseTransformPoint(worldPivot);

            return (localPivot, worldPivot);
        }



        public static Vector3 GetScaleWithConstraint(this AxisConstraints constraint, Vector3 value, Vector3 scaler)
        {
            // Check each axis and update accordingly
            if ((constraint & AxisConstraints.X) == AxisConstraints.X)
                value.x = scaler.x; // Only scale X axis            

            if ((constraint & AxisConstraints.Y) == AxisConstraints.Y)
                value.y = scaler.y; // Only scale Y axis            

            if ((constraint & AxisConstraints.Z) == AxisConstraints.Z)
                value.z = scaler.z; // Only scale Z axis            

            return value;
        }


        #endregion

        #region Hash

        public static int FNV1aHash(this string str)
        {
            const int fnvPrime = 16777619;
            const int offsetBasis = unchecked((int)2166136261);

            int hash = offsetBasis;
            for (int i = 0; i < str.Length; i++)
            {
                hash ^= str[i];
                hash *= fnvPrime;
            }
            return hash;
        }
        #endregion

        #region ISetBased

        public static T GetValueOfSetBasedData<T>(this List<T> list, string setUD) where T : ISetBased
        {
            var res = list.Find(x => x.SetID.Equals(setUD, StringComparison.OrdinalIgnoreCase));

            if (res != null)
                return res;

            $"[{setUD}] {typeof(T)} is null! getting [{ISetBased.k_Control}] instead!".LogWarning();

            var control = list.Find(x => x.SetID.Equals(ISetBased.k_Control, StringComparison.OrdinalIgnoreCase));

            if (control == null)
                $"even [{ISetBased.k_Control}] {typeof(T)} is null!!".LogWarning();

            return control;
        }
        #endregion


        #region Editor


#if UNITY_EDITOR

        #region Path
        public static string GetPath(this Object obj) => AssetDatabase.GetAssetPath(obj)
               .Replace(obj.name + ".asset", "");

        public static string GetClassPath([System.Runtime.CompilerServices.CallerFilePath] string path = null) => path;


        static string k_EssentialsProjectPath = Path.Combine("Assets", "_Essentials_ProjectSpecific");
        static string k_EssentialsProjectResourcesPath = Path.Combine("Assets", "_Essentials_ProjectSpecific", "Resources");
        public static string GetEssentialsFolder()
        {
            MakeSureResourcesFolderExists();
            return k_EssentialsProjectPath;
        }
        public static string GetEssentialsResourceFolder()
        {
            MakeSureResourcesFolderExists();
            return k_EssentialsProjectResourcesPath;
        }
        static void MakeSureResourcesFolderExists()
        {
            if (!Directory.Exists(k_EssentialsProjectPath))
                Directory.CreateDirectory(k_EssentialsProjectPath);

            if (!Directory.Exists(k_EssentialsProjectResourcesPath))
                Directory.CreateDirectory(k_EssentialsProjectResourcesPath);
        }
        public static void EnsureFolderExists(this string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }


        public static void LoadPathsRecursive(string path, ref List<string> paths)
        {
            var fullPath = Path.Combine(Application.dataPath, path);

            DirectoryInfo dirInfo = new DirectoryInfo(fullPath);

            foreach (var file in dirInfo.GetFiles())
            {
                paths.Add(Path.Combine(path, file.Name));
            }

            foreach (var dir in dirInfo.GetDirectories())
            {
                LoadPathsRecursive(Path.Combine(path, dir.Name), ref paths);
            }
        }

        public static void GetAllDirectories(string path, ref List<string> dirs)
        {
            var rootPath = Path.Combine(Application.dataPath, path);

            GetAllDirectories_Recursive(rootPath, ref dirs);
        }
        public static void GetAllDirectories_Recursive(string path, ref List<string> dirs)
        {
            var ds = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

            foreach (var p in ds)
            {
                if (dirs.Contains(p))
                    continue;

                dirs.Add(p);

                GetAllDirectories_Recursive(p, ref dirs);
            }

        }

        #endregion

        #region Types

        public static void LogTypesInNameSpace(MonoBehaviour mono, string nameSpace)
        {
            Type[] typelist = GetTypesInNamespace(Assembly.GetExecutingAssembly(), nameSpace);

            for (int i = 0; i < typelist.Length; i++)
            {
                mono.Log(typelist[i].Name);
            }
        }

        public static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }

        #endregion

        #region Get_Editor

        public static IEnumerable<T> FindInterfacesSO_Editor<T>()
        {
            return GetAllInstances_Editor<ScriptableObject>().OfType<T>();
        }
        public static IEnumerable<T> FindInterfacesPrefab_Editor<T>()
        {
            return GetAllPrefabs_Editor<MonoBehaviour>().OfType<T>();
        }
        public static T[] GetAllInstances_Editor<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }
        public static T[] GetAllInstances_Editor<T>(this T[] obj) where T : Object
        {
            return GetAllInstances_Editor<T>();
        }
        public static T[] GetAllInstances_Editor<T>(Object inThisObjectsDirectory) where T : Object
        {
            string assetPath = AssetDatabase.GetAssetPath(inThisObjectsDirectory);
            string directory = Path.GetDirectoryName(assetPath);

            return GetAllInstances_Editor<T>(directory);
        }
        public static T[] GetAllInstances_Editor<T>(string directory) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { directory });
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }
        public static T[] GetAllInstances_Editor<T>(this T[] obj, string directory) where T : Object
        {
            return GetAllInstances_Editor<T>(directory);
        }

        public static List<T> GetAllPrefabs_Editor<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            List<T> a = new List<T>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var loaded = AssetDatabase.LoadAssetAtPath<T>(path);
                if (loaded is T)
                    a.Add(loaded);
            }

            return a;
        }
        public static List<T> GetAllPrefabs_Editor<T>(string directory) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { directory });
            List<T> a = new List<T>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var loaded = AssetDatabase.LoadAssetAtPath<T>(path);
                if (loaded is T)
                    a.Add(loaded);
            }

            return a;
        }


        public static T FindPrefabWithName<T>(this string name) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets(name + " t:Prefab");

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<T>(path);

                if (prefab == null || !string.Equals(prefab.name, name, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (prefab is T)
                    return prefab;
            }

            return null;
        }


        public static T GetSO<T>(List<T> all, string id) where T : ICreatableSO
        {
            return all.Find(x => x.UID.Equals(id, comparisonType: StringComparison.OrdinalIgnoreCase));
        }

        public static T GetOrCreateSO<T>(ref List<T> all, string id, string path) where T : ScriptableObject, ICreatableSO
        {
            var so = all.Find(x => x.UID.Equals(id, comparisonType: StringComparison.OrdinalIgnoreCase));

            if (so != null)
                return so;

            var createSO = CreateSO<T>(id, path);
            createSO.UID = id;
            all.Add(createSO);
            EditorUtility.SetDirty(createSO);
            AssetDatabase.SaveAssets();
            return createSO;
        }

        public static T GetOrCreateSONew<T>(string id, string path) where T : ScriptableObject, ICreatableSO
        {
            var res = GetAssetByID<T>(id);

            if (res != null)
                return res;

            var createSO = CreateSO<T>(id, path);
            createSO.UID = id;
            EditorUtility.SetDirty(createSO);
            AssetDatabase.SaveAssets();
            return createSO;
        }

        public static T GetAssetByID<T>(string uid) where T : Object, ICreatableSO
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            if (guids != null && guids.Length > 0)
            {
                foreach (var guid in guids)
                {
                    if (string.IsNullOrEmpty(guid))
                        continue;

                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    T asset = AssetDatabase.LoadAssetAtPath<T>(path);

                    if (asset != null && asset.UID == uid)
                        return asset;
                }
            }

            return null;
        }
        public static T GetAsset<T>(string id) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"{id} t:{typeof(T).Name}");

            if (guids != null && guids.Length > 0)
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    var guid = guids[i];

                    if (string.IsNullOrEmpty(guid))
                        continue;

                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    return AssetDatabase.LoadAssetAtPath<T>(path);
                }
            }

            return null;
        }


        public static T CreateSO<T>(string id, string path) where T : ScriptableObject
        {
            T so = ScriptableObject.CreateInstance<T>();
            var pathdir = path + "/" + id + ".asset";
            AssetDatabase.CreateAsset(so, pathdir);
            so.Log($"{so} is created!");

            return so;
        }

        #endregion

        #region Load
        public static void LoadAllAssetsOfType<T>(string absolutePath, ref List<T> assets) where T : Object
        {
            var ds = Directory.GetDirectories(absolutePath, "*", SearchOption.AllDirectories);

            var dpath = Application.dataPath + "/";

            dpath = dpath.Replace("Assets/", "");

            foreach (var d in ds)
            {
                var a = d.Replace('\\', '/');
                a = a.Replace(dpath, "");

                LoadAssetsAt(a, assets);
            }
        }
        public static void LoadAllAssetsOfType<T>(ref List<T> assets, string parenfolder) where T : UnityEngine.Object
        {
            var startswiht = "Assets/" + parenfolder;

            var ds = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);

            var dpath = Application.dataPath + "/";

            dpath = dpath.Replace("Assets/", "");

            foreach (var d in ds)
            {
                var a = d.Replace('\\', '/');
                a = a.Replace(dpath, "");

                if (a.Contains(startswiht))
                    MADUtility.LoadAssetsAt(a, assets);
            }
        }


        public static void LoadAllAssetsOfType<T>(ref List<T> assets) where T : UnityEngine.Object
        {
            var ds = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);

            var dpath = Application.dataPath + "/";

            dpath = dpath.Replace("Assets/", "");

            foreach (var d in ds)
            {
                var a = d.Replace('\\', '/');
                a = a.Replace(dpath, "");

                MADUtility.LoadAssetsAt(a, assets);
            }
        }

        public static int LoadAssetsAt<T>(string path, List<T> assetsFound) where T : UnityEngine.Object
        {
            if (!Directory.Exists(path))
                return 0;

            string[] filePaths = System.IO.Directory.GetFiles(path);


            int countFound = 0;

            if (filePaths != null && filePaths.Length > 0)
            {
                for (int i = 0; i < filePaths.Length; i++)
                {
                    UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(filePaths[i], typeof(T));
                    if (obj is T asset)
                    {
                        countFound++;
                        if (!assetsFound.Contains(asset))
                        {
                            assetsFound.Add(asset);
                        }
                    }
                }
            }

            return countFound;
        }


        public static void SaveObjectToFile(Object obj, string fileName)
        {
            AssetDatabase.CreateAsset(obj, fileName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        #endregion

        #region Texture

        private delegate void GetWidthAndHeight(TextureImporter importer, ref int width, ref int height);
        private static GetWidthAndHeight getWidthAndHeightDelegate;

        public struct Size
        {
            public int width;
            public int height;

            public override string ToString()
            {
                return $"({width},{height})";
            }
        }

        public static Size GetOriginalTextureSize(Texture2D texture)
        {
            if (texture == null)
                throw new NullReferenceException();

            var path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path))
                throw new Exception("Texture2D is not an asset texture.");

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                throw new Exception("Failed to get Texture importer for " + path);

            return GetOriginalTextureSize(importer);
        }

        public static Size GetOriginalTextureSize(TextureImporter importer)
        {
            if (getWidthAndHeightDelegate == null)
            {
                var method = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                getWidthAndHeightDelegate = Delegate.CreateDelegate(typeof(GetWidthAndHeight), null, method) as GetWidthAndHeight;
            }

            var size = new Size();
            getWidthAndHeightDelegate(importer, ref size.width, ref size.height);

            return size;
        }

        public static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.hideFlags = HideFlags.DontSave;
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        #endregion

        #region EditorWindow
        public static Rect GetMainWindowPosition()
        {
            // Get the Unity Editor window on the current screen
            var mainWindow = EditorWindow.focusedWindow;

            if (mainWindow != null)
            {
                // If the main window is available, use its position
                return mainWindow.position;
            }
            else
            {
                // Default position if the main window cannot be found
                return new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);
            }
        }
        #endregion

        #region OnValidate

        public static void TrySetIDbyName(this Object obj, ref string id)
        {
            if (Application.isPlaying)
                return;

            var setDirty = false;

            if (string.IsNullOrEmpty(id))
            {
                id = obj.name;
                setDirty = true;
            }

            if (setDirty)
                EditorUtility.SetDirty(obj);
        }
        public static void TrySetIDbyNameMatch(this Object obj, ref string id)
        {
            if (Application.isPlaying)
                return;

            var setDirty = false;

            if (string.IsNullOrEmpty(id) || id != obj.name)
            {
                id = obj.name;
                setDirty = true;
            }

            if (setDirty)
                EditorUtility.SetDirty(obj);
        }
        #endregion

        #region MenuItem


        public static T GetOrCreateSOAtEssentialsFolder<T>() where T : ScriptableObject
        {
            T instance = null;

            var instances = GetAllInstances_Editor<T>();

            if (instances == null || instances.Length == 0)
            {
                var path = GetEssentialsResourceFolder();
                instance = CreateSO<T>(id: typeof(T).Name, path);
            }
            else
            {
                instance = instances[0];

                if (instances.Length > 1)
                    $"There are more thatn one ScriptableObject of type [{typeof(T)}]!".LogWarning();
            }


            return instance;
        }
        public static object GetOrCreateSOAtEssentialsFolder(Type fieldType)
        {
            var method = typeof(MADUtility)
               .GetMethods(BindingFlags.Public | BindingFlags.Static)
               .FirstOrDefault(m => m.IsGenericMethod && m.Name == "GetOrCreateSOAtEssentialsFolder");

            if (method == null)
            {
                throw new InvalidOperationException("Could not find the correct static generic method.");
            }

            var genericMethod = method.MakeGenericMethod(fieldType);
            return genericMethod.Invoke(null, null);
        }

        #endregion

        #region Symbols
        public static void TryRemoveSymbol(string defineSymbol)
        {
            if (string.IsNullOrEmpty(defineSymbol))
                return;

            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (defineSymbols.Contains(defineSymbol))
            {
                $"[{defineSymbol}] symbol removed!".Log();
                defineSymbols = defineSymbols.Replace($";{defineSymbol}", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbols);
            }

        }
        public static void TryAddSymbol(string defineSymbol)
        {
            if (string.IsNullOrEmpty(defineSymbol))
                return;

            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (!defineSymbols.Contains(defineSymbol))
            {
                $"[{defineSymbol}] symbol added!".Log();
                defineSymbols += $";{defineSymbol}";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbols);
            }
        }
        #endregion

        #region Assets

        public static void SetRandomName<T>(this List<T> list) where T : ScriptableObject
        {
            for (int i = 0; i < list.Count; i++)
            {
                T asset = list[i];
                string tempName = $"__TEMP__{Guid.NewGuid()}";


                string path = AssetDatabase.GetAssetPath(asset);
                AssetDatabase.RenameAsset(path, tempName);
                asset.name = tempName;
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RenameItemsByIndex<T>(this List<T> items, string itemPrefix, Action<T, string> onItemNameChanged) where T : ScriptableObject
        {
            var index = 0;
            foreach (var item in items)
            {
                string tempName = $"__TEMP__{Guid.NewGuid()}_{index}";
                RenameAssetFully(item, tempName, null);
                index++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            for (int i = 0; i < items.Count; i++)
            {
                var parent = items[i];
                string desiredParentName = $"{itemPrefix} {i + 1}";
                string uniqueParentName = GetUniqueAssetName(parent, desiredParentName);
                RenameAssetFully(parent, uniqueParentName, onItemNameChanged);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RenameItemsAndSubItems<T, B>(this List<T> items, Func<T, List<B>> getSubItems,
            Action<T, string> onItemNameChanged, Action<B, string> onSubItemNameChanged,
            string itemPrefix = "Item", string subItemPrefix = "_Sub ")
            where T : ScriptableObject
            where B : ScriptableObject
        {

            var index = 0;

            // === STEP 1: TEMP RENAME ALL ===
            foreach (var obj in items)
            {
                string tempName = $"__tmp_parent_{index++}";
                RenameAssetFully(obj, tempName, null);
            }

            foreach (var obj in items.SelectMany(getSubItems))
            {
                string tempName = $"__tmp_child_{index++}";
                RenameAssetFully(obj, tempName, null);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            System.Threading.Thread.Sleep(500);

            //// === STEP 2: FINAL RENAME ITEMS ===
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                string finalName = $"{itemPrefix} {i + 1}";
                string uniqueName = GetUniqueAssetName(item, finalName);
                RenameAssetFully(item, uniqueName, onItemNameChanged);
            }

            // === STEP 3: FINAL RENAME SUB-ITEMS ===
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                string parentName = $"{itemPrefix} {i + 1}";
                var children = getSubItems(item);
                if (children == null) continue;

                for (int j = 0; j < children.Count; j++)
                {
                    var child = children[j];
                    string finalName = $"{parentName}{subItemPrefix} {j + 1}";
                    string uniqueName = GetUniqueAssetName(child, finalName);
                    RenameAssetFully(child, uniqueName, onSubItemNameChanged);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void RenameAssetFully<T>(T asset, string newName, Action<T, string> onNameChanged) where T : ScriptableObject
        {
            string oldPath = AssetDatabase.GetAssetPath(asset);
            string dir = Path.GetDirectoryName(oldPath);
            string newPath = Path.Combine(dir, newName + ".asset");

            if (Path.GetFileNameWithoutExtension(oldPath) == newName)
                return;

            string result = AssetDatabase.MoveAsset(oldPath, newPath);
            if (!string.IsNullOrEmpty(result))
            {
                Debug.LogError($"Failed to move asset: {result}");
                return;
            }

            asset.name = newName;
            onNameChanged?.Invoke(asset, newName);
            EditorUtility.SetDirty(asset);
        }
        private static string GetUniqueAssetName<T>(T asset, string baseName) where T : ScriptableObject
        {
            string path = AssetDatabase.GetAssetPath(asset);
            string dir = Path.GetDirectoryName(path);
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dir, baseName + ".asset"));
            return Path.GetFileNameWithoutExtension(uniquePath);
        }


        #endregion

        #region MetaFiles
        /// <summary>
        /// it changes the meta file slightly in case if it's not been tracked by git automatically
        /// </summary>
        /// <param name="obj"></param>
        public static void TouchMeta(this ScriptableObject obj)
        {
            var assetPath = AssetDatabase.GetAssetPath(obj);
            string metaPath = assetPath + ".meta";

            if (File.Exists(metaPath))
            {
                string[] lines = File.ReadAllLines(metaPath);
                // Add or toggle a dummy comment at the end
                const string marker = "# touched";
                if (lines.Length == 0 || lines[^1] != marker)
                {
                    File.AppendAllText(metaPath, Environment.NewLine + marker);
                }
                else
                {
                    // Toggle back to remove the touch marker
                    File.WriteAllLines(metaPath, lines.Take(lines.Length - 1));
                }
            }
            else
            {
                Debug.Log("Meta file doesn't exist");
            }
        }

        #endregion

        #region Debug Draw
        public static void DebugDrawBounds(this Bounds bounds, Color color, float duration = 1)
        {
            Vector3 c = bounds.center;
            Vector3 e = bounds.extents;

            Vector3[] pts = new Vector3[8]
            {
               c + new Vector3(-e.x, -e.y, -e.z),
               c + new Vector3(-e.x, -e.y,  e.z),
               c + new Vector3(-e.x,  e.y, -e.z),
               c + new Vector3(-e.x,  e.y,  e.z),
               c + new Vector3( e.x, -e.y, -e.z),
               c + new Vector3( e.x, -e.y,  e.z),
               c + new Vector3( e.x,  e.y, -e.z),
               c + new Vector3( e.x,  e.y,  e.z),
            };

            // Draw bottom square
            Debug.DrawLine(pts[0], pts[1], color, duration);
            Debug.DrawLine(pts[1], pts[5], color, duration);
            Debug.DrawLine(pts[5], pts[4], color, duration);
            Debug.DrawLine(pts[4], pts[0], color, duration);

            // Draw top square
            Debug.DrawLine(pts[2], pts[3], color, duration);
            Debug.DrawLine(pts[3], pts[7], color, duration);
            Debug.DrawLine(pts[7], pts[6], color, duration);
            Debug.DrawLine(pts[6], pts[2], color, duration);

            // Draw vertical lines
            Debug.DrawLine(pts[0], pts[2], color, duration);
            Debug.DrawLine(pts[1], pts[3], color, duration);
            Debug.DrawLine(pts[5], pts[7], color, duration);
            Debug.DrawLine(pts[4], pts[6], color, duration);
        }
        #endregion

        #region Console
        public static void ClearConsole()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod?.Invoke(null, null);
        }
        #endregion
#endif



        #region Excel Reader

#if UNITY_EDITOR
        public static int GetRowIndex(this DataRow dataRow) => dataRow.Table.Rows.IndexOf(dataRow);

        public static string TryGetStringAtColumn(this DataRow row, int columnIndex)
        {
            return row[columnIndex].ToString().TrimEnd();
        } 
#endif

        public static void GetExcelValueStrAndOptions(string str, out string valueStr, out string options)
        {
            valueStr = str;
            options = null;
            var bracIndex = valueStr.IndexOf('[');

            if (bracIndex >= 1)
            {
                valueStr = str.Substring(0, bracIndex);

                int pFrom = bracIndex + 1;
                int pTo = str.LastIndexOf(']');
                if (pTo < 0)
                {
                    $"no ']' for {str}".LogWarning();
                    return;
                }
                options = str.Substring(pFrom, pTo - pFrom);
            }
        }

        public static void GetExcelObjectiveStrAndCount(string str, out string objectiveStr, out int count)
        {
            var split = str.Split('*');
            objectiveStr = split[0];
            count = 1;

            if (split.Length > 1)
                if (int.TryParse(split[1], out int pCount))
                    count = pCount;
        }

        #endregion

        public static void TrySetDirty(this Object obj)
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;
            EditorUtility.SetDirty(obj);
#endif
        }

        #endregion
    }


    public enum PivotType
    {
        Center,
        Left,
        Right,
        TopCenter,
        TopLeft,
        TopRight,
        BottomCenter,
        BottomLeft,
        BottomRight,
        Custom
    }
    public enum PivotAxis
    {
        Forward,
        Up,
        Right
    }

    [Flags]
    public enum AxisConstraints
    {
        None = 0,
        X = 1 << 0,  // 1
        Y = 1 << 1,  // 2
        Z = 1 << 2,  // 4
        XY = X | Y,  // 3
        XZ = X | Z,  // 5
        YZ = Y | Z,  // 6
        XYZ = X | Y | Z  // 7
    }

}