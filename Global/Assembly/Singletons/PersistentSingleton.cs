using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MadApper.Singleton
{

    public abstract class PersistentSingleton<T> : MonoBehaviour where T : PersistentSingleton<T>
    {
        private static T _instance;
        public static T Instance { get { return _instance; } }

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