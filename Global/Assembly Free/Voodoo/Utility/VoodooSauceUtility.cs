#if VOODOOSAUCE_ENABLED
using Cysharp.Threading.Tasks;
using System;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

using Voodoo.Sauce.Core;
using Voodoo.Sauce.Debugger;
using Voodoo.Sauce.Internal.Ads;

namespace BEH.Voodoo
{
    public static class VoodooSauceUtility
    {
        static bool s_IsInitialized;

        public static Action<OrderedRecursiveActions, bool> onPreInterstitialOrderedAction;
        public static Action<OrderedRecursiveActions, bool> onPostInterstitialOrderedAction;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Load()
        {
            s_IsInitialized = false;

            VoodooSauce.SubscribeOnInitFinishedEvent(OnInitialized);

            IAPRestore_Voodoo.onTryRestorePurchases += TryRestorePurchase;

#if UNITY_EDITOR

            s_IsInitialized = true;

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        private static void OnInitialized(VoodooSauceInitCallbackResult obj)
        {
            VoodooSauce.UnSubscribeOnInitFinishedEvent(OnInitialized);

            s_IsInitialized = true;
        }


#if UNITY_EDITOR
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Reset();
            }
            else if (state == PlayModeStateChange.EnteredPlayMode)
            {
                Init();
            }

        }

#endif

        public static void Reset()
        {
            s_IsInitialized = false;

            IAPRestore_Voodoo.onTryRestorePurchases -= TryRestorePurchase;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            ResetVoodooSauce();
#endif

        }

#if UNITY_EDITOR

        private static void Init()
        {
            // Get the static constructor using TypeInitializer
            ConstructorInfo staticConstructor = typeof(Debugger).TypeInitializer;

            // Invoke the static constructor
            if (staticConstructor != null)
            {
                staticConstructor.Invoke(null, null);
            }
        }

        static void ResetVoodooSauce()
        {
            var privateStaticflags = BindingFlags.Static | BindingFlags.NonPublic;

            typeof(VoodooSauceBehaviour)
                .SetValueViaReflection(classobject: null, valuename: "_startCalled", newvalue: false, privateStaticflags);
            typeof(VoodooSauceBehaviour)
                .SetValueViaReflection(classobject: null, valuename: "_initStarted", newvalue: false, privateStaticflags);
            typeof(VoodooSauceBehaviour)
                .SetValueViaReflection(classobject: null, valuename: "_initFinished", newvalue: false, privateStaticflags);
            typeof(AdsManager)
                .SetValuesViaReflection(classobject: null, newvalue: null, privateStaticflags);
            typeof(Debugger)
                .SetValuesViaReflection(classobject: null, newvalue: null, privateStaticflags);
            typeof(VoodooSauceBehaviour)
                .SetValuesViaReflection(classobject: null, newvalue: null, privateStaticflags);

        }
#endif




        #region Restore

        private static void TryRestorePurchase(Action<bool, string> action)
        {
            VoodooSauce.RestorePurchases((x) =>
            {
                Result(result: true, message: x.ToString());
            });

            void Result(bool result, string message)
            {
                action?.Invoke(result, message);
            }
        }

        #endregion

        #region VoodooTune



        public static async void TryInitVSAndGetRemoteData<T>(Action<T> onComplete, MonoBehaviour mono = null) where T : class, new()
        {
            if (s_IsInitialized)
            {
                Debug.Log("VoodooSauceUtility has been initialized!");
                var res = GetConfig<T>(mono);
                onComplete?.Invoke(res);
                return;
            }

            await UniTask.WaitUntil(() => s_IsInitialized == true);

            var ress = GetConfig<T>(mono);

            onComplete?.Invoke(ress);
        }


        private static T GetConfig<T>(MonoBehaviour mono) where T : class, new()
        {
            var res = new T();

            res = VoodooSauce.GetItemOrDefault<T>();

            if (res == null)
            {
                Debug.LogWarning($"Couldnt fetch remote config for {typeof(T)}!");
                return null;
            }

#if UNITY_EDITOR
            //  res = new T();
            mono?.LogBlue($" {typeof(T)} :: {res}");
#endif

            return res;
        }

        #endregion


        public static void ShowInterstitial(Action<bool> onComplete = null, bool ignoreConditions = false, string interstitialType = null)
        {
            $"Trying To Show '{interstitialType}' Interstitial...".LogBlue();

            #region Post Ads

            Action<bool> onWatched = (adsShown) =>
            {
                $"'{interstitialType}' Interstitial Watched: {adsShown}".LogBlue();

                Action onAllFinished = () =>
                {
                    onComplete?.Invoke(adsShown);
                };

                var postAdsActions = new OrderedRecursiveActions(onAllFinished);

                onPostInterstitialOrderedAction?.Invoke(postAdsActions, adsShown);

                postAdsActions.Execute();
            };

            #endregion

            #region Pre Ads

            Action onShowInterstitial = () =>
            {
                VoodooSauce.ShowInterstitial(onComplete: onWatched, ignoreConditions: ignoreConditions, interstitialType: interstitialType);
            };

            var canBeShown = VoodooSauce.InterstitialCanBeShown();

            var preAdsAction = new OrderedRecursiveActions(onComplete: onShowInterstitial);

            onPreInterstitialOrderedAction?.Invoke(preAdsAction, canBeShown);

            preAdsAction.Execute(); 
            #endregion

        }



    }
}
#endif