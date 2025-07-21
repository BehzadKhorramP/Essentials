using BEH.Common;
using Cysharp.Threading.Tasks;

namespace MadApper.Levels
{
    public class StageStabilizeWaiter : Waiter
    {
     
        public async UniTask WaitForOngoingTasks(QBool tasks)
        {
            var token = cts.Token;

            while (tasks == false && !token.IsCancellationRequested)
                await UniTask.Delay(10, cancellationToken: token);
        }

        public class Builder : Builder<StageStabilizeWaiter, Builder>
        {           
            public Builder WithOnGoingTasks(QBool tasks)
            {
                source.tasksList.Add(() => source.WaitForOngoingTasks(tasks));
                return this;
            }
        }
    }





}
