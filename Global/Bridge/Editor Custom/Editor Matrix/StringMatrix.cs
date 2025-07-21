#if UNITY_EDITOR

using MadApperEditor.Common;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MadApper.Bridge
{
    [Serializable]
    public class StringMatrix : EditorMatrix<StringEditorCell>
    {
        [SerializeField][HideInInspector] Action<string> onAdded;
        [SerializeField][HideInInspector] Action<StringEditorCell> onRemoved;

        public StringMatrix(EditorCommonAssets assets, IEnumerable<StringEditorCell> cells, Action<string> onAdded, Action<StringEditorCell> onRemoved,
            float width = 64, float height = 64, bool drawAddButton = true) : base(assets, cells, width, height, drawAddButton)
        {
            this.onAdded = onAdded;
            this.onRemoved = onRemoved;
        }

        public override void OnDrawOptions(StringEditorCell cell, float width, float heigth)
        {
            EditorCommonAssets.BigSpace();
            commonAssets.DrawRemoveButton(width, height: 24, () => onRemoved?.Invoke(cell));
        }

        public override void OnAddPressed()
        {
            PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero),
                new TextInputPopup(onSubmit: value =>
                {
                    if (!string.IsNullOrEmpty(value))
                        onAdded?.Invoke(value.ToUpper().TrimEnd());
                }));
        }
    }
    public class StringEditorCell : EditorCellBase
    {
        [HideInInspector] public string Value;
        public StringEditorCell(EditorCommonAssets assets, string value) : base(assets) { this.Value = value; }
        public override string Name => Value;
        public override Object Icon => null;
        public override int FontSize => 18;
    }



}

#endif