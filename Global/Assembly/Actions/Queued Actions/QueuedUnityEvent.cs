using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using System;
using System.Threading;
using UnityEngine;

namespace MadApper
{
    [Serializable]
    public class QueuedUnityEvent
    {
        [SerializeField] UnityEventDelayList events;
        [SerializeField] float duration;

        protected CancellationTokenSource cts;

        public async void Invoke(Action onCompleted)
        {
            Stop();

            events?.Invoke();

            if (duration > 0)
            {
                cts = new CancellationTokenSource();

                try
                {
                    await UniTask.WaitForSeconds(duration, cancellationToken: cts.Token);
                }
                catch (Exception) { return; }
            }

            onCompleted?.Invoke();
        }
        public void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
            cts = null;

            events?.Stop();
        }

    }

    public static class QueuedUnityEventExtentions
    {
        public static void Invoke(params QueuedUnityEvent[] events)
        {
            var actions = new RecursiveActions();

            events.ForEach(item => { actions.Add((onCompleted) => item?.Invoke(onCompleted)); });   
            
            actions.Execute();
        }
    }

}