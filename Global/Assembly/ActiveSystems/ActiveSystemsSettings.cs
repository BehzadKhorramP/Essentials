using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MadApper
{
    public interface IActiveableSystem
    {
        public string i_PrefabName { get; }

        public static bool IsActive(string prefabName) => ActiveSystemsSettings.Instance.IsActive(prefabName);
    }

    public class ActiveSystemsSettings : SingletonScriptable<ActiveSystemsSettings>
    {

        [Serializable]
        public class System
        {
            public bool IsActive;
            public string SystemID;
        }

        public List<System> Systems;


#if UNITY_EDITOR

        [MenuItem("MAD/Active Systems/Settings", false, 100)]
        static void Edit()
        {
            Selection.activeObject = GetSO();
        }

        public static void TryAdd(string systemID)
        {
            var so = GetSO();

            if (so.Systems == null)
                so.Systems = new List<System>();

            var sys = so.Systems.Find(x => x.SystemID == systemID);

            if (sys == null)
            {
                so.Systems.Add(new System() { SystemID = systemID });
                EditorUtility.SetDirty(so);
            }

        }

#endif

        public bool IsActive(string systemID)
        {
            if (Systems == null)
                return false;

            var sys = Systems.Find(x => x.SystemID == systemID);

            if (sys == null)
                return false;

            return sys.IsActive;
        }
    }
}
