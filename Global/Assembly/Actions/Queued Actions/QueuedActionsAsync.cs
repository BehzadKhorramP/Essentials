using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace MadApper
{
    public abstract class QueuedActionsAsync<TSender, TQueuedActions>
        where TQueuedActions : QueuedActionsAsync<TSender, TQueuedActions>
    {
        TSender sender;

        List<ActionAsync> actions;
        CancellationTokenSource cts;

        protected bool debug;

        public static Action<TQueuedActions> s_OnAsyncQueuedActions;

        public QueuedActionsAsync(TSender sender, bool debug = false)
        {
            this.sender = sender;
            this.debug = debug;

            actions = new List<ActionAsync>();
            cts = new CancellationTokenSource();
        }

        public abstract TQueuedActions GetSelf();
        public TSender GetSender() => sender;

        public void CollectActions()
        {
            s_OnAsyncQueuedActions?.Invoke(GetSelf());
        }
        public CancellationToken GetCToken() => cts.Token;

        public void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
            cts = null;
        }

        public void Append(ActionAsync action)
        {
#if UNITY_EDITOR
            if (debug) $"Appended : {action} - to : {sender}".Log();
#endif
            actions.Add(action);
        }
        public async UniTask Execute()
        {
            var sorted = actions.OrderBy(x => x.GetPriority());
            var cToken = cts.Token;

            try
            {
                foreach (var item in sorted)
                {
#if UNITY_EDITOR
                    if (debug) $"Started {item}".Log();
#endif
                    await item.Execute().AttachExternalCancellation(cToken);

#if UNITY_EDITOR
                    if (debug) $"Finished {item}".Log();
#endif

                }
#if UNITY_EDITOR
                if (debug) $"Finished all : {sender}".Log();
#endif
            }
            catch (Exception) { }
        }
    }



    public class ActionAsync
    {
        private ActionAsync()
        {
            tasksList = new List<Func<UniTask>>();
        }

        List<Func<UniTask>> tasksList;

        CancellationToken cToken;

        int priority;

        string tag;

        public async UniTask Execute()
        {
            try
            {
                foreach (var taskFunc in tasksList)
                {
                    await taskFunc().AttachExternalCancellation(cToken);
                }
            }
            catch (Exception) { }
        }

        public int GetPriority() => priority;

        public override string ToString() => $"[{tag}] - Prio : [{priority}]";

        public class Builder
        {
            readonly ActionAsync source = new ActionAsync();

            public Builder(CancellationToken cToken)
            {
                source.cToken = cToken;
            }
            /// <summary>
            /// with cancelation token
            /// </summary>
            /// <param name="taskFunc"></param>
            /// <returns></returns>
            public Builder SetTask(Func<CancellationToken, UniTask> taskFunc)
            {
                source.tasksList.Add(() => taskFunc(source.cToken));
                return this;
            }
            public Builder SetTask(Func<UniTask> taskFunc)
            {
                source.tasksList.Add(() => taskFunc());
                return this;
            }
            public Builder Priority(int priority)
            {
                source.priority = priority;
                return this;
            }
            public Builder Tag(string tag)
            {
                source.tag = tag;
                return this;
            }
            public ActionAsync Build()
            {
                return source;
            }
        }
    }


}