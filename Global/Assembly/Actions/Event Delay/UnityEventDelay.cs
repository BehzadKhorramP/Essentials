using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Threading;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using MadApper.Essentials;

namespace MadApper
{
    [Serializable]
    public class UnityEventDelay
    {
        [Space(10)] public float m_Delay;
        [Space(10)] public float m_RandomizeRange;
        [Space(10)] public UnityEvent m_Event;

        CancellationTokenSource cts;

        public void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
            cts = null;
        }

        public async void Invoke()
        {
            Stop();

            var delay = Random.Range(m_Delay, m_Delay + m_RandomizeRange);

            if (delay > 0)
            {
                cts = new CancellationTokenSource();

                try
                {
                    await UniTask.WaitForSeconds(delay, cancellationToken: cts.Token);
                }
                catch (Exception) { return; }
            }

            m_Event?.Invoke();
        }
    }

    [Serializable]
    public class UnityEventDelayList
    {
        public List<UnityEventDelay> m_List;

        public void Stop()
        {
            if (m_List == null) return;
            foreach (var item in m_List)item.Stop();            
        }
        public void Invoke()
        {
            if (m_List == null) return;
            foreach (var item in m_List) item?.Invoke();
        }
    }

    [Serializable]
    public class UnityEventDelay<T> : IDisposable
    {
        [Space(10)] public float m_Delay;
        [Space(10)] public float m_RandomizeRange;
        [Space(10)] public UnityEvent<T> m_Event;

        protected CancellationTokenSource cts;
        public void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
            cts = null;
        }
        public async void Invoke(T obj)
        {
            Stop();

            var delay = Random.Range(m_Delay, m_Delay + m_RandomizeRange);

            if (delay > 0)
            {
                cts = new CancellationTokenSource();

                try
                {
                    await UniTask.WaitForSeconds(delay, cancellationToken: cts.Token);
                }
                catch (Exception) { return; }
            }

            m_Event?.Invoke(obj);
        }

        public void Dispose()
        {
            Stop();
        }
    }

    [Serializable]
    public class UnityEventDelayList<T>
    {
        public List<UnityEventDelay<T>> m_List;

        public void Stop()
        {
            if (m_List == null) return;
            foreach (var item in m_List) item.Stop();
        }
        public void Invoke(T obj)
        {
            if (m_List == null) return;
            foreach (var item in m_List) item?.Invoke(obj);
        }
    }





    [Serializable]
    public class UnityEventDelayEditor<T>
    {
        [Space(10)] public float m_Delay;
        [Space(10)] public UnityEvent<T> m_Event;

        protected CancellationTokenSource cts;
        public void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
            cts = null;
        }

        public async void Invoke(T obj)
        {           
            Stop();

#if UNITY_EDITOR
            if (m_Delay > 0)
            {
                cts = new CancellationTokenSource();

                try
                {
                    await UniTaskUtils.WaitForSecondsEditor(m_Delay, cancellationToken: cts.Token); 
                }
                catch (Exception) { return; }
            }
#endif

            m_Event?.Invoke(obj);
        }
    }







}