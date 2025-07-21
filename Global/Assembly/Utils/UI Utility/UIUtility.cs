using System;
using UnityEngine;

namespace MadApper
{
    public static class UIUtility
    {
        public static void MakeRectCentered(this RectTransform rect)
        {
            if (rect == null)
                return;

            var aPos = rect.anchoredPosition;
            var rectSize = rect.rect.size;
            var mid = -rectSize.x / 2f;
            aPos.x = mid;
            rect.anchoredPosition = aPos;
        }



        public static bool IsRectTransformPartiallyInView_Overlaps(this RectTransform rectTransform, Vector3[] rectCorners,  Camera camera)
        {
            // Get the RectTransform's world corners           
            rectTransform.GetWorldCorners(rectCorners);

            // Get the screen's bounds in world space
            Rect screenRect = GetScreenWorldRect(camera);

            // Get the RectTransform bounds as a Rect (in world space)
            Rect rectTransformBounds = new Rect(rectCorners[0].x, rectCorners[0].y,
                                                 rectCorners[2].x - rectCorners[0].x,
                                                 rectCorners[2].y - rectCorners[0].y);

            // Check if the two Rects overlap
            return rectTransformBounds.Overlaps(screenRect);


            Rect GetScreenWorldRect(Camera camera)
            {
                // Screen corners in world space, assuming the camera renders the UI
                Vector3 bottomLeft, topRight;

                if (camera == null)
                {
                    // ScreenSpace-Overlay: Use Screen coordinates
                    bottomLeft = new Vector3(0, 0, 0);
                    topRight = new Vector3(Screen.width, Screen.height, 0);
                }
                else
                {
                    // Convert screen space corners to world space using the camera
                    bottomLeft = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
                    topRight = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camera.nearClipPlane));
                }

                return new Rect(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
            }
        }


        [Serializable]
        public struct PartialViewData
        {
            public float OffestX;
            public float OffsetY;
        }
        public static bool IsRectTransformPartiallyInView_Mathsy(this RectTransform rectTransform, Vector3[] rectCorners, PartialViewData data, Camera camera)
        {
            // Get the RectTransform's world corners          
            rectTransform.GetWorldCorners(rectCorners);

            foreach (Vector3 corner in rectCorners)
            {
                Vector3 screenPoint = camera.WorldToScreenPoint(corner);

                if (screenPoint.x + data.OffestX >= 0 && screenPoint.x - data.OffestX <= Screen.width &&
                    screenPoint.y + data.OffsetY >= 0 && screenPoint.y - data.OffsetY <= Screen.height)
                {

                    // At least one corner is within the screen bounds
                    return true;
                }
            }



            // Define the screen corners in world space
            Vector3 bottomLeft = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
            Vector3 topLeft = camera.ScreenToWorldPoint(new Vector3(0, Screen.height, camera.nearClipPlane));
            Vector3 topRight = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camera.nearClipPlane));
            Vector3 bottomRight = camera.ScreenToWorldPoint(new Vector3(Screen.width, 0, camera.nearClipPlane));


            Vector3[] screenCorners = { bottomLeft, topLeft, topRight, bottomRight };


            // Check if any of the screen corners are inside the RectTransform
            foreach (Vector3 screenCorner in screenCorners)
            {
                if (IsPointInsideRectTransform(screenCorner, rectCorners))
                {
                    return true;
                }
            }

            // No corners are within the screen bounds
            return false;



            bool IsPointInsideRectTransform(Vector3 point, Vector3[] rectCorners)
            {
                // Create a plane for the RectTransform in world space
                Plane rectPlane = new Plane(rectCorners[0], rectCorners[1], rectCorners[2]);

                // Project the point onto the RectTransform's plane
                Vector3 projectedPoint = rectPlane.ClosestPointOnPlane(point);

                // Check if the projected point is within the 2D bounds of the RectTransform
                return projectedPoint.x >= rectCorners[0].x && projectedPoint.x <= rectCorners[2].x &&
                       projectedPoint.y >= rectCorners[0].y && projectedPoint.y <= rectCorners[2].y;
            }


        }


    }

}