using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MadApper
{
    public class RendererTweaker : MonoBehaviour
    {

        public Renderer[] Renderers;

        public Color HighlightColor;


        Color[] originalColor;
        Color[] m_OriginalColor
        {
            get
            {
                if (originalColor == null || originalColor.Length == 0)
                {
                    foreach (var item in Renderers)
                    {
                        if (!item.gameObject.activeInHierarchy)
                            continue;

                        originalColor = new Color[item.materials.Length];

                        for (int i = 0; i < item.materials.Length; i++)                        
                            originalColor[i] = item.materials[i].color;
                        
                        break;
                    }      
                }
                return originalColor;
            }
        }




        private void Start()
        {
            if (Renderers == null || Renderers.Length == 0)
                return;

            var orgSetter = m_OriginalColor;
        }

        public void z_ResetOrgColor()
        {
            originalColor = null;

            if (Renderers == null || Renderers.Length == 0)
                return;

            var orgSetter = m_OriginalColor;
        }

        public void z_HighlightColorForSeconds(float duration)
        {
            if (Renderers == null || Renderers.Length == 0)
                return;

            var orgSetter = m_OriginalColor;

            foreach (var item in Renderers)
            {
                if (!item.gameObject.activeInHierarchy)
                    continue;

                var mats = item.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (m_OriginalColor.Length <= i)
                        break;

                    var sq = DOTween.Sequence();
                    var mat = item.materials[i];
                    var orgColor = m_OriginalColor[i];

                    sq.Append(mat.DOColor(HighlightColor, duration / 2).SetEase(Ease.Linear));
                    sq.Append(mat.DOColor(orgColor, duration / 2).SetEase(Ease.Linear));
                }

            }


        }

        public void z_HighlightColor(float duration)
        {
            if (Renderers == null || Renderers.Length == 0)
                return;

            var orgSetter = m_OriginalColor;

            foreach (var item in Renderers)
            {
                if (!item.gameObject.activeInHierarchy)
                    continue;

                var mats = item.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var mat = item.materials[i];
                    mat.DOColor(HighlightColor, duration).SetEase(Ease.Linear);
                }
            }

          
        }
        public void z_DeHighlightColor(float duration)
        {
            if (Renderers == null || Renderers.Length == 0)
                return;

            foreach (var item in Renderers)
            {
                if (!item.gameObject.activeInHierarchy)
                    continue;

                var mats = item.materials;

                for (int i = 0; i < mats.Length; i++)
                {
                    if (m_OriginalColor.Length <= i)
                        break;

                    var mat = item.materials[i];
                    var orgColor = m_OriginalColor[i];
                    mat.DOColor(orgColor, duration).SetEase(Ease.Linear);
                }
            }
          
        }

        public async void z_DeHighlightColorAfter(float duration)
        {
            if (Renderers == null || Renderers.Length == 0)
                return;

            await UniTask.WaitForSeconds(duration);
            z_DeHighlightColor(.1f);
        }
    }
}
