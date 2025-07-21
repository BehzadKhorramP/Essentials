using UnityEngine;


namespace MadApper
{
    public class ResourceViewSimple : ResourcesViewTypeBase
    {
        [Space(10)] public ResourceItemSO.TextRule m_TextRule = new();

        public override string GetText(ResourceData data)
        {
            return $"{m_TextRule.Prefix}{data.Amount}{m_TextRule.Suffix}";
        }
    }

}