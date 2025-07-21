
using UnityEngine;

namespace MadApper
{
    public class ResourcesUtility : MonoBehaviour
    {
        public ResourceItemSO m_ResourceSO;
        public int m_Amount;

        public void z_GiveResourceDebug()
        {
            m_ResourceSO.Modify(m_Amount);
        }
    }
}
