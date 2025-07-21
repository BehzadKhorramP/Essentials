using Cysharp.Threading.Tasks;
using MadApper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BEH
{

    [CreateAssetMenu(fileName = "UnlockableSO", menuName = "Unlockables/UnlockableSO")]

    public class UnlockableSO : ScriptableObject, ILookupObject
    {
        [SerializeField] List<Unlocking> m_Unlockings = new List<Unlocking>();
        [SerializeField] int levelToUnlockGeneral = -1;

        #region ILookupObject

        [SerializeField] string lookupIndicator;
        public string i_LookupIndicator { get => lookupIndicator; set => lookupIndicator = value; }

        #endregion


        public int GetLevelToUnlock(string setID)
        {
            var d = m_Unlockings.Find(x => x.SetID.Equals(setID, StringComparison.OrdinalIgnoreCase));

            if (d == null)
            {
                $"no Unlocking data for setID of {setID}".Log();
                return levelToUnlockGeneral;
            }

            return d.LevelToUnlock;
        }
     
        public bool IsUnlocked(string setID, int level)
        {
            var levelToUnlock = GetLevelToUnlock(setID);
            if (levelToUnlock == -1) return false;
            return level >= levelToUnlock;
        }

        public void SetLevelToUnlock(string setID, int level)
        {
            var d = m_Unlockings.Find(x => x.SetID.Equals(setID, StringComparison.OrdinalIgnoreCase));

            if (d == null)
            {
                d = new Unlocking() { SetID = setID };
                m_Unlockings.Add(d);
            }

            d.LevelToUnlock = level;

#if UNITY_EDITOR            
            EditorUtility.SetDirty(this);
#endif
        }

    }

    [Serializable]
    public class Unlocking
    {
        public string SetID;
        public int LevelToUnlock;
    }
}
