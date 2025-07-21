using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MadApper.Essentials
{
    public class FontAssetSyncer : MonoBehaviour, ISyncerHelper
    {
        public TextMeshPro Reference;
        public TextMeshPro Target;

        public bool Sync()
        {
            if (Reference == null || Target == null)
                return false;

            var setDirty = false;

            var @ref = Reference.font;
            var @tar = Target.font;

            if (@ref != @tar)
            {
                setDirty = true;               
            }

            var @refMat = Reference.fontSharedMaterial;
            var @tarMat = Target.fontSharedMaterial;
            if (@refMat != @tarMat)
            {
                setDirty = true;
            }

            Target.font = Reference.font;
            Target.fontSharedMaterial = Reference.fontSharedMaterial;

            if (setDirty)
            {
                Target.TrySetDirty();
            }

            return setDirty;
        }


        [Button]
        bool z_Sync()
        {
            return Sync();
        }
    }
}
