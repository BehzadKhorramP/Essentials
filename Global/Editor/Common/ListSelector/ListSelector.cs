using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MadApperEditor.Common
{

    [Serializable]
    public abstract class ListSelector<TContainer, TItem>
    {
        [HideInInspector] public string Label;
        [HideInInspector] public int SelectedIndex = -1;
        [HideInInspector] public TContainer Container;

        public Action<TItem, int> OnItemSelected;

        protected ListSelector(TContainer container, string label)
        {
            Container = container;
            Label = label;
        }

        public abstract List<TItem> GetItems();
        public void SetOnItemSelected(Action<TItem, int> callback) => OnItemSelected = callback;
        public TItem SelectedItem
        {
            get
            {
                var list = GetItems();
                return (SelectedIndex >= 0 && SelectedIndex < list.Count) ? list[SelectedIndex] : default;
            }
        }
        public void Select(int index)
        {
            var items = GetItems();
            if (index >= 0 && index < items.Count)
            {
                SelectedIndex = index;
                OnItemSelected?.Invoke(items[index], index);
            }
        }
    }

    public class GenericListSelectorDrawer<Selector, TContainer, TItem> : OdinValueDrawer<Selector>
       where Selector : ListSelector<TContainer, TItem>
    {
        private Vector2 scroll;

        protected virtual UnityEngine.Object GetUnityPreviewObject(TItem item)
        {
            if (item is Component comp) return comp.gameObject;
            return item as UnityEngine.Object;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var selector = this.ValueEntry.SmartValue;
            var items = selector.GetItems();

            SirenixEditorGUI.BeginBox();
            EditorGUILayout.LabelField(selector.Label, EditorStyles.boldLabel);

            if (items == null || items.Count == 0)
            {
                SirenixEditorGUI.WarningMessageBox("No items available.");
                SirenixEditorGUI.EndBox();
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(110));

            GUILayout.BeginHorizontal();

            var hasOnItemSelected = selector.OnItemSelected != null;

            const float previewSize = 75f;
            const float padding = 4f; // green border "overflow"

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) continue;

                var obj = GetUnityPreviewObject(item);
                if (obj == null) continue;

                GUILayout.BeginVertical(GUILayout.Width(previewSize));

                Rect paddedRect = GUILayoutUtility.GetRect(previewSize + padding, previewSize + padding);
                Rect contentRect = new Rect(
                    paddedRect.x + padding / 2f,
                    paddedRect.y + padding / 2f,
                    paddedRect.width - padding,
                    paddedRect.height - padding);

                if (hasOnItemSelected && i == selector.SelectedIndex)
                {
                    EditorGUI.DrawRect(paddedRect, new Color(0.2f, 0.8f, 0.2f, 0.4f));
                }

                Texture2D preview = AssetPreview.GetAssetPreview(obj) ?? AssetPreview.GetMiniThumbnail(obj);

                if (preview != null) GUI.DrawTexture(contentRect, preview, ScaleMode.ScaleToFit);
                else GUI.Box(contentRect, "No Preview");

                if (GUI.Button(contentRect, GUIContent.none, GUIStyle.none))
                {
                    selector.Select(i);
                    if (obj != null) Selection.activeObject = obj;
                }
                GUILayout.Label(obj.name, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(previewSize));
                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            SirenixEditorGUI.EndBox();
        }
    }
}
