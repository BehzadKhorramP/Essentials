using System.Collections;
using UnityEngine;

namespace MadApper.Essentials
{

    public class RendererShadowTweaker : MonoBehaviour
    {
        [SerializeField][AutoGetOrAdd] Renderer m_Renderer;

        public void z_ShadowOn() => m_Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        public void z_ShadowOff() => m_Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

}
