

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace MadApper.Essentials
{
    public static class UniTaskUtils
    {

#if UNITY_EDITOR
        public static UniTask WaitForSecondsEditor(float seconds, CancellationToken cancellationToken = default)
        {
            var tcs = new UniTaskCompletionSource();
            float startTime = (float)EditorApplication.timeSinceStartup;

            void Update()
            {
                // If canceled, stop waiting immediately
                if (cancellationToken.IsCancellationRequested)
                {
                    EditorApplication.update -= Update;
                    tcs.TrySetCanceled();
                    return;
                }

                // If time has passed, complete the task
                if ((float)EditorApplication.timeSinceStartup - startTime >= seconds)
                {
                    EditorApplication.update -= Update;
                    tcs.TrySetResult();
                }
            }

            EditorApplication.update += Update;
            return tcs.Task;
        }
#endif

    }
}
