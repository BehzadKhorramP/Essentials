
#if UNITY_EDITOR || BEH_DEBUG
#define DEBUG
#endif

using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Text;


namespace BEH.Common
{

    [System.Serializable]
    public class QBool
    {

        public QBool(bool active = true)
        {
            activeCount = active ? 0 : -1;
        }

        [ShowInInspector, ReadOnly]
        int activeCount;
        public void Clear()
        {
#if DEBUG
            lockTags = null;
#endif
            activeCount = 0;
        }

        public bool IsTrue => activeCount >= 0;

        public static bool operator ==(QBool a, bool b)
        {
            if (a == null)
            {
                return false;
            }

            return a.IsTrue == b;
        }

        public static bool operator !=(QBool a, bool b)
        {
            return !(a == b);
        }

        public static QBool operator ++(QBool a)
        {
            a.activeCount++;

            if (a.activeCount >= 0)
                a.activeCount = 0;

            return a;

        }

        public static QBool operator --(QBool a)
        {
            a.activeCount--;
            return a;
        }



#if DEBUG

        private List<string> lockTags;
        [ShowInInspector, ReadOnly]
        public List<string> m_LockTags
        {
            get
            {
                if (lockTags == null)
                    lockTags = new List<string>();
                return lockTags;
            }
        }
#endif

        public void Lock(string tag)
        {
            activeCount--;

#if DEBUG
            if (!string.IsNullOrEmpty(tag))
                m_LockTags.Add(tag);
#endif
        }
        public void Unlock(string tag)
        {
            activeCount++;

            if (activeCount >= 0)
                activeCount = 0;

#if DEBUG
            if (!string.IsNullOrEmpty(tag))
                m_LockTags.Remove(tag);
#endif
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return activeCount.GetHashCode();
        }

        public override string ToString()
        {
            var bstr = $"{IsTrue}, Count: {activeCount}";
#if DEBUG
            var stringbuilder = new StringBuilder();
            stringbuilder.Append(bstr);
            if (m_LockTags.Count > 0)
            {
                stringbuilder.Append("\n");
                stringbuilder.AppendJoin(",", m_LockTags);
            }

            bstr = stringbuilder.ToString();
#endif
            return bstr;
        }
    }


}