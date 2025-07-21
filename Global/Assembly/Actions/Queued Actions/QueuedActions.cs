using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace MadApper
{

    public class QueuedActions
    {
        public List<QueuedAction> Actions;

        public int Done;

        private Action OnCompleted;

        public QueuedActions(Action onCompleted = null)
        {
            Done = 0;
            OnCompleted = onCompleted;
            Actions = new List<QueuedAction>();
        }

        public void Stop()
        {

        }

        protected virtual void OnCompelted()
        {
#if UNITY_EDITOR
            $"all QueuedActions done!".LogBlue();
#endif

            OnCompleted?.Invoke();
        }

        public void Add(QueuedAction action)
        {
            Actions.Add(action);

#if UNITY_EDITOR
            Message(action, $"[{action.ID}] added : {Actions.Count}");
#endif
        }

        public void Add(Action<Action> action, string id = null, int? order = null, Object @object = null)
        {
            Add(new QueuedAction(action: action, id: id, order: order, obj: @object));
        }

        public virtual void Execute()
        {
            if (Actions == null || Actions.Count == 0)
            {
#if UNITY_EDITOR
                $"no queued actions!".LogBlue();
#endif
                OnActionDone(null);
                return;
            }


            foreach (var action in Actions)
            {
                if (action.Order == null)
                {
#if UNITY_EDITOR
                    Message(action, $"[{action.ID}] started! (not queued)");
#endif

                    action.Action?.Invoke(() =>
                    {

#if UNITY_EDITOR
                        Message(action, $"[{action.ID}] done!");
#endif

                        OnActionDone(action);
                    });
                }
            }

            var ordered = Actions.Where(x => x.Order.HasValue).OrderBy(x => x.Order);

            var recursiveActions = new RecursiveActions<QueuedAction>();

            foreach (var action in ordered)
            {
                Action<Action<QueuedAction>> value = (x) =>
                {
#if UNITY_EDITOR
                    Message(action, $"[{action.ID}] started!");
#endif

                    action.Action?.Invoke(
                           () =>
                           {
#if UNITY_EDITOR
                               Message(action, $"[{action.ID}] done!");
#endif

                               x.Invoke(action);
                               OnActionDone(action);
                           });
                };

                recursiveActions.Add(value);
            }

            recursiveActions.Execute();
        }

        public virtual void OnActionDone(QueuedAction action)
        {
            Done++;

            if (Done >= Actions.Count)
            {
                OnCompelted();
            }
        }


        public static void Message(QueuedAction action, string message)
        {
            if (action.Object != null) action.Object.LogBlue(message);
            else message.LogBlue();
        }
    }



    [Serializable]
    public class QueuedActions<T> : QueuedActions
    {
        public T Sender;

        public static Action<QueuedActions<T>> s_CollectActions;

        public QueuedActions(Action onCompleted, T sender) : base(onCompleted)
        {
            Sender = sender;
        }

        public void Collect() => s_CollectActions?.Invoke(this);

    }


    public class QueuedAction
    {
        public Action<Action> Action;
        public string ID;
        public int? Order;
        public Object Object;

        public QueuedAction(Action<Action> action,
            string id = null,
            int? order = null,
            Object obj = null)
        {
            ID = id;
            Action = action;
            Order = order;
            Object = obj;
        }
    }



}