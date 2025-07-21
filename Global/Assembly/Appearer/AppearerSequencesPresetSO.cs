using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper
{
    [CreateAssetMenu(fileName = "Appearer Seq", menuName = "Appearer Sequnces/Appearer Seq Preset")]

    public class AppearerSequencesPresetSO : ScriptableObject
    {
#if ANIMATIONSEQ_ENABLED
        public AppearerSequencesPreset m_Value; 
#endif
    }
}
