using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MadApper
{

    public interface ILookupObject
    {
        public string i_LookupIndicator { get; set; }
    }

    public class LookUpSO : SingletonScriptable<LookUpSO>
    {

#if UNITY_EDITOR

        [MenuItem("MAD/Lookup/Asset", false, 100)]
        static void Edit()
        {
            Selection.activeObject = GetSO();
        }

#endif


        public List<Data> m_Data;


        [Serializable]
        public class Data : ISerializationCallbackReceiver
        {
            public Object Object;
            public ILookupObject ILookupObject;
            public string Indicator;

            public void OnAfterDeserialize()
            {
                if (Object != null && ILookupObject == null)
                {
                    ILookupObject = Object as ILookupObject;
                }
            }

            public void OnBeforeSerialize() => this.OnValidate();

            void OnValidate()
            {
#if UNITY_EDITOR
                if (TryConvert()) return;

                Debug.LogWarning($"[{Object}] Object is not an ILookupObject!");

                Object = null;
#endif
            }


            public bool TryConvert()
            {
                if (Object is ILookupObject ilookup)
                {
                    ILookupObject = ilookup;
                    ILookupObject.i_LookupIndicator = Indicator;
                    Object.TrySetDirty();
                    return true;
                }

                return false;
            }


            public ILookupObject GetILookupObject()
            {
                if (ILookupObject != null)
                    return ILookupObject;

                if (Object is ILookupObject ilookup)
                    ILookupObject = ilookup;

                return ILookupObject;
            }
        }


        public void TryAdd<T>(T obj, string id) where T : Object
        {
            if (obj is not ILookupObject lookupObj) return;
            if (string.IsNullOrEmpty(id)) return;
            var data = GetData<T>(id);
            if (data != null)
            {
                data.Object = obj;
                data.TryConvert();
            }
            else
            {
                data = new Data() { Object = obj, Indicator = id };
                data.TryConvert();
                m_Data.Add(data);
                this.TrySetDirty();
            }

        }

        public T GetValue<T>(string id) where T : Object
        {
            var o = m_Data.Where(x => x.GetILookupObject().i_LookupIndicator.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (o == null) return default;

            foreach (var item in o)
                if (item.Object != null && item.Object is T t)
                    return t;

            return null;
        }

        public Data GetData<T>(ILookupObject iLookupObject) where T : Object
        {
            if (string.IsNullOrEmpty(iLookupObject.i_LookupIndicator))
            {
                Debug.LogWarning($"[{iLookupObject}]'s LookupIndicator is NullOrEmpty!");
                return default;
            }

            var o = m_Data.Where(x => x.GetILookupObject().i_LookupIndicator.Equals(iLookupObject.i_LookupIndicator, StringComparison.OrdinalIgnoreCase));

            if (o == null) return null;

            foreach (var item in o)
                if (item.Object != null && item.Object is T t)
                    return item;

            return default;
        }
        public Data GetData<T>(string id) where T : Object
        {
            var o = m_Data.Where(x => x.GetILookupObject().i_LookupIndicator.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (o == null) return null;

            foreach (var item in o)
                if (item.Object != null && item.Object is T t)
                    return item;

            return default;
        }
    }
}
