using MadApper;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MadApperEditor.Common
{
    public abstract class PropertyPopupWindow<TProperty> : OdinEditorWindow where TProperty : class
    {
        #region Editor Assets      

        [NonSerialized] EditorCommonAssets _commonAssets;
        protected EditorCommonAssets commonAssets => _commonAssets ??= MADUtility.GetOrCreateSOAtEssentialsFolder<EditorCommonAssets>();
        #endregion

        protected TProperty property;

        protected Action<TProperty> onSaveCallback;

        private PropertyTree propertyTree;

        protected Color saveColor = Color.white;

        protected override void OnEnable()
        {
            base.OnEnable();
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            DisposeTree();
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                CleanupEditor();
            }
        }


        public static TWindow OpenEditor<TWindow>(TProperty property, Action<TProperty> onSaveCallback, string name = null)
          where TWindow : PropertyPopupWindow<TProperty>
        {
            var window = GetWindow<TWindow>();
            var id = name;
            if (string.IsNullOrEmpty(name)) id = $"{typeof(TProperty)} Editor";
            window.titleContent = new GUIContent(id);
            window.Show();
            window.CleanupEditor();
            window.SetOnSaveCallback(onSaveCallback);
        //    window.InitializeStock(property);

            return window;
        }




        protected virtual void CleanupEditor()
        {
            property = null;
            saveColor = Color.white;
            DisposeTree();
            Repaint();
        }
        void DisposeTree()
        {
            if (propertyTree != null) propertyTree.Dispose();
            propertyTree = null;
        }

        public void SetOnSaveCallback(Action<TProperty> onSaveCallback) => this.onSaveCallback = onSaveCallback;






        protected virtual void OnAnyPropertyChanged()
        {
            saveColor = commonAssets.GreenColor;
        }

        protected virtual void Save()
        {
            onSaveCallback?.Invoke(property);
            saveColor = Color.white;
        }
    }
}
