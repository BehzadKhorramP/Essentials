using MadApper;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace MadApperEditor.Common
{
    
    public class EditorCommonAssets : SingletonScriptable<EditorCommonAssets>
    {
#if UNITY_EDITOR

        public const string k_GreenColor = "#1CFF8E";
        public const string k_RedColor = "#EE586A";
        public const string k_YellowColor = "#FFD43E";


        [SerializeField, PreviewField] Texture2D addIcon;
        [SerializeField, PreviewField] Texture2D removeIcon;
        [SerializeField, PreviewField] Texture2D rotateIcon;

        public Color GreenColor;
        public Color RedColor;
        public Color YellowColor;


        public static EditorCommonAssets Get()
        {
            return MADUtility.GetOrCreateSOAtEssentialsFolder<EditorCommonAssets>();        
        }


        public Texture2D GetRotateIcon() => rotateIcon;

        #region Styles

        static GUIStyle _centerWrappedTextStyle;
        public static GUIStyle CenterWrappedTextStyle => _centerWrappedTextStyle ?? (_centerWrappedTextStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            wordWrap = true,           
            alignment = TextAnchor.MiddleCenter
        });

        static GUIContent _addButtonContent;
        public GUIContent AddButtonContent => _addButtonContent ?? (addIcon != null ? _addButtonContent = new GUIContent(addIcon) : null);

        static GUIContent _removeButtonContent;
        public GUIContent RemoveButtonContent => _removeButtonContent ?? (removeIcon != null ? _removeButtonContent = new GUIContent(removeIcon) : null);

        static GUIContent _rotatoeButtonContent;
        public GUIContent RotateButtonContent => _rotatoeButtonContent ?? (rotateIcon != null ? _rotatoeButtonContent = new GUIContent(rotateIcon) : null);

        public static readonly GUIStyle layoutStyle = new GUIStyle();

        #endregion

        #region Helper Methods

        public void DrawAddButtonWithLabel(string label, Action action)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            {
                DrawAddButton(width: 64, height: 64, action: action);
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            {
                GUILayout.Label(label, CenterWrappedTextStyle, GUILayout.Width(128));
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public void DrawAddButton(float width, float height, Action action, bool centered = true)
        {
            if (centered)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }
            GUI.color = GreenColor;

            if (GUILayout.Button(AddButtonContent, GUI.skin.button, GUILayout.Width(width), GUILayout.Height(height)))
            {
                action?.Invoke();
            }

            GUI.color = Color.white;

            if (centered)
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
        public void DrawRemoveButton(float width, float height, Action action, bool centered = true)
        {
            if (centered)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }

            GUI.color = RedColor;

            if (GUILayout.Button(RemoveButtonContent, GUI.skin.button, GUILayout.Width(width), GUILayout.Height(height)))
            {
                action?.Invoke();
            }
            GUI.color = Color.white;

            if (centered)
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }


        public void DrawLabel(string label, float maxWidth,float maxHeight, int fontSize = 0, bool centered = true , params GUILayoutOption[] options)
        {
            if (centered)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }

           
            Vector2 size = Vector2.zero;

            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };

            if (fontSize == 0)
            {
                fontSize = style.fontSize;

                while (fontSize > 4)
                {
                    style.fontSize = fontSize;
                    size = style.CalcSize(new GUIContent(label));

                    // If both constraints are satisfied, break
                    if ((maxWidth <= 0 || size.x <= maxWidth) &&
                        (maxHeight <= 0 || size.y <= maxHeight))
                    {
                        break;
                    }

                    fontSize--;
                }
            }
            else
            {
                style.fontSize = fontSize;
            }

            GUILayout.Label(label, style, options);

            if (centered)
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
        public void DrawLabel(Texture label, bool centered = true, params GUILayoutOption[] options)
        {
            if (centered)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }

            GUILayout.Label(label, options);

            if (centered)
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }



        public static void SetBackgroundColor(Color color)
        {
            var backgroundTexture = layoutStyle.normal.background;
            if (backgroundTexture == null)
            {
                backgroundTexture = new Texture2D(1, 1)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                layoutStyle.normal.background = backgroundTexture;
                layoutStyle.normal.textColor = Color.white;
            }

            backgroundTexture.SetPixel(0, 0, color);
            backgroundTexture.Apply();
        }

        public static void BigSpace()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
        public static void MidSpace()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
        public static void Space()
        {
            EditorGUILayout.Space();
        }

        public static void HorizontalLine() => EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        #endregion  

#endif
    }



}