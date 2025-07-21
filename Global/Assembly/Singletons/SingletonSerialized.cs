using Sirenix.OdinInspector;
using UnityEngine;

namespace MadApper.Singleton {
    public abstract class SingletonSerialized<T> : SerializedMonoBehaviour where T : SingletonSerialized<T> {
        private static volatile T _instance;
        private static readonly object _lock = new();

        [SerializeField]
        private bool dontDestroyOnLoad;

        public static T Instance {
            get {
                if (_instance == null) {
                    lock (_lock) {
                        if (_instance == null) {
                            _instance = FindObjectOfType<T>();
                        }
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake() {
            if (_instance != null) {
                Destroy(gameObject);
                return;
            }

            _instance = (T)this;

            if (dontDestroyOnLoad) {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnDestroy() {
            if (_instance == this) {
                _instance = null;
            }
        }
    }
}