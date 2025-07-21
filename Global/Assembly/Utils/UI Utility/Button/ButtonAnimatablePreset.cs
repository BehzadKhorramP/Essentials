#if ANIMATIONSEQ_ENABLED
using BrunoMikoski.AnimationSequencer; 
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BEH
{
    public class ButtonAnimatablePreset : MonoBehaviour
    {
#if ANIMATIONSEQ_ENABLED
        public AnimationSequencerController m_Sequence; 
#endif
    }

}