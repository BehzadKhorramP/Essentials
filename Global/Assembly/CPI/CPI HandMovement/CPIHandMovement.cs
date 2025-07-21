using DG.Tweening;
using MadApper.Input;
using UnityEngine;

namespace MadApper.CPI
{
    public class CPIHandMovement : MonoBehaviour
    {
        [SerializeField] CanvasGroup m_Hand;
        [SerializeField] bool m_Active;
        [SerializeField] bool m_UpdateOnlyWhenDragging;
        [SerializeField] bool m_RippleEffectOn = true;
        [SerializeField] float m_Speed = 5.5f;
        [SerializeField] float scaleDivider = 1.2f;
        [SerializeField] Transform m_RippleParent;
        [SerializeField] SimpleAutoDestroy m_RippleAnim;

        bool dragging;
        bool active;
        Vector2 targetPos;

        private void OnEnable()
        {
            var instance = InputManager.Instance;

            if (instance != null)
            {
                InputManager.Instance.Pressed(Pressed);
                InputManager.Instance.Released(Released);
            }


        }
        private void OnDisable()
        {
            var instance = InputManager.Instance;

            if (instance != null)
            {
                InputManager.Instance.PressedUnsubscribe(Pressed);
                InputManager.Instance.ReleasedUnsubscribe(Released);
            }
        }

        private void Start()
        {
            active = m_Active;
            m_Hand.gameObject.SetActive(active);
            gameObject.SetActive(active);
        }


        private void Pressed(SwipeInput obj)
        {
            OnStart(obj.StartPosition);
            m_Hand.DOKill();
            m_Hand.transform.DOScale(Vector2.one / scaleDivider, .1f);
            dragging = true;
        }
        private void Released()
        {
            dragging = false;
            m_Hand.DOKill();
            m_Hand.transform.DOScale(Vector2.one, .1f);
        }
        private void OnStart(Vector2 obj)
        {
            m_Hand.transform.position = obj;
            targetPos = obj;

            if (m_RippleEffectOn && m_RippleAnim != null)
            {
                var rip = Instantiate(m_RippleAnim, m_RippleParent);
                rip.transform.position = obj;
                rip.DestroySelf_WO_Parent(2);
            }

        }


        private void Update()
        {
            if (m_UpdateOnlyWhenDragging)
            {
                if (dragging)
                {
                    targetPos = InputManager.Instance.GetPointerPosition();
                }
            }
            else
            {
                if (InputManager.Instance != null)
                    targetPos = InputManager.Instance.GetPointerPosition();
            }


            m_Hand.transform.position = Vector3.Lerp(m_Hand.transform.position, targetPos, Time.deltaTime * m_Speed * 3);
        }


    }

}