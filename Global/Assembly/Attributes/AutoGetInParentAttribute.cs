using System;
using System.Diagnostics;


namespace MadApper
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AutoGetInParentAttribute : Attribute
    {
        public bool Mandatory { get; }

        public AutoGetInParentAttribute(bool mandatory = true)
        {
            Mandatory = mandatory;
        }
    }
}
