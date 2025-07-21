using BEH;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadApper.Solatro
{
    public class GameContextHelperEvents : GameContextHelper
    {
        [SerializeField] List<ContextEvents> m_List;

        protected override void ExecuteInternal(GameContext activeContext)
        {
            var res = m_List.Find(x => x.Context == activeContext);
            if (res == null) return;
            res.Events?.Invoke();
        }

        [Serializable]
        public class ContextEvents
        {
            public GameContext Context;
            public UnityEventDelayList Events;
        }
    }
}
