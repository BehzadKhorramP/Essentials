#if UNITY_EDITOR
using UnityEditor;


public static class CompilationDisabler
{

    [MenuItem("BEH/Tools/Compilation/Enable Compilation")]
    public static void EnableCompilation()
    {
        // Manually enable script compilation
        EditorApplication.UnlockReloadAssemblies();
        AssetDatabase.Refresh();
    }

    [MenuItem("BEH/Tools/Compilation/Disable Compilation")]
    public static void DisableCompilation()
    {
        // Manually disable script compilation
        EditorApplication.LockReloadAssemblies();
    }
} 
#endif