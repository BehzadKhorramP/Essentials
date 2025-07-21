using System;
using UnityEngine;

namespace BEH
{
    [Serializable]
    public class RendererToSort<TRenderer> where TRenderer : Renderer
    {
        public TRenderer Renderer;
        public int InitialSortingOrder;
        public bool OnValidate()
        {
            var setDirty = Renderer.sortingOrder != InitialSortingOrder;
            InitialSortingOrder = Renderer.sortingOrder;
            return setDirty;
        }

        public void Setup()
        {
            Renderer.sortingOrder = InitialSortingOrder;
        }

        public void SetSortingOrder(int order)
        {
            Renderer.sortingOrder = InitialSortingOrder + order;
        }
        public void AddSortingOrder(int add)
        {
            Renderer.sortingOrder += add;
        }
        public void SubSortingOrder(int add)
        {
            Renderer.sortingOrder -= add;
        }
    }

    [Serializable]
    public class RendererToSort : RendererToSort<Renderer> { }

    [Serializable]
    public class SpriteRendererToSort : RendererToSort<SpriteRenderer> { }


}
