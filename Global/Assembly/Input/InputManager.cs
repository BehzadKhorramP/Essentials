using MadApper;
using MadApper.Singleton;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace MadApper.Input
{

    [Serializable]
    public class InputManagerSettings
    {
        public bool CheckPinching;
    }

    public class InputManager : PersistentSingleton<InputManager>, IActiveableSystem
    {
        const string k_PrefabName = "InputManagerMAD";
        public string i_PrefabName => k_PrefabName;

        [Space(10)][SerializeField] SingletonScriptableHelper<InputManagerSettingsSO> m_Settings;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            if (!IActiveableSystem.IsActive(k_PrefabName))
                return;

            PointerInputComposite.Register();
            MADUtility.TryLoadAndInstantiate<InputManager>(k_PrefabName);
        }

        protected override void Awake()
        {
            base.Awake();

            var scene = new SceneChangeSubscriber.Builder()
                .SetOnSceneToBeChangedReset(Reset)
                .Build();

            m_Controller.Setup(m_Settings.RunTimeValue);
        }

        private void Reset(string scene)
        {
            m_Controller.Reset(scene);
        }

        public GestureController m_Controller;

        public Vector2 GetPointerPosition() => m_Controller.GetPointerPosition();

        public void Pressed(Action<SwipeInput> callback) => m_Controller.Pressed += callback;
        public void Dragged(Action<SwipeInput> callback) => m_Controller.Dragged += callback;
        public void Released(Action callback) => m_Controller.Released += callback;
        public void Tapped(Action<TapInput> callback) => m_Controller.Tapped += callback;
        public void Swiped(Action<SwipeInput> callback) => m_Controller.Swiped += callback;
        public void Pinching(Action<PinchInput> callback) => m_Controller.GetInputManager().Pinching += callback;
        public void Scrolled(Action<float> callback) => m_Controller.GetInputManager().Scrolled += callback;
        public void PinchingFinished(Action callback) => m_Controller.GetInputManager().OnPinchingFinished += callback;



        public void PressedUnsubscribe(Action<SwipeInput> callback)
        {
            // cause they might be destroyed earlier than the object that is calling this method
            if (m_Controller == null) return;

            m_Controller.Pressed -= callback;
        }
        public void DraggedUnsubscribe(Action<SwipeInput> callback)
        {
            // cause they might be destroyed earlier than the object that is calling this method
            if (m_Controller == null) return;

            m_Controller.Dragged -= callback;
        }
        public void ReleasedUnsubscribe(Action callback)
        {
            if (m_Controller == null) return;

            m_Controller.Released -= callback;
        }
        public void TappedUnsubscribe(Action<TapInput> callback)
        {
            if (m_Controller == null) return;

            m_Controller.Tapped -= callback;
        }
        public void SwipedUnsubscribe(Action<SwipeInput> callback)
        {
            if (m_Controller == null) return;

            m_Controller.Swiped -= callback;
        }
        public void PinchingUnsubscribe(Action<PinchInput> callback)
        {
            if (m_Controller == null) return;
            if (m_Controller.GetInputManager() == null) return;

            m_Controller.GetInputManager().Pinching -= callback;
        }
        public void ScrolledUnsubscribe(Action<float> callback)
        {
            if (m_Controller == null) return;
            if (m_Controller.GetInputManager() == null) return;

            m_Controller.GetInputManager().Scrolled -= callback;
        }
        public void PinchingFinishedUnsubscribe(Action callback)
        {
            if (m_Controller == null) return;
            if (m_Controller.GetInputManager() == null) return;

            m_Controller.GetInputManager().OnPinchingFinished -= callback;
        }

    }

}