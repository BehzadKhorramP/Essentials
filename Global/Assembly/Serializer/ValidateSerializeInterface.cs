using MadApper;
using UnityEditor;
using UnityEngine;

namespace BEH
{
    public abstract class ValidateSerializeInterface<TInterface, TObj> : ISerializationCallbackReceiver
        where TInterface : class
        where TObj : Object
    {
        public TInterface Inteface;
        public TObj Obj;

        public ValidateSerializeInterface()
        {

        }
        public ValidateSerializeInterface(TInterface inteface, TObj obj)
        {
            Inteface = inteface;
            Obj = obj;
            
            OnValidate();
        }

        public void OnAfterDeserialize()
        {
            if (Obj != null && Inteface == null)
                Inteface = Obj as TInterface;
        }

        public void OnBeforeSerialize() => this.OnValidate();

        void OnValidate()
        {
#if UNITY_EDITOR
            if (TryConvert())
                return;

            if (Obj == null)
                return;

            Debug.LogWarning($"[{Obj}] Object is not Typeof [{typeof(TInterface)}]!");

            Obj = null;
            Inteface = null;
#endif
        }


#if UNITY_EDITOR
        public bool TryConvert()
        {
            if (Obj is TInterface iInterface)
            {
                Inteface = iInterface;
                return true;
            }
            return false;
        }
#endif
    }
}
