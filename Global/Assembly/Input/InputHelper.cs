using UnityEngine;

namespace MadApper.Input
{
    public class InputHelper : MonoBehaviour
    {
        [SerializeField] UnityEventDelayList<Vector2> onPressed;
        [SerializeField] UnityEventDelayList<Vector2> onTapped;
        [SerializeField] UnityEventDelayList<SwipeInput> onSwiped;
        [SerializeField] UnityEventDelayList onReleased;

        private void OnEnable()
        {
            var instance = InputManager.Instance;

            if (instance != null)
            {
                instance.Pressed(Pressed);
                instance.Tapped(Tapped);
                instance.Swiped(Swiped);
                instance.Released(Released);
            }
        }
        private void OnDisable()
        {
            var instance = InputManager.Instance;

            if (instance != null)
            {
                instance.PressedUnsubscribe(Pressed);
                instance.TappedUnsubscribe(Tapped);
                instance.SwipedUnsubscribe(Swiped);
                instance.ReleasedUnsubscribe(Released);
            }
        }

        private void Pressed(SwipeInput obj) => onPressed?.Invoke(obj.StartPosition);
        private void Tapped(TapInput obj) => onTapped?.Invoke(obj.ReleasePosition);
        private void Swiped(SwipeInput obj) => onSwiped?.Invoke(obj);
        private void Released() => onReleased?.Invoke();
    }

}