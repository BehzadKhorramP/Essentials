using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Essentials
{
    public class ParticlesUtils : MonoBehaviour
    {
        [SerializeField] List<ParticleSystem> particleSystems;

        public void z_SetColor(ParticleSystem.MinMaxGradient color)
        {
            if (particleSystems == null) return;

            foreach (var item in particleSystems)
            {
                var main = item.main;
                main.startColor = color;
            }
        }


        public void z_SetColor(Color color)
        {
            z_SetColor(new ParticleSystem.MinMaxGradient(color));
        }
    }

}
