using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MadApper.Essentials
{
    public class VolumePPUtils : MonoBehaviour
    {
        [SerializeField] Volume m_Volume;

        public void z_UpdateWeight(float weight)
        {
            m_Volume.weight = weight;
        }
    }
}
