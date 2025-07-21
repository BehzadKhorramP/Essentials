#if UNITY_EDITOR

using MadApperEditor.Common;
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MadApper.Bridge
{
    [Serializable]
    public abstract class EditorCellBase
    {
        protected EditorCommonAssets commonAssets;

        public EditorCellBase(EditorCommonAssets assets) { this.commonAssets = assets; }

        public abstract string Name { get; }
        public abstract Object Icon { get; }
        public virtual int FontSize => 0;

        public void DrawPreview(float width, float height, Action<EditorCellBase, float, float> drawOptions)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(width), GUILayout.Height(height));

            GUILayout.FlexibleSpace();

            float labelHeight = 16;

            if (Icon == null)
            {
                labelHeight = 32;
                EditorGUILayout.Space();
            }

            if (!string.IsNullOrEmpty(Name))
            {
                commonAssets.DrawLabel(Name, width, labelHeight, FontSize, true, GUILayout.Width(width), GUILayout.Height(labelHeight));
            }

            if (Icon != null)
            {
                var icon = AssetPreview.GetAssetPreview(Icon) ?? AssetPreview.GetMiniThumbnail(Icon);

                if (icon != null)
                {
                    commonAssets.DrawLabel(icon, true, GUILayout.Width(width), GUILayout.Height(height));
                }
            }

            drawOptions?.Invoke(this, width, height);

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();
        }
    }





}

#endif