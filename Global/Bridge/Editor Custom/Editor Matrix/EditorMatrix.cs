#if UNITY_EDITOR

using MadApperEditor.Common;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MadApper.Bridge
{
    [Serializable]
    public class EditorMatrix<TEditorCell> where TEditorCell : EditorCellBase
    {
        protected EditorCommonAssets commonAssets;

        List<TEditorCell> cells;

        float width;
        float height;
        bool drawAddButton;

        public EditorMatrix(EditorCommonAssets assets, IEnumerable<TEditorCell> cells, float width = 64, float height = 64, bool drawAddButton = true)
        {
            this.commonAssets = assets;
            this.cells = new List<TEditorCell>(cells);
            this.width = width;
            this.height = height;
            this.drawAddButton = drawAddButton;
        }

        [OnInspectorGUI]
        private void DrawMatrix()
        {
            var count = drawAddButton ? 1 : 0;

            if (cells != null) count += cells.Count;
            if (count == 0) return;

            float viewWidth = EditorGUIUtility.currentViewWidth;
            int columns = Mathf.FloorToInt((viewWidth) / (width + 8)); // leave margin
            columns = Mathf.Max(1, columns); // Ensure at least one column

            int rows = Mathf.CeilToInt(count / (float)columns);

            for (int y = 0; y < rows; y++)
            {
                EditorGUILayout.BeginHorizontal();

                for (int x = 0; x < columns; x++)
                {
                    int index = y * columns + x;

                    if (drawAddButton)
                    {
                        if (index >= count - 1)
                        {
                            DrawAddButton();
                            break;
                        }
                    }
                    else if (index >= count) break;

                    var cell = cells[index];

                    cell.DrawPreview(width, height, drawOptions: OnDrawOptions);
                }

                EditorGUILayout.EndHorizontal();
            }
        }


        protected void DrawAddButton()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(width), GUILayout.Height(height));

            commonAssets.DrawAddButton(width, height, OnAddPressed);

            EditorGUILayout.Space();

            commonAssets.DrawLabel("Add", width, height);

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
        }

        public void OnDrawOptions(EditorCellBase cell, float width, float heigth) => OnDrawOptions(cell as TEditorCell, width, heigth);

        public virtual void OnDrawOptions(TEditorCell cell, float width, float heigth) { }
        public virtual void OnAddPressed() { }
    }



}

#endif