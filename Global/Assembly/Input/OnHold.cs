using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;


namespace MadApper.Input
{
    [RequireComponent(typeof(OnPointer))]
    public class OnHold : MonoBehaviour
    {

        [Space(10)][SerializeField] List<Transform> m_Transform;
        [Space(10)][SerializeField] float m_Timer;
        [Space(10)][SerializeField] bool isAuto;

        [Space(10)][SerializeField] bool releaseOnDone;
      //  float? elapsed;
        Vector2 pos;
        Camera mainCam;

        OnPointer onPointer;
        public OnPointer m_OnPointer
        {
            get
            {
                if (onPointer == null)
                    onPointer = GetComponent<OnPointer>();

                return onPointer;
            }
        }

        System.Action<Vector2> onHeldEnough;

        public static System.Action<bool, float, Vector2> onHoldVisuals;

        private void Awake()
        {
            mainCam = Camera.main;

            if (isAuto)
                SubscribeAll();
        }

        public void OnAddTransform(Transform t)
        {
            if (!m_Transform.Contains(t))
            {              
                m_Transform.Add(t);         
            }
        }
        public void OnRemoveTransform(Transform t)
        {
            if (m_Transform.Contains(t))
            {              
                m_Transform.Remove(t);      
            }
        }
        public void SubscribeAll()
        {
            m_OnPointer?.OnPointerDownCallback(OnHoldDown);
            m_OnPointer?.OnPointerExitCallback(OnExit);
            m_OnPointer?.OnPointerUpCallback(OnHoldUp);
        }

        public void UnSubscribeAll()
        {
            m_OnPointer?.OnPointerDownCallback(null);
            m_OnPointer?.OnPointerExitCallback(null);
            m_OnPointer?.OnPointerUpCallback(null);

            foreach (var item in m_Transform)
            {
                Kill(item);
            }
        }


        public void OnHeldEnoughCallback(System.Action<Vector2> callback) => onHeldEnough = callback;

        public void OnHoldDown()
        {          
            foreach (var item in m_Transform)
            {
                OnHoldDown(item);
            }
           // elapsed = 0;
        }
        void OnHoldDown(Transform transform)
        {
            //if (m_OnPointer is OnPointer3D)
            //{
            //    pos = mainCam.WorldToScreenPoint(transform.position);
            //}
            //else
            //{
            //    pos = transform.position;
            //}
            transform.DOKill();
            transform.DOScale(Vector3.one / 1.2f, .1f);
         //   elapsed = 0;
        }

        public void OnHoldUp()
        {
            foreach (var item in m_Transform)
            {
                Kill(item);
            }

            OnEnd();
        }

       
        void Kill(Transform transform)
        {
            transform.DOKill();
            transform.DOScale(Vector3.one, .1f);
        }

        private async void OnExit()
        {
            await System.Threading.Tasks.Task.Delay(10);

            if (this == null)
                return;

            foreach (var item in m_Transform)
            {
                Kill(item);
            }

            OnEnd();
        }

        void OnEnd()
        {
            onHoldVisuals?.Invoke(false, 0, pos);
          //  elapsed = null;
        }

        //private void Update()
        //{
        //    if (elapsed == null)
        //        return;

        //    elapsed += Time.deltaTime;

        //    if (elapsed >= .15f)
        //        onHoldVisuals?.Invoke(true, elapsed.Value / m_Timer, pos);

        //    if (elapsed >= m_Timer)
        //    {
        //        if (releaseOnDone)
        //        {
        //            foreach (var item in m_Transform)
        //            {
        //                Kill(item);
        //            }
        //        }

        //        OnEnd();

        //        onHeldEnough?.Invoke(pos);
        //    }
        //}

        private void OnDestroy()
        {
            foreach (var item in m_Transform)
            {
                item.DOKill();
            }
        }

    }
}
