using UnityEngine;
using TMPro;

namespace MadApper.Moves
{
    public class MoveCounterUI : MonoBehaviour 
    {
        [SerializeField] protected TextMeshProUGUI m_Text;
        [SerializeField] protected Transform m_Target;

        [SerializeField] protected UnityEventDelayList m_OnUpdate;
        [SerializeField] protected UnityEventDelayList m_OnInit;
        [SerializeField] protected UnityEventDelayList m_OnNoMovesLeft;

        public virtual void OnCreated(int maxMoves)
        {
            if (m_Text != null)
            {
                m_Text.text = maxMoves.ToString();
            }

            m_OnInit?.Invoke();
        }

        public virtual void OnUpdate(int remainingMoves)
        {
            bool noMovesLeft = remainingMoves <= 0;

            m_OnUpdate?.Invoke();

            if (noMovesLeft)
            {
                m_OnNoMovesLeft?.Invoke();
            }

            if (m_Text != null)
            {
                // m_Text.gameObject.SetActive(!noMovesLeft);
                m_Text.text = remainingMoves.ToString();
            }
        }

        public virtual void GetDestroyed()
        {
            transform.SetParent(null);

            if (m_Text != null)
            {
                Destroy(m_Text.gameObject);
            }

            Destroy(gameObject);
        }

        public Transform GetTarget() => m_Target;
    }
}