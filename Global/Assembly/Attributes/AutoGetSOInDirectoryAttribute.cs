using System;
using System.Diagnostics;

namespace MadApper
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AutoGetSOInDirectoryAttribute : Attribute
    {
        public string PathOverride { get; }
        public bool UseAssetDirectory { get; } 

        public AutoGetSOInDirectoryAttribute(string pathOverride = null, bool useAssetDirectory = false)
        {
            PathOverride = pathOverride;
            UseAssetDirectory = useAssetDirectory;
        }
    }

}
