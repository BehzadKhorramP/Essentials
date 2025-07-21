using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace MadApper.Addressable
{
    public struct AddressableToLoadArg<T>
    {
        public AssetReference AssetReference;
        public Action<T> OnLoaded;
    }


    public static class AddressablesLoader
    {
        #region Reset
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

                Reset();
            }
        }
#endif

        static void Reset()
        {
            Release();
        }
        #endregion

        static HashSet<AssetReference> assetReferences;
        static HashSet<AssetReference> s_AssetReferences
        {
            get
            {
                if (assetReferences == null)
                    assetReferences = new HashSet<AssetReference>();
                return assetReferences;
            }
        }

        public static async UniTask<T[]> Load<T>(this List<AddressableToLoadArg<T>> args, Action<float> onTotalProgress = null)
        {
            List<UniTask<T>> tasks = new List<UniTask<T>>();
            List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();

            foreach (var arg in args)
            {
                AsyncOperationHandle<T> handle = GetHandle(arg);
                handles.Add(handle);
                tasks.Add(LoadAsync(arg, handle));
            }

            TrackTotalProgress(handles, onTotalProgress);

            var results = await UniTask.WhenAll(tasks);

            return results;
        }
        public static async UniTask<T> Load<T>(this AddressableToLoadArg<T> arg, Action<float> onTotalProgress = null)
        {
            List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();

            AsyncOperationHandle<T> handle = GetHandle(arg);
            handles.Add(handle);
            var task = LoadAsync(arg, handle);

            TrackTotalProgress(handles, onTotalProgress);

            var result = await task;

            return result;
        }

        static async UniTask<T> LoadAsync<T>(AddressableToLoadArg<T> arg, AsyncOperationHandle<T> handle)
        {
            T obj = await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                arg.OnLoaded?.Invoke(obj);
                return obj;
            }
            else
                Debug.LogWarning($"Failed to load asset: [{arg.AssetReference}]");

            return default;
        }

        static AsyncOperationHandle<T> GetHandle<T>(AddressableToLoadArg<T> arg)
        {
            if (arg.AssetReference.IsValid())
                arg.AssetReference.ReleaseAsset();

            AsyncOperationHandle<T> handle = arg.AssetReference.LoadAssetAsync<T>();

            s_AssetReferences.Add(arg.AssetReference);

            return handle;
        }
        static async void TrackTotalProgress(this List<AsyncOperationHandle> handles, Action<float> onTotalProgress)
        {
            while (!AllAssetsLoaded())
            {
                float totalProgress = 0f;

                foreach (var handle in handles)
                    if (handle.IsValid())
                        totalProgress += handle.PercentComplete;

                totalProgress /= handles.Count;

                //    Debug.Log($"Total Loading Progress: {totalProgress * 100}%");

                onTotalProgress?.Invoke(totalProgress);

                await UniTask.Yield();
            }

            onTotalProgress?.Invoke(1);

            Debug.Log("All assets have finished loading.");

            await UniTask.Yield();

            bool AllAssetsLoaded()
            {
                foreach (var handle in handles)
                    if (handle.IsValid() && handle.Status != AsyncOperationStatus.Succeeded)
                        return false;

                return true;
            }
        }




        static void Release()
        {
            if (assetReferences != null)
                foreach (var assetRefenced in assetReferences)
                    if (assetRefenced.IsValid())
                        assetRefenced.ReleaseAsset();

            assetReferences = null;
        }
    }

}