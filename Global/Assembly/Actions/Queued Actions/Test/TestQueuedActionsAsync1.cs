using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace MadApper
{
    public class TestQueuedActionsAsync1 : MonoBehaviour
    {
        public int order;
        public float wait = 2;

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
            var action = new ActionAsync.Builder(cToken).SetTask(Test1).Priority(order).Build();
            actions.Append(action);

        }

        async UniTask Test1(CancellationToken ctoken)
        {
            await UniTask.WaitForSeconds(wait, cancellationToken: ctoken);

            this.LogGreen($"{order} Finished");
        }
    }


}