using System;
using System.Collections.Generic;
using UnityEngine;
using FancyScrollView;
using EasingCore;

namespace MadApper.Scroller
{
   
    public abstract class MadScrollView<TData, TContext> : FancyScrollView<TData, TContext>
        where TContext : Context, new()
        where TData : ItemData
    {
        [SerializeField][AutoGetOrAdd] protected FancyScrollView.Scroller m_Scroller;
        [SerializeField] protected MadCell<TData, TContext> m_CellPrefab;
        [SerializeField] protected Ease m_SelectionEase = Ease.OutCubic;

        protected Action<int> onSelectionChanged;
        protected Action<int> onSelectedAgain;

        public MadCell<TData, TContext> GetMadCellPrefab() => m_CellPrefab;
        public Context GetContext() => Context;
        protected override GameObject CellPrefab => m_CellPrefab.gameObject;

        protected override void Initialize()
        {
            base.Initialize();

            Context.OnCellClicked = SelectCell;

            m_Scroller.OnValueChanged(UpdatePosition);
            m_Scroller.OnSelectionChanged(UpdateSelection);
        }

        void UpdateSelection(int index)
        {
            if (Context.SelectedIndex == index)
            {
                return;
            }

            Context.SelectedIndex = index;
            Refresh();

            onSelectionChanged?.Invoke(index);
        }

        public void UpdateData(IList<TData> items)
        {
            UpdateContents(items);
            m_Scroller.SetTotalCount(items.Count);
        }

        public void OnSelectionChanged(Action<int> callback)
        {
            onSelectionChanged = callback;
        }
        public void OnSelectedAgain(Action<int> callback)
        {
            onSelectedAgain = callback;
        }

        public void SelectNextCell()
        {
            SelectCell(Context.SelectedIndex + 1);
        }

        public void SelectPrevCell()
        {
            SelectCell(Context.SelectedIndex - 1);
        }
        public void SelectCell(int index)
        {
            SelectCell(index, duration: .35f);
        }
        public void SelectCell(int index, float duration = .35f)
        {
            if (index < 0 || index >= ItemsSource.Count)
            {
                return;
            }
            if (index == Context.SelectedIndex)
            {
                onSelectedAgain?.Invoke(index);
                return;
            }

            UpdateSelection(index);
            m_Scroller.ScrollTo(index, duration, m_SelectionEase);
        }
    }
}
