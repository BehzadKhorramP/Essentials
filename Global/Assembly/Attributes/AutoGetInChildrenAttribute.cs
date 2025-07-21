using System;
using System.Diagnostics;


namespace MadApper
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AutoGetInChildrenAttribute : Attribute
    {
        public bool Mandatory { get; }

        public AutoGetInChildrenAttribute(bool  mandatory = true)
        {
            Mandatory = mandatory;
        }

    }
}
