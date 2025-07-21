using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MadApper
{

    [Serializable]
    public abstract class ProviderShuffled<T> where T : class
    {
        [SerializeField] protected List<T> allowables;
        [ShowInInspector][ReadOnly][NonSerialized] protected List<T> availables;

        public ProviderShuffled()
        {
            allowables = new List<T>();
            availables = new List<T>();
        }

        public ProviderShuffled(List<T> allowables)
        {
            this.allowables = new List<T>(allowables);
            Initialize();
        }
        public void Initialize(params object[] deletes)
        {
            if (allowables != null && deletes != null) allowables.RemoveAll(item => deletes.Contains(item));
            availables = new List<T>();
            TryShuffle();
        }

        public bool IsValid() => allowables != null && allowables.Count > 0;

        public T Provide()
        {
            TryShuffle();

            if (availables == null || availables.Count == 0) return default;

            var res = availables[0];
            availables.RemoveAt(0);

            return res;
        }

        public void Nulify()
        {
            allowables = null;
            availables = null;
        }

        public void SetAllowables(List<T> list)
        {
            allowables = new List<T>(list);
            availables = new List<T>();
        }


        public List<T> GetAllowables() => allowables;
        public List<T> GetAvailalbes() => availables;


        public void AddAllowable(T item)
        {
            if (allowables == null) allowables = new List<T>();
            allowables.Add(item);
        }
        public void RemoveAllowable(T item)
        {
            if (allowables == null) allowables = new List<T>();
            if (allowables.Contains(item)) allowables.Remove(item);
        }
        public void RemoveAllowableAll(T remove)
        {
            if (allowables == null) allowables = new List<T>();
            allowables.RemoveAll(x => x == remove);        
        }

        public void RemoveSomeAvailables(List<T> remove)
        {
            if (availables == null) availables = new List<T>();
            availables.RemoveAll(x => remove.Contains(x));
        }
        public void RemoveAvailables(T remove)
        {
            if (availables == null) availables = new List<T>();
            if (availables.Contains(remove)) availables.Remove(remove);
        }
        public void RemoveAvailablesAll(T remove)
        {
            if (availables == null) availables = new List<T>();
            availables.RemoveAll(x => x == remove);
        }

        public void AddAvailables(T item)
        {
            // dont want to add a not allowed item (e.g. reward) into availables
            if (allowables.Contains(item)) availables.Add(item);
        }


        public void TryShuffle()
        {
            if (availables == null) availables = new List<T>();
            if (availables.Count > 0) return;
            if (allowables == null || allowables.Count == 0) return;

            availables.Clear();
            availables.AddRange(allowables);         
            availables.Shuffle();
        }
    }
}
