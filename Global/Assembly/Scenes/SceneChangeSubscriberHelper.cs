using UnityEngine;


namespace MadApper
{
    public class SceneChangeSubscriberHelper : MonoBehaviour
    {
        [SerializeField] UnityEventDelayList m_OnSceneChanged;
        [SerializeField] UnityEventDelayList m_OnSceneActivated;
        [SerializeField] UnityEventDelayList m_OnSceneUnloaded;
        [SerializeField] UnityEventDelayList m_OnSceneReset;

        SceneChangeSubscriber subscriber;

        public void Start()
        {
            subscriber = new SceneChangeSubscriber.Builder()
                .SetOnSceneChanged((s) => m_OnSceneChanged?.Invoke())
                .SetOnSceneActivatedInitialize((s) => m_OnSceneActivated?.Invoke())
                .SetOnSceneToBeChangedReset((s) => m_OnSceneReset?.Invoke())
                .SetOnSceneUnloaded((s) => m_OnSceneUnloaded?.Invoke())
                .AddGameObjectScene(gameObject)
                .Build();
        }

        public void z_Unsubscribe()
        {
            if (subscriber == null)
                return;

            subscriber.Unsubscribe();
            subscriber = null;
        }
    }

}
