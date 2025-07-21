using MadApper.Essentials;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MadApper.BB
{
    public class MaterialSyncer : MonoBehaviour, ISyncerHelper
    {
        public Renderer Reference;
        public Renderer Target;

        public bool SyncOnValidate;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!SyncOnValidate)
                return;

            SyncEditor();
        }
#endif

        public bool Sync()
        {
            if (Reference == null || Target == null)
                return false;

            var setDirty = false;

            for (int i = 0; i < Target.sharedMaterials.Length; i++)
            {
                if (Reference.sharedMaterials.Length > i)
                {
                    var @ref = Reference.sharedMaterials[i];
                    var @tar = Target.sharedMaterials[i];

                    if (@ref != @tar)
                    {
                        setDirty = true;
                        break;
                    }
                }
            }

            Target.sharedMaterials = Reference.sharedMaterials;

            return setDirty;
        }

        [Button]
        bool z_Sync()
        {
            return Sync();
        }
        void SyncEditor()
        {
            if (Reference == null || Target == null)
                return;

            var setDirty = Sync();

#if UNITY_EDITOR
            if (!setDirty)
                return;
            if (Application.isPlaying)
                return;
            EditorUtility.SetDirty(Target);
#endif
        }
    }
}
