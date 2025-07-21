using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


namespace BEH
{
    public interface IPoolable
    {
        public string i_PoolID { get; set; }
        public bool i_InPool { get; set; }
        public void i_OnSpawned(bool instantiated);
    }

    public abstract class Pool
    {
        public abstract void Initialize();
        public abstract void Reset();
        public abstract void OnSceneUnloaded(string scene);
        public abstract void OnSceneChanged(string scene);

    }
    public abstract class Pool<T> : Pool where T : MonoBehaviour, IPoolable
    {

        static Dictionary<string, Queue<T>> s_Queues;
        static Dictionary<int, Transform> s_Parents;

        public override void Initialize()
        {
            s_Queues = new Dictionary<string, Queue<T>>();
            s_Parents = new Dictionary<int, Transform>();
        }
        public override void Reset()
        {
            s_Queues = null;

            foreach (var item in s_Parents)
                if (item.Value != null)
                    Object.Destroy(item.Value);
        }

        public override void OnSceneUnloaded(string scene) { }
        public override void OnSceneChanged(string scene) { }


        public static T Get(string id, T prefab)
        {
            return Get(id, prefab, parent: null);
        }
        public static T Get(string id, T prefab, Transform parent)
        {
            T item = TryDequeueItem(id);
            var instantiated = item == null;

            if (instantiated) item = CreateItem(id, prefab, parent);
            else
            {
                item.transform.SetParent(parent);
                item.gameObject.SetActive(true);
            }

            item.i_InPool = false;
            item.i_OnSpawned(instantiated: instantiated);

            return item;
        }
        public static T Get(string id, Func<Transform, T> createFunc, Transform parent)
        {
            T item = TryDequeueItem(id);
            var create = item == null;

            if (create)
            {
                item = createFunc(parent);
                item.i_PoolID = id;
            }
            else
            {
                item.transform.SetParent(parent);
                item.gameObject.SetActive(true);
            }

            item.i_InPool = false;
            item.i_OnSpawned(instantiated: create);

            return item;
        }

        static T TryDequeueItem(string id)
        {
            if (s_Queues.ContainsKey(id) && s_Queues[id].Count != 0) return s_Queues[id].Dequeue();
            return null;
        }

        static T CreateItem(string id, T prefab, Transform parent)
        {
            var item = Object.Instantiate(prefab, parent);
            item.i_PoolID = id;
            return item;
        }


        public static void Despawn(T item)
        {
            if (item.i_InPool) return;

            item.transform.SetParent(GetParent());
            item.gameObject.SetActive(false);
            item.transform.localScale = Vector3.one;
            item.transform.rotation = Quaternion.Euler(Vector3.zero);

            var id = item.i_PoolID;

            if (string.IsNullOrEmpty(id)) return;
            if (!s_Queues.ContainsKey(id)) s_Queues[id] = new Queue<T>();

            item.i_InPool = true;

            s_Queues[id].Enqueue(item);
        }

        static Transform GetParent()
        {
            var index = SceneManager.GetActiveScene().buildIndex;

            if (!s_Parents.ContainsKey(index))
            {
                var parent = new GameObject($"Pool=>{typeof(T)}").transform;
                s_Parents.Add(index, parent);
            }

            return s_Parents[index];
        }

    }



}