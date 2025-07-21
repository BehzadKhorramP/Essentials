using UnityEngine;

namespace MadApper.Essentials
{
    public class RendererShadowTweakerRecursive : MonoBehaviour
    {
        public void z_ShadowOnAll()
        {
            var list = GetComponentsInChildren<RendererShadowTweaker>(true);
            if (list != null)
                foreach (var item in list)
                    item.z_ShadowOn();
        }
        public void z_ShadowOffAll()
        {
            var list = GetComponentsInChildren<RendererShadowTweaker>(true);
            if (list != null)
                foreach (var item in list)
                    item.z_ShadowOff();
        }
    }

}
