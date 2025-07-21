
using UnityEngine;

namespace MadApper.Levels
{
    public abstract class LevelListener<TLevelSystem, TListener, TLevelSetsCollection, TLevelSet, TLevel, TStage> : MonoBehaviour
       where TListener : LevelsSystem<TLevelSetsCollection, TLevelSet, TLevel, TStage>.Listener
       where TLevelSystem : LevelsSystem<TLevelSetsCollection, TLevelSet, TLevel, TStage>
       where TLevelSetsCollection : LevelSetsCollection<TLevelSet, TLevel, TStage>
       where TLevelSet : LevelSet<TLevel, TStage>
       where TLevel : Level<TStage>
       where TStage : Stage
    {
        [SerializeField] protected TListener listener;

        protected virtual void OnEnable()
        {
            listener.OnEnable();
        }

        protected virtual void OnDisable()
        {
            listener.OnDisable();
        }
    }
}
