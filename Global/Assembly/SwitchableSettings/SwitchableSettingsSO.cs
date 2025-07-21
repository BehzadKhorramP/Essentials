using MadApper;
using UnityEngine;

namespace BEH
{

    public interface ISwitchableSettings<TSettings> where TSettings : class
    {
        public void Switch(TSettings newValue);
    }

    public abstract class SwitchableSettingsSO<TSettings> : ScriptableObject where TSettings : class
    {
        public TSettings Value;

        public virtual void Switch(TSettings newValue)
        {
            if (newValue == null)
                return;

            Value = newValue;

            this.TrySetDirty();
        }
    }

}
