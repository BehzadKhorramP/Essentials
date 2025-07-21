using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MadApper
{
    public class SceneBasedEventsHelper : MonoBehaviour
    {
        public List<SceneBasedEvent> m_Events;

        public void Execute()
        {
            var eve = GetEvent();

            if (eve == null)
                return;

            foreach (var item in eve.Events)
                item?.Invoke();
        }


        public void z_Execute()
        {
            Execute();
        }


        SceneBasedEvent GetEvent()
        {
            if (m_Events == null)
                return null;

            var sceneName = SceneManager.GetActiveScene().name;
            return m_Events.Find(x => x.SceneSO.m_SceneName.Equals(sceneName));
        }

    }

    [Serializable]
    public class SceneBasedEvent
    {
        [Space(10)] public SceneSO SceneSO;
        [Space(10)] public List<UnityEventDelay> Events;
    }
}
