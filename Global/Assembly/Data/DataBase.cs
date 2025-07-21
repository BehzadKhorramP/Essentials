using LocalSave;
using MadApper;
using UnityEditor;
using UnityEngine;


public abstract class DataBase
{
    private static JsonSaver _jsonSave;
    public static JsonSaver JsonSaver
    {
        get
        {
            if (_jsonSave == null)
                _jsonSave = new JsonSaver();

            return _jsonSave;
        }
    }

    public abstract void TryDelete();
    public abstract void TryUnload();

}


public abstract class DataBase<T> : DataBase where T : class, ISaveable, new()
{
    private static T _data;
    public static T Data
    {
        get
        {
            if (_data == null)
                LoadData();

            return _data;
        }
    }

    public static void LoadData()
    {
        //if (!Application.isPlaying)
        //    return;

        _data = new T();

        if (JsonSaver.Load(_data.SaveName, out T data))
        {
            _data = data;
        }
    }

    public static void Save()
    {
        JsonSaver.Save(Data);
    }
    public static void Unload()
    {
        if (_data == null)
            return;

        _data = null;      
    }

    public static void Delete()
    {
        JsonSaver.Delete(Data.SaveName);
        OnReset();
    }

    public static void OnReset()
    {
        _data = null; 
    }



    public override void TryUnload()
    {
        Unload();
    }
    public override void TryDelete()
    {
        Delete();
    }

}





#if UNITY_EDITOR

public class DataBaseReseter
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnRuntimeMethodLoad()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Reset();
        }
    }

    public static void Reset()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

        TryReset();
    }

    static void TryReset()
    {
        var all = MADUtility.GetAllDerivedInstancesInAllAssemblies<DataBase>();

        if (all == null)
            return;

        foreach (var item in all)
            item.TryUnload();
    }
}
#endif
