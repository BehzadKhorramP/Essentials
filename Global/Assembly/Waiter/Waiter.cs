using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MadApper
{
    public class Waiter
    {
        protected CancellationTokenSource cts;
        protected List<Func<UniTask>> tasksList;

        protected Waiter()
        {
            tasksList = new List<Func<UniTask>>();
            cts = new CancellationTokenSource();
        }
        public void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
            cts = null;
        }

        /// <summary>
        /// onWaited will be triggered when taskslist finishes successfully,
        /// if it stopped, onWaited wont be triggered! 
        /// </summary>
        /// <param name="onWaited"></param>
        /// <returns></returns>
        public async UniTask Run(Action onWaited)
        {
            Stop();

            cts = new CancellationTokenSource();

            try
            {
                await RunInternal().AttachExternalCancellation(cts.Token);
                onWaited?.Invoke();
            }
            catch (OperationCanceledException) { }
        }


        protected async UniTask RunInternal()
        {
            foreach (var taskFunc in tasksList)
            {
                await taskFunc().AttachExternalCancellation(cts.Token);
            }
        }
        protected async UniTask WaitForTime(float duration)
        {
            await UniTask.WaitForSeconds(duration, cancellationToken: cts.Token);
        }
        protected async UniTask Log(string message)
        {
            $"{message}".Log();

            await UniTask.Delay(1, cancellationToken: cts.Token);
        }


        public class Builder<TWaiter, TBuilder>
            where TWaiter : Waiter, new()
            where TBuilder : Builder<TWaiter,TBuilder>
        {
            protected readonly TWaiter source = new TWaiter();


            public TBuilder WithWaitTime(float duration)
            {
                source.tasksList.Add(() => source.WaitForTime(duration));
                return (TBuilder)this;
            }
            public TBuilder WithLog(string message)
            {
                source.tasksList.Add(() => source.Log(message));
                return (TBuilder)this;
            }
            public TBuilder WithCustom(Func<CancellationToken, UniTask> taskFunc)
            {
                source.tasksList.Add(() => taskFunc(source.cts.Token));
                return (TBuilder)this;
            }
            public TWaiter Build() => source;
                     
        }

    }
}
