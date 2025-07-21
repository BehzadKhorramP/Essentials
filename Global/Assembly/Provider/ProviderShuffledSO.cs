using System.Collections.Generic;
using UnityEngine;

namespace MadApper
{
    public abstract class ProviderShuffledSO<T> : ScriptableObject where T : class
    {
        public class Provider : ProviderShuffled<T>
        {
            public Provider(List<T> alloweds) : base(alloweds) { }
        }

        [SerializeField] Provider provider;

        public void Initialize(List<T> alloweds)
        {
            provider = new Provider(alloweds);
        }

        public Provider GetProvider() => provider;    

        public T Provide() => provider.Provide();
    }
}
