using UnityEngine;

namespace MadApper
{
    public interface ISingletonSO<TSO>
    {
       static TSO Instance { get; }
    }

    public abstract class SingletonScriptable<TSO> : ScriptableObject
        where TSO : SingletonScriptable<TSO>
    {

#if UNITY_EDITOR
        public static TSO GetSO() => MADUtility.GetOrCreateSOAtEssentialsFolder<TSO>();
#endif

        static TSO instance;
        public static TSO Instance
        {
            get
            {
                if (instance == null)                
                    instance = Resources.Load<TSO>(typeof(TSO).Name);                
                return instance;
            }
        }

    }
}
