using FancyScrollView;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MadApper.Scroller
{

    public abstract class MadCell<TData, TContext> : FancyCell<TData, TContext>
        where TContext : Context, new()
        where TData : ItemData
    {
        public static readonly int Scroll = Animator.StringToHash("scroll");

        [SerializeField][AutoGetInChildren]protected Animator m_Animator;
        [SerializeField] protected Image m_Image;
        [SerializeField] protected TextMeshProUGUI[] m_NameTxt;

        [FoldoutGroup("Events")]
        [SerializeField] protected UnityEventDelayList<Action> m_Initialize;
        [FoldoutGroup("Events")]
        [SerializeField] protected UnityEventDelayList m_Locked;
        [FoldoutGroup("Events")]
        [SerializeField] protected UnityEventDelayList m_UnLocked;
        [FoldoutGroup("Events")]
        [SerializeField] protected UnityEventDelayList m_OnUnLocked;

        float currentPosition = 0;

        protected TData itemData;

        void OnEnable()
        {
            UpdatePosition(currentPosition);
        }
        public override void Initialize()
        {
            m_Initialize?.Invoke(() => Context.OnCellClicked?.Invoke(Index));
        }
        public override void UpdateContent(TData itemData)
        {
            if (itemData == this.itemData)
                return;

            UpdateContetnInternal(itemData);
        }

        protected virtual void UpdateContetnInternal(TData itemData)
        {
            this.itemData = itemData;
            m_Image.sprite = itemData.Sprite;

            foreach (var item in m_NameTxt)
                item.SetText(itemData.Name);

            if (itemData.IsUnlocked) m_UnLocked?.Invoke();
            else m_Locked?.Invoke();
        }


        public override void UpdatePosition(float position)
        {
            currentPosition = position;

            if (m_Animator.isActiveAndEnabled)
            {
                m_Animator.Play(Scroll, -1, position);
            }

            m_Animator.speed = 0;

            if (position >= .4f && position <= .6f)
            {
                transform.SetAsLastSibling();
            }
        }
        public void OnUnlocked()
        {
            m_OnUnLocked?.Invoke();
        }
    }



    [Serializable]
    public class ItemData
    {
        public string Name;
        public Sprite Sprite;
        public bool IsUnlocked;

        public override bool Equals(object obj)
        {
            if (obj is ItemData other)
            {
                return Name == other.Name && IsUnlocked == other.IsUnlocked;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Name?.GetHashCode() ?? 0);
                hash = hash * 23 + IsUnlocked.GetHashCode();
                return hash;
            }
        }
    }

    public class Context
    {
        public int SelectedIndex = -1;
        public Action<int> OnCellClicked;
    }
}
