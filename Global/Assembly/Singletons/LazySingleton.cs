using UnityEngine;
namespace MadApper.Singleton
{

    public abstract class LazySingleton<T> : MonoBehaviour where T : LazySingleton<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject($"{typeof(T)} - Lazy Singleton");
                    _instance = go.AddComponent<T>();
                    Debug.Log($"{go.name} Instantiated");
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
                OnAwaked();
            }
        }

        protected virtual void OnAwaked() { }

        // unset the instance if this object is destroyed
        protected virtual void OnDestroy()
        {
            _instance = null;
        }
    }

}