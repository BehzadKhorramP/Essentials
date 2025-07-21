using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Threading;
using UnityEngine;

namespace BEH.Voodoo
{
    public abstract class VoodooTuneConfigSO : ScriptableObject
    {
        public abstract void Retrieve(Action onComplete);

        public abstract void OnReset();

    }

    // if a new scriptable is created, VoodooTuneConfigsRetriever should be once opened to update the list of items!

    public abstract class VoodooTuneConfigSO<T> : VoodooTuneConfigSO where T : class, new()
    {
        public enum State { Local, Remote }

        [ReadOnly][Space(10)] bool Initialized;

        [Space(10)] public State m_State = State.Local;

        [Space(10)][SerializeField] T m_LocalConfig = default;

        [Space(20)][SerializeField] T m_RemoteConfig;

        public override void Retrieve(Action onComplete)
        {
#if VOODOOSAUCE_ENABLED
            $"Retrieving [{typeof(T)}] ...".LogBlue();

            VoodooSauceUtility.TryInitVSAndGetRemoteData<T>((x) =>
            {
                m_RemoteConfig = x;

                $"[{typeof(T)}] Retrieved : [{m_RemoteConfig}]".LogBlue();

                onComplete?.Invoke();

                Initialized = true;
            });
#endif
        }

        public override void OnReset()
        {
            Initialized = false;
        }

        public T Get()
        {
            var state = m_State;

#if !UNITY_EDITOR
            state = State.Remote;
#endif
            switch (state)
            {
                case State.Local:
                    return m_LocalConfig;
                case State.Remote:
                    return m_RemoteConfig;
            }

            return m_RemoteConfig;
        }

        public async UniTask<T> GetAsync(CancellationToken ct)
        {
            if (!Initialized)
                Retrieve(null);

            await UniTask.WaitUntil(() => Initialized == true, cancellationToken: ct).SuppressCancellationThrow();

            return Get();
        }
    }

}