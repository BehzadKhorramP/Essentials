
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MadApper
{
    public abstract class ObjectsCollectionSO<TOBJ> : ScriptableObject where TOBJ : UnityEngine.Object
    {
        [SerializeField] bool m_Retrievable;

        public List<TOBJ> Values;

#if UNITY_EDITOR
        [ShowIf(nameof(m_Retrievable))]
        [PropertySpace(10, 10)]
        [Button(ButtonSizes.Large)]
        public void Retrieve()
        {
            Values = MADUtility.GetAllInstances_Editor<TOBJ>().ToList();
            this.TrySetDirty();
        }
#endif
        public void Clear()=> Values.Clear();

        public void Add(TOBJ obj)
        {           
            Values.Add(obj);
        }
        public void Remove(TOBJ obj)
        {
            if (!Values.Contains(obj))
                return;
            Values.Remove(obj);
        }

    }
}
