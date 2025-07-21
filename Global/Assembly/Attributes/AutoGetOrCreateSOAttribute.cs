using System;
using System.Diagnostics;

namespace MadApper
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AutoGetOrCreateSOAttribute : Attribute { }

}
