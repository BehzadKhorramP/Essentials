using MadApper;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;

namespace BEH
{

    public abstract class ProviderSO<T> : ScriptableObject
    {
        public abstract T GetValue();
    }
}
