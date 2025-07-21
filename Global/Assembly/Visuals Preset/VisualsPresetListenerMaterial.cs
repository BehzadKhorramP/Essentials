using System.Collections.Generic;
using UnityEngine;

namespace BEH.VisualsPreset
{
    public class VisualsPresetListenerMaterial : VisualsPresetListener<MaterialVisualsSet, Material>
    {
        [SerializeField] List<Renderer> m_Renderers;

        protected override void UpdateVisuals(MaterialVisualsSet visualSet)
        {
            base.UpdateVisuals(visualSet);

            if (m_Renderers == null || m_Renderers.Count == 0)
                return;

            var value = visualSet.GetValue() ;

            if (value == null)
                return;

            foreach (var item in m_Renderers)
            {
                item.sharedMaterial = value;
            }
        }
   

    }
}
