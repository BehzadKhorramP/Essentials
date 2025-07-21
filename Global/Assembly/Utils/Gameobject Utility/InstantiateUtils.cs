using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MadApper.Essentials
{
    public class InstantiateUtils : MonoBehaviour
    {
        public GameObject objToInstantiate;

        List<GameObject> instantiateds = new List<GameObject>(1);

        CancellationTokenSource cts;

        private void Start()
        {
            new SceneChangeSubscriber.Builder()
             .SetOnSceneToBeChangedReset(Reset)
             .AddGameObjectScene(gameObject)
             .Build();
        }

        void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
            cts = null;
        }

        private void Reset(string obj)
        {
            Stop();

            var count = instantiateds.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var item = instantiateds[i];
                Destroy(item);
            }
            instantiateds.Clear();
        }

        public void z_InstantiateAndDestroy(float destroyDelay)
        {
            if (cts == null) cts = new CancellationTokenSource();
            if (objToInstantiate == null) return;

            var newGo = Instantiate(objToInstantiate, transform);
            newGo.transform.SetParent(null);
            newGo.SetActive(true);

            DestroyAsync(newGo, destroyDelay);
        }

        async UniTask DestroyAsync(GameObject go, float timer)
        {          
            instantiateds.Add(go);

            try
            {
                if (timer > 0) await UniTask.WaitForSeconds(timer, cancellationToken: cts.Token);
                instantiateds.Remove(go);
                Destroy(go);
            }
            catch (System.Exception) { }
        }

    }
}
