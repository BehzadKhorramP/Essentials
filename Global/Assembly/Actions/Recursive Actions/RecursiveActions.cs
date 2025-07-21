using System;
using System.Collections.Generic;

namespace MadApper
{
    public class RecursiveActions
    {
        private int Depth;

        private List<Action<Action>> Actions;

        private Action OnComplete;

        public RecursiveActions(Action onComplete = null)
        {
            Depth = 0;
            Actions = new List<Action<Action>>();
            OnComplete = onComplete;
        }

        public void Add(Action<Action> action) => Actions.Add(action);
        public void AddCallback(Action action) => Actions.Add((x) => { action?.Invoke(); x?.Invoke(); });

        public int GetActionsCount() => Actions.Count;

        public void Execute()
        {
            if (Depth >= Actions.Count)
            {
                OnComplete?.Invoke();
                return;
            }

            var action = Actions[Depth];

            action.Invoke(() =>
            {
                Depth++;

                Execute();
            });
        }
    }

    public class RecursiveActions<T>
    {
        private int Depth;

        private List<Action<Action<T>>> Actions;

        private Action OnComplete;

        public RecursiveActions(Action onComplete = null)
        {
            Depth = 0;
            Actions = new List<Action<Action<T>>>();
            OnComplete = onComplete;
        }

        public void Add(Action<Action<T>> action) => Actions.Add(action);

        public int GetActionsCount() => Actions.Count;

        public void Execute()
        {
            if (Depth >= Actions.Count)
            {
                OnComplete?.Invoke();
                return;
            }

            var action = Actions[Depth];

            action.Invoke((obj) =>
            {
                Depth++;
                Execute();
            });
        }
    }
}
