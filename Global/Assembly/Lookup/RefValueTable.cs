using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper
{
    public abstract class RefValueTable<TRef, TValue> : ScriptableObject
        where TRef : class
        where TValue : class
    {
        [TableList(AlwaysExpanded = true, DrawScrollView = true)][SerializeField] public List<Item> items;

        public TValue GetValueByRef(TRef @ref)
        {
            var targetRef = @ref;
            var match = items.Find(item => item.Ref == targetRef);
            if (match == null) return null;
            return match.Value;
        }


        [Serializable]
        public class Item
        {
            [InlineEditor(InlineEditorObjectFieldModes.Boxed, Expanded = true)] public TRef Ref;
            [InlineProperty, HideLabel] public TValue Value;           
        }
    }
}
