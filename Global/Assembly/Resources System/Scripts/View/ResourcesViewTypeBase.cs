using BEH;
using TMPro;
using UnityEngine;


namespace MadApper
{
    public abstract class ResourcesViewTypeBase : MonoBehaviour
    {
        [Space(10)] public ResourceType m_Type;
        [Space(10)] public TextMeshProUGUI m_Text;
#if ANIMATIONSEQ_ENABLED
        [Space(10)] public Appearer m_Appearer; 
#endif
        [Space(20)] public UnityEventDelayList m_OnActivationEvents;

        public virtual void OnResourceChagend(ResourceData data)
        {
            m_Text?.SetText(GetText(data));
        }
        public abstract string GetText(ResourceData data);
    }
}