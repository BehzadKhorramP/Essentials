using System;
using UnityEngine;

namespace BEH
{
    // Simple version
    [Serializable]
    public class IntProbability : ProbabilityBase<int> { }


    [CreateAssetMenu(fileName = "IntProbabilitySO", menuName = "Probability/IntProbabilitySO")]
    public class IntProbabilitySO : ProbabilitySO<int> { }
}
