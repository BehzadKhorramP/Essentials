using DG.Tweening;
using MadApper;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BEH.Objective
{
    public class ObjectiveNeedUI : MonoBehaviour
    {
        [NonSerialized] public Need Need;

        [Space(10)][SerializeField] protected TextMeshProUGUI m_Text;
        [Space(10)][SerializeField] protected Image[] m_Image;
        [Space(10)][SerializeField] protected Transform m_Target;

        [SerializeField] protected UnityEventDelayList m_OnInit;
        [SerializeField] protected UnityEventDelayList m_OnUpdate;
        [SerializeField] protected UnityEventDelayList m_OnDone;

        public virtual void OnCreated(Need need)
        {
            this.Need = need;

            m_Text?.SetText(need.NeededCount.ToString());

            var icon = need.IObjectivable.GetIcon();

            if (icon != null)
                foreach (var item in m_Image)
                    item.sprite = icon;

            m_OnInit?.Invoke();
        }
      
        public virtual void OnUpdate(int remaining)
        {
            var done = remaining == 0;

            m_OnUpdate?.Invoke();

            if (done)
                m_OnDone?.Invoke();

            if (m_Text != null)
            {
                m_Text.gameObject.SetActive(!done);
                m_Text.text = remaining.ToString();
            }
        }

        public virtual void GetDestroyed()
        {
            transform.SetParent(null);

            if (m_Text != null)
                Destroy(m_Text.gameObject);

            Destroy(gameObject);
        }

        public Transform GetTarget() => m_Target;
    }
}
