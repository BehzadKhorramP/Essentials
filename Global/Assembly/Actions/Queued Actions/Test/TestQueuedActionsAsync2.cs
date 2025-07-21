using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace MadApper
{
    public class TestQueuedActionsAsync2 : MonoBehaviour
    {
        public float wait = .1f;
        public int order;
        private void OnEnable()
        {
            TestQueuedActionsAsync.Actions.s_OnAsyncQueuedActions += Append;
        }
        private void OnDisable()
        {
            TestQueuedActionsAsync.Actions.s_OnAsyncQueuedActions -= Append;
        }

        private void Append(TestQueuedActionsAsync.Actions actions)
        {
            this.Log(order);
            var cToken = actions.GetCToken();
            var action = new ActionAsync.Builder(cToken).SetTask(internalT).Priority(order).Build();
            actions.Append(action);

        }
     

        async UniTask internalT(CancellationToken token)
        {
            for (int i = 0; i < 10; i++)
            {
                await UniTask.WaitForSeconds(wait, cancellationToken: token);
                this.Log($"{i} - iterator");
            }
         //   token.ThrowIfCancellationRequested();

            this.LogGreen($"{order} Finished");
        }
    }


}