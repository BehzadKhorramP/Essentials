using Sirenix.OdinInspector;
using UnityEngine;

namespace MadApper
{
    /// <summary>
    /// <typeparamref name="TSO"/> is Project Specific and AutoGetOrCreateSO will only trigger when component explicitly selected in editor or the hierarchy changes
    /// so as MadApperEssentials is shared between different projects it's easy the reference becomes null with a new push from another project
    /// so this class makes sure the singleton instance being returned if the explicit reference is lost
    /// yet the singleton scriptable itself should have been created in the projcet beforehand
    /// </summary>   
    [System.Serializable]
    public class SingletonScriptableHelper<TSO> where TSO : SingletonScriptable<TSO>
    {
        [AutoGetOrCreateSO][ReadOnly][SerializeField] protected TSO m_AutoAssingedSO;
        public TSO RunTimeValue
        {
            get
            {
                if (m_AutoAssingedSO == null)                
                    m_AutoAssingedSO = SingletonScriptable<TSO>.Instance;                
                
                return m_AutoAssingedSO;
            }
        }
        
    }
}
