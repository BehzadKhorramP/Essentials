using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Levels.Editor
{
    public class InspectWindow : OdinEditorWindow
    {
        private static readonly Dictionary<object, InspectWindow> openWindows = new();

        [SerializeField] object targetObject;

        public static void ShowWindow<TObj>(TObj objToInspect, string title = null) where TObj : Object
        {
            if (objToInspect == null) return;

            if (openWindows.TryGetValue(objToInspect, out var existingWindow))
            {
                existingWindow.Focus(); 
                return;
            }

            var window = ScriptableObject.CreateInstance<InspectWindow>();
            var t = string.IsNullOrEmpty(title) ? $"{objToInspect}" : title;
            window.titleContent = new GUIContent(t);
            window.targetObject = objToInspect;
            openWindows[objToInspect] = window;
            window.Show();
        }

        protected override object GetTarget() => targetObject;
        public TObj GetObject<TObj>() where TObj : Object => targetObject as TObj;

        protected override void OnDestroy()
        {
            if (targetObject != null) openWindows.Remove(targetObject);
            base.OnDestroy();
        }
    }
}
