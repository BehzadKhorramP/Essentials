using UnityEngine;
using UnityEngine.Events;


namespace BEH.IAP
{
    public class IAPInProgress : PersistentSingleton<IAPInProgress>
    {
        const string k_PrefabName = "IAPInProgress";

        [Space(10)] public UnityEvent m_OnInProgress;
        [Space(10)] public UnityEvent m_OnDisappear;
        [Space(10)] public UnityEvent m_OnFailed;



        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void InitializeIAPInProgressOnLoad()
        {
            BEHUtility.TryLoadAndInstantiate<IAPInProgress>(k_PrefabName);
        }


        private void OnEnable()
        {
          
        }

        private void OnDisable()
        {          
           
        }
       
        private void Start()
        {
            iapInProgressDisappear();
        }



        public static void Appear()
        {
            Instance?.iapInProgressAppear();
        }
        public static void Disappear()
        {
            Instance?.iapInProgressDisappear();
        }
        public static void Failed()
        {
            Instance?.iapInProgressFailed();
        }




        private void iapInProgressAppear()
        {
            m_OnInProgress?.Invoke();
        }
        private void iapInProgressDisappear()
        {
            m_OnDisappear?.Invoke();
        }
        private void iapInProgressFailed()
        {
            m_OnFailed?.Invoke();
        }



        // also called from outside 
        public void z_IAPAappear()
        {
            iapInProgressAppear();
        }
        public void z_IAPDisappear()
        {
            iapInProgressDisappear();
        }
    }

}