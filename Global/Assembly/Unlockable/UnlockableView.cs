using Cysharp.Threading.Tasks;
using MadApper;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BEH
{
    public abstract class UnlockableView : MonoBehaviour
    {
        [SerializeField] protected UnlockableSO m_UnlockabelSO;

        [PropertySpace(10, 10)]
        [FoldoutGroup("WhenEvents")][SerializeField] UnityEventDelayList m_WhenLocked;
        [FoldoutGroup("WhenEvents")][SerializeField] UnityEventDelayList m_WhenUnlocked;

        [PropertySpace(10, 10)]
        [FoldoutGroup("UnlockedAt")][SerializeField] TextMeshProUGUI[] m_UnlockedAtTexts;
        [FoldoutGroup("UnlockedAt")][SerializeField] string m_UnlockedAtPrefix;

        public virtual async UniTask<string> GetSetID() => ISetBased.k_Control;
        public abstract int GetLevel();


        public async void TryRefresh()
        {
            if (m_UnlockabelSO == null)
                return;

            var setID = await GetSetID();
            var level = GetLevel();

            TryRefresh(setID, level);
        }
        public void TryRefresh(string setID, int level)
        {
            if (m_UnlockabelSO == null) return;

            var isUnlocked = m_UnlockabelSO.IsUnlocked(setID, level);

            if (isUnlocked) m_WhenUnlocked?.Invoke();
            else m_WhenLocked?.Invoke();
        }
  
        public async void TrySetUnlockedAtLevelText()
        {
            if (m_UnlockabelSO == null)
                return;

            var setID = await GetSetID();
            var levelToUnlock = m_UnlockabelSO.GetLevelToUnlock(setID);

            TrySetUnlockedAtLevelText(levelToUnlock);
        }
        public void TrySetUnlockedAtLevelText(int levelToUnlock)
        {
            if (levelToUnlock == -1) return;
            var text = $"{m_UnlockedAtPrefix}{levelToUnlock}";
            foreach (var item in m_UnlockedAtTexts) item.SetText(text);
        }

        public async UniTask<bool> GetUnlockableState()
        {
            if (m_UnlockabelSO == null) return false;

            var setID = await GetSetID();
            var level = GetLevel();

            return m_UnlockabelSO.IsUnlocked(setID, level);
        }


        #region Inspector
        public void z_TrySetUnlockedAtLevelText() => TrySetUnlockedAtLevelText();
        public void z_TryRefresh() => TryRefresh();

        #endregion

    }
}
