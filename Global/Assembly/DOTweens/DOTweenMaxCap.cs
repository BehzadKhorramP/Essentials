using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper
{
    public class DOTweenMaxCap
    {
        const int m_MaxTweenCap = 1000;
        const int m_MaxSequenceCap = 650;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            Debug.Log($"Max Tween : {m_MaxTweenCap}, Max Sequence : {m_MaxSequenceCap}");
            DOTween.SetTweensCapacity(m_MaxTweenCap, m_MaxSequenceCap);
        }

    }
}
