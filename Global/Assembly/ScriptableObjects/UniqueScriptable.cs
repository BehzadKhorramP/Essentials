using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace MadApper
{
    public abstract class UniqueScriptable : ScriptableObject, ILookupObject
    {
        [Title("Unique Scriptable Object")]

        [SerializeField][ReadOnly] protected string uId;
        [SerializeField][ReadOnly] protected int hash;
        [SerializeField][ReadOnly] protected string lookupIndicator;

        #region ILookupObject
        public string i_LookupIndicator { get => lookupIndicator; set => lookupIndicator = value; }

        #endregion


#if UNITY_EDITOR

        [Button]
        void GenerateNewHash()
        {
            uId = Guid.NewGuid().ToString("N");
            hash = uId.FNV1aHash();
            this.TrySetDirty();
        }
#endif


        #region Equals

        public string GetUID() => uId;

        public static bool operator ==(UniqueScriptable a, UniqueScriptable b)
        {          
            // Handle nulls
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.hash == b.hash;
        }

        public static bool operator !=(UniqueScriptable a, UniqueScriptable b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            return hash == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return hash;
        }


        #endregion

    }
}
