using System;
using UnityEditor;
using UnityEngine;

public abstract class DataBaseSingleDelete : MonoBehaviour
{
    public abstract Type GetDataBaseType();
}


public abstract class DataBaseSingleDelete<T> : DataBaseSingleDelete where T : DataBase
{
    public override Type GetDataBaseType() { return typeof(T); }
}



#if UNITY_EDITOR

[CustomEditor(typeof(DataBaseSingleDelete), true)]
public class DataBaseSingleDeleteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        DataBaseSingleDelete script = (DataBaseSingleDelete)target;

        var databaseType = script.GetDataBaseType();

        if (databaseType != null)
        {
            EditorGUILayout.Space(10);
            GUILayout.BeginHorizontal("box");
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button($"Delete {databaseType}",
                GUILayout.Height(45),
                GUILayout.Width(EditorGUIUtility.currentViewWidth / 1.1f)))
            {               
                var cla = Activator.CreateInstance(databaseType);

                if (cla is DataBase db)
                {
                    db.TryDelete();
                }                              
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUI.backgroundColor = Color.white;

        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif