using Cysharp.Threading.Tasks;
using DG.Tweening;
using MadApper;
using System;
using UnityEngine;

namespace BEH
{
    public class PoolVFX : Pool<VFX> { }

    public class VFX : MonoBehaviour, IPoolable
    {
        [Space(10)] public ParticleSystem[] m_ExtraParticles;
        [Space(10)] public ParticleSystem[] m_TrailParticles;
        [Space(10)] public ParticleSystem m_MoveParticles;
        [Space(10)] public ParticleSystem m_HitParticles;
        [Space(10)] public TrailRenderer m_Trail;
        [Space(10)][SerializeField] UnityEventDelayList m_OnStarted;

        bool colorsSet;

        Args args;

        private void OnDestroy()
        {
            transform.DOKill();
        }


        #region IPoolable

        public bool i_InPool { get; set; }

        public string i_PoolID { get; set; }

        public void i_OnSpawned(bool instantiated)
        {
            transform.localScale = Vector3.one;

            ResetAll();
        }

        public void Despawn()
        {
            PoolVFX.Despawn(this);
        }

        #endregion


        void ResetAll()
        {
            if (m_TrailParticles != null)
                foreach (var item in m_TrailParticles)
                    ResetParticle(item);
            if (m_ExtraParticles != null)
                foreach (var item in m_ExtraParticles)
                    ResetParticle(item);
            ResetParticle(m_HitParticles);
            ResetParticle(m_MoveParticles);
            ResetTrail();
        }

        void ResetParticle(ParticleSystem p)
        {
            if (p == null)
                return;
            p.Stop();
        }
        void ResetTrail()
        {
            if (m_Trail == null)
                return;
            m_Trail.emitting = false;
        }
        async void PlayTrail()
        {
            if (m_Trail == null)
                return;
            await UniTask.DelayFrame(1);
            m_Trail.emitting = true;
        }
        void RefreshColor(Color? ncolor, bool notFadeColor2)
        {
            if (colorsSet)
                return;

            if (!ncolor.HasValue)
                return;

            var color = ncolor.Value;
            var mainColor = color;
            var maxColor = color;

            if (m_Trail != null)
            {
                var gradient = new Gradient();

                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(mainColor, 0), new GradientColorKey(maxColor, 1) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(.5f, 1) });

                m_Trail.colorGradient = gradient;
            }

            if (!notFadeColor2)
                maxColor.a = .5f;

            var startColor = new ParticleSystem.MinMaxGradient(mainColor, maxColor);

            if (m_TrailParticles != null)
                foreach (var item in m_TrailParticles)
                {
                    var main = item.main;
                    main.startColor = startColor;
                }
            if (m_ExtraParticles != null)
                foreach (var item in m_ExtraParticles)
                {
                    var main = item.main;
                    main.startColor = startColor;
                }

            if (m_HitParticles != null)
            {
                var main = m_HitParticles.main;
                main.startColor = startColor;
            }

            colorsSet = true;
        }



        public async void OnStarted(Args args)
        {
            this.args = args;

            RefreshColor(args.Color, args.NotFadeColor2);

            await UniTask.DelayFrame(1);

            if (args.Delay > 0)
                await UniTask.WaitForSeconds(args.Delay);

            m_OnStarted?.Invoke();

            if (m_TrailParticles != null)
                foreach (var item in m_TrailParticles)
                    item.Play();

            PlayTrail();

            if (m_MoveParticles != null)
                m_MoveParticles?.Play();
        }

        public async void OnEnded()
        {
            PlayHitParticles();

            if (m_MoveParticles != null)
                m_MoveParticles?.Stop();

            if (m_TrailParticles != null)
                foreach (var item in m_TrailParticles)
                    item.Stop();

            await UniTask.WaitForSeconds(2);

            ResetTrail();

            if (!string.IsNullOrEmpty(i_PoolID))
                Despawn();
        }


        public void PlayHitParticles()
        {
            if (m_HitParticles != null)
                m_HitParticles?.Play();
        }

        [Serializable]
        public struct Args
        {
            public Color? Color;
            public bool NotFadeColor2;
            public float Delay;
        }
    }
}
