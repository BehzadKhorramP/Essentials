#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;



#endif
namespace MadApper
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif  
    public static class DataExtra
    {

#if UNITY_EDITOR

        const string k_GeneratedClassName = "GeneratedDataExtra";
        const string k_BoolName = "s_IgnoreHash";


        static DataExtra()
        {
#if DATAEXTRA_ENABLED
            return;
#endif
            GenerateClass();
        }


        [MenuItem("BEH/Data/Extra/GenerateClass", false, 100)]
        static void Generate()
        {
            GenerateClass();
        }

        public static void GenerateClass()
        {
            StringBuilder result = new StringBuilder();

            AddClassHeader(result);

            result.Append($"public static bool {k_BoolName} = false;");

            AddMethod(result);

            AddClassFooter(result);

            var path = MADUtility.GetEssentialsFolder().Replace("Assets", "");
            string scriptPath = Application.dataPath + path + $"/{k_GeneratedClassName}.cs";

            File.WriteAllText(scriptPath, result.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }



        static void AddClassHeader(StringBuilder result)
        {
            result.Append(@"
                          using UnityEngine;
                          namespace MadApper 
                          {
                           public static class ");

            result.Append($"{k_GeneratedClassName}");
            result.Append(@"{
                           ");
        }

        static void AddMethod(StringBuilder result)
        {
            result.Append(@"
                     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
                     public static void SetDataExtra()
                     {
                       ");

            result.Append($"LocalSave.JsonSaver.s_IgnoreHash = {k_BoolName};");
            result.Append(@"
                          }");

        }

        static void AddClassFooter(StringBuilder result)
        {
            result.Append(@"
                  }
              }");
        }

#endif


    }
}
