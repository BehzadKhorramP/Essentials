using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Singleton
{

    public abstract class LazyPersistentSingleton<T> : MonoBehaviour where T : LazyPersistentSingleton<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject($"{typeof(T)} - Lazy Persistent Singleton");
                    _instance = go.AddComponent<T>();                   
                }
                return _instance;
            }
        }

        // self-destruct if another instance already exists
        protected virtual void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = (T)this;
                DontDestroyOnLoad(this.gameObject);
            }
        }

        // unset the instance if this object is destroyed
        protected virtual void OnDestroy()
        {
            _instance = null;
        }
    }

}