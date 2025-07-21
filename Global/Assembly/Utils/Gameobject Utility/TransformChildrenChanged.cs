using UnityEngine;
using UnityEngine.Events;

namespace MadApper.Essentials
{

    [ExecuteInEditMode]
    public class TransformChildrenChanged : MonoBehaviour
    {
        [SerializeField][AutoGetOrAdd] Transform m_Transform;

        [SerializeField] UnityEventDelayEditor<int> m_OnChildCountChanged;

        int childCount;

        private void Update()
        {
            if (Application.isPlaying)
            {
                enabled = false;
                return;
            }

            if (m_Transform == null)
                return;

            var childCount = m_Transform.childCount;

            if (this.childCount != childCount)
            {               
                m_OnChildCountChanged?.Invoke(childCount);
                this.childCount = childCount;
            }
        }
    }
}
