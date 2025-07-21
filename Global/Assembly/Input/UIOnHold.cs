using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Input
{
    public class UIOnHold : MonoBehaviour
    {
        [SerializeField] Vector3 m_Scale = Vector3.one * .9f;
        [SerializeField] float m_Duration = .2f;
        [SerializeField][AutoGetOrAdd] OnPointer m_OnPointer;
   
        private void Awake()
        {
            m_OnPointer.OnPointerDownCallback(OnDown);
            m_OnPointer.OnPointerUpCallback(OnUp);
            m_OnPointer.OnPointerExitCallback(OnUp);          
        }
              
        private void OnDown()
        {
            transform.DOKill();
            transform.DOScale(m_Scale, m_Duration);
        }
        private void OnUp()
        {
            transform.DOKill();
            transform.DOScale(Vector3.one, m_Duration);
        }

        private void OnDisable()
        {
            transform.DOKill();
        }
    }
}
