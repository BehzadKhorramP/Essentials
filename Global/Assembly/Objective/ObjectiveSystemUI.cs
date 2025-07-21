using MadApper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BEH.Objective
{
    public class ObjectiveSystemUI : MonoBehaviour
    {
        [Space(10)][SerializeField] Transform m_Layout;

        [Space(10)][SerializeField] ObjectiveNeedUI m_Prefab;

        [Space(10)][SerializeField] UnityEventDelayList m_OnInit;

        List<ObjectiveNeedUI> objectiveNeeds;

        public Transform GetLayoutTransform() => m_Layout;
        public List<ObjectiveNeedUI> GetObjectiveNeeds() => objectiveNeeds;

        public void Init(ObjectiveSystem objectiveSystem)
        {
            DestroyObjectives();

            objectiveNeeds = new List<ObjectiveNeedUI>();

            foreach (var need in objectiveSystem.m_NeedsCollection.m_Needs)
            {
                var ui = Instantiate(m_Prefab, m_Layout.transform);
                ui.OnCreated(need);
                objectiveNeeds.Add(ui);
            }

            m_OnInit?.Invoke();
        }
        public void OnReset()
        {
            DestroyObjectives();
        }

        void DestroyObjectives()
        {
            var objectives = m_Layout.GetComponentsInChildren<ObjectiveNeedUI>();

            if (objectives == null)
                return;

            for (int i = objectives.Length - 1; i >= 0; i--)
            {
                var it = objectives[i];
                it.GetDestroyed();
            }
        }

        public ObjectiveNeedUI GetObjectiveUI(string id)
        {
            if (objectiveNeeds == null || objectiveNeeds.Count == 0)
                return null;

            var objective = objectiveNeeds.Find(x => x.Need.IObjectivable.ID.Equals(id));

            return objective;
        }



        public void OnUpdateUI(string id, int remaining)
        {
            var objective = GetObjectiveUI(id);

            if (objective == null)
                return;

            objective.OnUpdate(remaining);
        }

    }
}
