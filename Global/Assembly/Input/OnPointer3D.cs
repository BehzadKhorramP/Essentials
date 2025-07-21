using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MadApper.Input
{
    [RequireComponent(typeof(Collider))]
    public class OnPointer3D : OnPointer
    {
        public void Awake()
        {
            Camera main = Camera.main;

            PhysicsRaycaster physicsRaycaster = null;

            if (main.TryGetComponent(out physicsRaycaster))
            {
                physicsRaycaster.enabled = true;
            }
            else
            {
                physicsRaycaster = main.gameObject.AddComponent<PhysicsRaycaster>();
                physicsRaycaster.enabled = true;
            }

        }
    }


}