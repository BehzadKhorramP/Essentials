using Cysharp.Threading.Tasks;

namespace MadApper.Levels
{
    public interface ILevelLoseChecker
    {
        public bool IsKilled { get; set; }
        public UniTask Check();
        public void Stop();
        public void Kill()
        {
            Stop();
            IsKilled = true;
        }
        public void Revive() => IsKilled = false;
    }
}
