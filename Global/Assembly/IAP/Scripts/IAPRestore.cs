using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

namespace BEH.IAP
{

    public class IAPRestore : MonoBehaviour
    {

        public TargetedEventSO m_IAPInProgressAppear;
        public TargetedEventSO m_IAPInProgressDisAppear;

        /// <summary>
        /// Type of event fired after a restore transactions was completed.
        /// </summary>
        [Serializable]
        public class OnTransactionsRestoredEvent : UnityEvent<bool, string> { }
        /// <summary>
        /// Event fired after a restore transactions.
        /// </summary>
        [Tooltip("Event fired after a restore transactions.")]
        [Space(10)] public OnTransactionsRestoredEvent onTransactionsRestored = null;


        public void z_Restore()
        {
            var iapSystem = IAPSystem.Instance;

            if (iapSystem == null)
                return;

            m_IAPInProgressAppear?.RaiseEvent();

            if (Application.platform == RuntimePlatform.WSAPlayerX86 ||
                    Application.platform == RuntimePlatform.WSAPlayerX64 ||
                    Application.platform == RuntimePlatform.WSAPlayerARM)
            {
                iapSystem.GetStoreExtensions<IMicrosoftExtensions>().RestoreTransactions();
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer ||
                     Application.platform == RuntimePlatform.OSXPlayer ||
                     Application.platform == RuntimePlatform.tvOS
#if UNITY_VISIONOS
                         || Application.platform == RuntimePlatform.VisionOS
#endif
                     )
            {
                iapSystem.GetStoreExtensions<IAppleExtensions>()
                    .RestoreTransactions(OnTransactionsRestored);
            }
            else if (Application.platform == RuntimePlatform.Android &&
                     StandardPurchasingModule.Instance().appStore == AppStore.GooglePlay)
            {
                iapSystem.GetStoreExtensions<IGooglePlayStoreExtensions>()
                    .RestoreTransactions(OnTransactionsRestored);
            }
            else
            {
                m_IAPInProgressDisAppear.RaiseEvent();

                Debug.LogWarning(Application.platform +
                    " is not a supported platform for the IAP restore button");
            }
        }



        protected void OnTransactionsRestored(bool success, string error)
        {
            m_IAPInProgressDisAppear.RaiseEvent();

            onTransactionsRestored?.Invoke(success, error);
        }

    }

}