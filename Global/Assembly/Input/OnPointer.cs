using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MadApper.Input
{

    public class OnPointer : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler,
        IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] float m_TapThreshold = .5f;

        [SerializeField] UnityEventDelayList onPointerEnterEvents;
        [SerializeField] UnityEventDelayList onPointerDownEvents;
        [SerializeField] UnityEventDelayList onPointerUpEvents;
        [SerializeField] UnityEventDelayList onPointerExitEvents;
        [SerializeField] UnityEventDelayList onPointerClickEvents;
        
        float tapStartTime;


        Action onPointerEnter;
        public void OnPointerEnteredCallback(Action callback) => onPointerEnter = callback;
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke();
            onPointerEnterEvents?.Invoke();
        }


        Action onPointerDown;
        public void OnPointerDownCallback(Action callback) => onPointerDown = callback;
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            onPointerDown?.Invoke();
            tapStartTime = Time.time;
            onPointerDownEvents?.Invoke();
        }

        Action onPointerUp;
        public void OnPointerUpCallback(Action callback) => onPointerUp = callback;
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            onPointerUp?.Invoke();
            onPointerUpEvents?.Invoke();
        }

        Action onPointerExit;
        public void OnPointerExitCallback(Action callback) => onPointerExit = callback;
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke();
            onPointerExitEvents?.Invoke();
        }


        Action onClicked;
        public void OnClickedCallback(Action callback) => onClicked = callback;
        public void z_OnClickedCallback(UnityEventDelayRaiser @event) => onClicked = () => @event?.RaiseEvents();
        public void z_RemoveOnClickedCallback() => onClicked = null;
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {          
            if (Time.time - tapStartTime <= m_TapThreshold)
            {
                onClicked?.Invoke();
                onPointerClickEvents?.Invoke();
            }
        }
    }

}