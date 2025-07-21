using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MadApper;


#if UNITY_EDITOR

using UnityEditor;


[CustomEditor(typeof(DataBasesDelete))]
public class DataBasesDeleteEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        DataBasesDelete script = (DataBasesDelete)target;


        EditorGUILayout.Space(10);
        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Delete All Data",
            GUILayout.Height(45),
            GUILayout.Width(EditorGUIUtility.currentViewWidth / 1.1f)))
        {
            foreach (var db in DataBasesDelete.allDBs)
            {
                db.TryDelete();
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Delete Player Prefs",
            GUILayout.Height(45),
            GUILayout.Width(EditorGUIUtility.currentViewWidth / 1.1f)))
        {
            PlayerPrefs.DeleteAll();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.backgroundColor = Color.white;


        EditorGUILayout.Space(10);
        GUILayout.BeginHorizontal("box");
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUI.backgroundColor = Color.yellow;

        for (int i = 0; i < DataBasesDelete.allDBs.Length; i++)
        {
            var db = DataBasesDelete.allDBs[i];
            if (GUILayout.Button($"{db}", GUILayout.Height(30),
                    GUILayout.Width(EditorGUIUtility.currentViewWidth / 1.5f)))
            {
                db.TryDelete();
            }
        }


        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}


#endif

public class DataBasesDelete : MonoBehaviour
{
    static DataBase[] _allDBs;
    public static DataBase[] allDBs
    {
        get
        {
            if (_allDBs == null || _allDBs.Length == 0)
            {
                _allDBs = MADUtility.GetAllDerivedInstancesInAllAssemblies<DataBase>().ToArray();
            }

            return _allDBs;
        }
    }

    public static void DeleteAll()
    {
        foreach (var db in allDBs)
        {
            db.TryDelete();
        }
    }
}


