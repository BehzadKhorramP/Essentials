using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BEH
{
    public class DebugFPS : MonoBehaviour
    {
        public int TargetFPS = 60;

        public void z_SetFPS()
        {
            Application.targetFrameRate = TargetFPS;
        }      
    }
}
