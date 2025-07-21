using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Scroller
{
    public abstract class UIScrollManager<TData, TContext> : MonoBehaviour
         where TContext : Context, new()
         where TData : ItemData
    {
        [SerializeField][AutoGetInChildren] protected MadScrollView<TData,TContext> m_ScrollView;
        [SerializeField] protected Transform m_StaticItemPanel;

        [FoldoutGroup("Events")]
        [SerializeField] protected UnityEventDelayList m_OnShowStatic;
        [FoldoutGroup("Events")]
        [SerializeField] protected UnityEventDelayList m_OnShowScroller;

        [FoldoutGroup("DEBUG")]
        [SerializeField] protected List<TData> m_ItemsData;
        [FoldoutGroup("DEBUG")]
        [SerializeField] protected int m_SelectedItemIndex;

        MadCell<TData, TContext> selectedStaticCell;

        Args args;

        public void Initialize(Args args)
        {
            this.args = args;

            m_ScrollView.OnSelectionChanged(OnSelectionChanged);
            m_ScrollView.OnSelectedAgain(OnSelectedAgain);
            m_ScrollView.UpdateData(args.ItemsData);

            SetupSelectedItem(args);
            z_OnShowStaticPanel();
        }
        public int GetSelectedItem()
        {
            if (args == null) return 0;
            return args.SelectedIndex;
        }
        private void OnSelectionChanged(int index)
        {
        }
        private void OnSelectedAgain(int index)
        {
            TrySelectItemOfIndex(index);
        }

        void SetupSelectedItem(Args args)
        {
            if (args.ItemsData == null || args.ItemsData.Count <= args.SelectedIndex)
                return;

            DeleteStaticPanel();

            var prefab = m_ScrollView.GetMadCellPrefab();

            var selectedItemData = args.ItemsData[args.SelectedIndex];
            selectedStaticCell = Instantiate(prefab, m_StaticItemPanel);
            selectedStaticCell.Index = args.SelectedIndex;
            selectedStaticCell.SetContext(new TContext()
            {
                OnCellClicked = (x) => z_OnShowScroller()
            });
            selectedStaticCell.Initialize();
            selectedStaticCell.UpdateContent(selectedItemData);
            selectedStaticCell.UpdatePosition(position: .5f);
        }
        public void z_OnShowScroller()
        {
            var currentIndex = m_ScrollView.GetContext().SelectedIndex;
            var selectedIndex = GetSelectedItem();
            if (currentIndex != selectedIndex)
                m_ScrollView.SelectCell(GetSelectedItem(), duration: 0f);

            m_OnShowScroller?.Invoke();
        }
        public void z_OnShowStaticPanel()
        {
            m_OnShowStatic?.Invoke();
        }
        void DeleteStaticPanel()
        {
            foreach (Transform item in m_StaticItemPanel)
            {
                Destroy(item.gameObject);
            }
        }

        void TrySelectItemOfIndex(int index)
        {
            if (args == null)
                return;
            if (index < 0 || index >= args.ItemsData.Count)
                return;

            var data = args.ItemsData[index];

            if (!data.IsUnlocked)
                return;

            args.SelectedIndex = index;

            SetupSelectedItem(args);
            z_OnShowStaticPanel();

            if (args != null)
                args.OnItemSelected?.Invoke(index);
        }

        public void z_TrySelectCurrentChapter()
        {
            var index = m_ScrollView.GetContext().SelectedIndex;
            TrySelectItemOfIndex(index);
        }

        public void z_OnBack()
        {
            z_OnShowStaticPanel();
        }

        #region Getter Settter

        public MadCell<TData, TContext> GetSelectedStaticCell() => selectedStaticCell;

        #endregion


        /// <summary>
        /// for debugging/testing purposes
        /// </summary>

        #region DEBUG
        public void z_InitializeDebug()
        {
            var args = new Args()
            {
                ItemsData = m_ItemsData,
                SelectedIndex = m_SelectedItemIndex
            };

            Initialize(args);
        }


        #endregion

        public class Args
        {
            public List<TData> ItemsData;
            public int SelectedIndex;
            public Action<int> OnItemSelected;
        }
    }
}
