#if UNITY_EDITOR

using Sirenix.Reflection.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MadApper.Bridge
{
    public class EditorSelectionPopup<TEditorCell> : EditorWindow where TEditorCell : EditorCellBase
    {
        private Action<TEditorCell> onSelect;
        private List<TEditorCell> allOptions;
        private Vector2 scroll;
        private float width = 32;
        private float height = 32;

        public static void ShowWindow<TWindow>(List<TEditorCell> allOptions, Action<TEditorCell> onSelect, float width = 48, float height = 48) where TWindow : EditorSelectionPopup<TEditorCell>
        {
            var window = GetWindow<TWindow>();

            window.allOptions = allOptions;
            window.onSelect = onSelect;
            window.width = width;
            window.height = height;
            window.titleContent = new GUIContent("Select Item");


            // Calculate window size
            int minColumns = 4;
            int totalItems = allOptions?.Count ?? 0;
            int rows = Mathf.CeilToInt(totalItems / (float)minColumns);
            float padding = 16f; // Optional for scrollbars/margins
            float windowWidth = minColumns * (width + 16f) + padding;
            float windowHeight = rows * (height + 16f) * 2 + padding;
       
            Vector2 screenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            window.position = new Rect(screenPos.x, screenPos.y, windowWidth, windowHeight);

            window.ShowUtility(); // or Show() to allow docked          
        }

        private void OnGUI()
        {
            if (allOptions == null || !allOptions.Any())
            {
                Close();
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            var count = allOptions.Count;
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

                    if (index >= count)
                        break;

                    var cell = allOptions[index];

                    cell.DrawPreview(width, height, drawOptions: (x, width, height) => DrawButton(cell));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawButton(TEditorCell cell)
        {
            if (GUILayout.Button("Add"))
            {
                onSelect?.Invoke(cell);
                Close();
            }

        }
    }




    public class TextInputPopup : PopupWindowContent
    {
        private string inputText = "";
        private readonly Action<string> onSubmit;

        public TextInputPopup(Action<string> onSubmit)
        {
            this.onSubmit = onSubmit;
        }

        public override Vector2 GetWindowSize() => new Vector2(200, 90);

        public override void OnGUI(Rect rect)
        {           
            GUILayout.Label("Enter Value:", EditorStyles.boldLabel);
            inputText = EditorGUILayout.TextField(inputText);

            GUILayout.Space(10);

            GUI.color = Color.yellow;
            if (GUILayout.Button("Submit", GUILayout.Height(28)))
            {
                onSubmit?.Invoke(inputText);
                editorWindow.Close();
            }
            GUI.color = Color.white;
        }
    }



}

#endif