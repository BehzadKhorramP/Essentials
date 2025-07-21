using UnityEngine;

namespace MadApper.Singleton
{

    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        static T _instance;
        static T Instance { get { return _instance; } }

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
            }
        }

        // unset the instance if this object is destroyed
        protected virtual void OnDestroy()
        {
            _instance = null;
        }

        public static T GetInstance() => Instance;
    }

}