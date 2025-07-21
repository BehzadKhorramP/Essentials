using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace MadApper
{
    public class TestQueuedActionsAsync : MonoBehaviour
    {

        Actions actions;

        [Button]
        public async void Test()
        {
            if (actions != null) actions.Stop();

            actions = new Actions(this);
            actions.CollectActions();

            await actions.Execute();

            this.Log("All Finished");
        }

        [Button]
        public void Stop()
        {
            this.LogRed("Canceled");

            if (actions != null) actions.Stop();
            actions = null;
        }


        public class Actions : QueuedActionsAsync<TestQueuedActionsAsync, Actions>
        {
            public Actions(TestQueuedActionsAsync sender) : base(sender) { }

            public override Actions GetSelf() => this;
        }
    }


}