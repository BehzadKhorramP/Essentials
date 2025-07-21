using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;

namespace MadApper.Essentials
{
    public interface ISyncerHelper
    {
        public bool Sync();
    }
    public class SyncersAllHelper : MonoBehaviour
    {
        [Button]
        public void TrySyncAll()
        {
            var all = transform.GetComponentsInChildren<ISyncerHelper>(true);

            if (all == null || !all.Any()) return;

            var dirty = false;

            foreach (var item in all)
            {
                if (item.Sync())
                    dirty = true;
            }

            if (dirty)
                this.TrySetDirty();
        }
    }
}
