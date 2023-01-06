using UnityEngine;

#nullable enable

namespace jwellone
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        static T? _instance;
        public static T instance => _instance ??= FindObjectOfType<T>();

        [SerializeField] bool _isDontDestroyOnLoad = true;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = (T)this;

            if (_isDontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            OnAwakened();
        }

        void OnDestroy()
        {
            OnDestroyed();
            if (_instance == this)
            {
                _instance = null;
            }
        }

        protected abstract void OnAwakened();
        protected abstract void OnDestroyed();
    }
}
