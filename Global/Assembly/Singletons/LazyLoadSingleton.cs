using UnityEngine;

namespace MadApper.Singleton
{

    public abstract class LazyLoadSingleton<T> : MonoBehaviour where T : LazyLoadSingleton<T>
    {

        private static T _instance;

        public static void InitializeInstance(string prefabname)
        {
            if (_instance == null)
            {
                var res = Resources.Load<T>(prefabname);

                if (res != null)
                    Instantiate(res);
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
            }
        }

        // unset the instance if this object is destroyed
        protected virtual void OnDestroy()
        {
            _instance = null;
        }
    }

}