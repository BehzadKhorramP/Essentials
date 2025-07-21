using BEH.Common;
using UnityEditor;
using UnityEngine;

namespace MadApper
{
    [CreateAssetMenu(fileName = "IDSO", menuName = "Utils/IDSO")]

    public class IDSO : ScriptableObject
    {
        public string ID;

#if UNITY_EDITOR
        private void OnValidate()
        {
            this.TrySetIDbyNameMatch(ref ID);
        }
#endif

        public static bool operator ==(IDSO a, IDSO b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.ID == b.ID;
        }

        public static bool operator !=(IDSO a, IDSO b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is IDSO other)
            {
                return ID == other.ID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ID?.GetHashCode() ?? 0;
        }


    }
}
