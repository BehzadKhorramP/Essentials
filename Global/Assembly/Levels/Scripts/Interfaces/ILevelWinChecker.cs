using Cysharp.Threading.Tasks;

namespace MadApper.Levels
{
    public interface ILevelWinChecker
    {
        public bool HasWon { get; set; }       
        public UniTask Check();
        public void Stop();     
    }
}
