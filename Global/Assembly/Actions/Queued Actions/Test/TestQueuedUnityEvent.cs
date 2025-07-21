using UnityEngine;
using Sirenix.OdinInspector;

namespace MadApper
{
    public class TestQueuedUnityEvent : MonoBehaviour
    {
        
        public QueuedUnityEvent action1;
        public QueuedUnityEvent action2;
        public QueuedUnityEvent action3;

        [Button]
        public void Test()
        {
            QueuedUnityEventExtentions.Invoke(action1, action2, action3);
        }
    }
}