using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Essentials
{
    public class OnFinishedBridge : MonoBehaviour
    {
        Action action;

        public void SetAction(Action action) => this.action = action;

        public void z_RaiseAction()
        {
            action?.Invoke();
            action = null;
        }

    }
}
