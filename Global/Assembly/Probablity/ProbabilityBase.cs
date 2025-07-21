
using MadApper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BEH
{
    [Serializable]
    public class ProbabilityBase<T>
    {
        [Serializable]
        public class Item
        {
            public T Value;
            public int Probability = 10;
        }

        public List<Item> Items;
        private System.Random random = new System.Random();

        public ProbabilityBase()
        {
            Items = new List<Item>();
        }

        public bool IsValid() => this != null && Items != null && Items.Any();

        public void Setup(List<Item> items)
        {
            Items = new List<Item>(items);
        }
        public void Add(Item item)
        {
            Items.Add(item);
        }
        public void Remove(Item item)
        {
            if (Items.Contains(item))
                Items.Remove(item);
        }

        public Item GetRandomItem()
        {
            var res = Items[0];

            float max = .01f;

            foreach (var item in Items)
                max += item.Probability;

            float rand = Random.Range(0f, max);
            float cur = 0;

            foreach (var item in Items)
            {
                cur += item.Probability;

                if (cur > rand)
                {
                    res = item;
                    break;
                }
            }

            return res;
        }
        public T GetRandomValue()
        {           
            return GetRandomItem().Value;
        }

        public void Shuffle()
        {
            Items.Shuffle();
        }

        public T GetIndependent100BasedRandom()
        {
            if (Items.Count == 0)
                return default;

            foreach (var item in Items)
            {
                int randomValue = random.Next(100);

                if (randomValue <= item.Probability)
                    return item.Value;
            }

            return default; 
        }
    }

    public abstract class ProbabilitySO<TValue> : ScriptableObject
    {
        public string ID;

        public ProbabilityBase<TValue> Probability;

        public bool IsValid() => this != null && Probability.IsValid();
    }

}
