using BEH.Objective;
using Cysharp.Threading.Tasks;
using MadApper;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BEH
{
    public class ObjectiveSystemPreview : MonoBehaviour
    {
        [SerializeField] Transform m_ContentParent;
        [SerializeField] float m_Wait = 1;
        [AutoGetOrAdd][ReadOnly][SerializeField] AppearerEvents m_AppearerEvents;

        public void z_Delete()
        {
            foreach (Transform item in m_ContentParent)
            {
                Destroy(item.gameObject);
            }
        }

        public async void z_OnShowAPreview(ObjectiveSystemUI objectiveUI)
        {
            z_Delete();

            var layout = Instantiate(objectiveUI.GetLayoutTransform(), m_ContentParent);

            var copyObjectives = layout.GetComponentsInChildren<ObjectiveNeedUI>();
            var orgObjectives = objectiveUI.GetObjectiveNeeds();

            for (int i = 0; i < orgObjectives.Count; i++)
            {
                if (copyObjectives.Length <= i)
                    break;

                var orgObj = orgObjectives[i];
                var cObj = copyObjectives[i];

                // cause some items may need to be intialized again 
                // and Need is [NonSerialized]
                cObj.OnCreated(orgObj.Need);
            }
            
            m_AppearerEvents.z_Appear();

            await UniTask.WaitForSeconds(m_Wait);

            m_AppearerEvents.z_Disappear();
        }

    }
}
