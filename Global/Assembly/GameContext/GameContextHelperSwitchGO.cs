using BEH;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Solatro
{
    public class GameContextHelperSwitchGO : GameContextHelper
    {
        [SerializeField] List<SwitchGO> m_List;

        protected override void ExecuteInternal(GameContext activeContext)
        {            
            foreach (var item in m_List)
            {
                item.GO.gameObject.SetActive(item.Context == activeContext);
            }
        }

        [Serializable]
        public class SwitchGO
        {
            public GameContext Context;
            public GameObject GO;
        }
    }
}
