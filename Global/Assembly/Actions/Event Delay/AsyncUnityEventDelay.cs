using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MadApper
{
    [Serializable]
    public class AsyncUnityEventDelay : UnityEventDelay
    {
        public new async UniTask InvokeAsync()
        {
            float delay = Random.Range(m_Delay, m_Delay + m_RandomizeRange);

            if (delay > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
            }

            if (await TryInvokeDelayableAsync())
            {
                return;
            }

            m_Event?.Invoke();
        }

        public new void Invoke()
        {
            InvokeAsync().Forget();
        }

        private async UniTask<bool> TryInvokeDelayableAsync()
        {
            for (int i = 0; i < m_Event.GetPersistentEventCount(); i++)
            {
                UnityEngine.Object target = m_Event.GetPersistentTarget(i);
                IEventDelayable delayable = GetDelayableFrom(target);

                if (delayable != null)
                {
                    await delayable.InvokeAsync();
                    return true;
                }
            }
            return false;
        }

        private IEventDelayable GetDelayableFrom(UnityEngine.Object target)
        {
            if (target is IEventDelayable eventDelayable)
            {
                return eventDelayable;
            }

            if (target is Component comp)
            {
                return comp.GetComponent<IEventDelayable>();
            }

            if (target is GameObject go)
            {
                return go.GetComponent<IEventDelayable>();
            }

            return null;
        }
    }

    public interface IEventDelayable
    {
        UniTask InvokeAsync();
    }

    [Serializable]
    public class AsyncUnityEventDelayList
    {
        public List<AsyncUnityEventDelay> m_List;

        public async UniTask InvokeAsync()
        {
            List<UniTask> tasks = new List<UniTask>();
            
            foreach (AsyncUnityEventDelay item in m_List)
            {
                if (item != null)
                {
                    tasks.Add(item.InvokeAsync());
                }
            }
            
            await UniTask.WhenAll(tasks);
        }

        public void Invoke()
        {
            foreach (AsyncUnityEventDelay item in m_List)
            {
                item?.Invoke();
            }
        }
    }

    [Serializable]
    public class AsyncUnityEventDelay<T> : UnityEventDelay<T>
    {
        public async UniTask InvokeAsync(T obj)
        {
            if (m_Delay > 0)
            {
                if (cts != null)
                {
                    cts.Cancel();
                    cts.Dispose();
                }

                cts = new CancellationTokenSource();

                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(m_Delay), cancellationToken: cts.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }

            m_Event?.Invoke(obj);
        }

        public new void Invoke(T obj)
        {
            InvokeAsync(obj).Forget();
        }
    }

    [Serializable]
    public class AsyncUnityEventDelayList<T>
    {
        public List<AsyncUnityEventDelay<T>> m_List;

        public async UniTask InvokeAsync(T obj)
        {
            List<UniTask> tasks = new List<UniTask>();
            
            foreach (AsyncUnityEventDelay<T> item in m_List)
            {
                if (item != null)
                {
                    tasks.Add(item.InvokeAsync(obj));
                }
            }
            
            await UniTask.WhenAll(tasks);
        }

        public void Invoke(T obj)
        {
            foreach (AsyncUnityEventDelay<T> item in m_List)
            {
                item?.Invoke(obj);
            }
        }
    }
}