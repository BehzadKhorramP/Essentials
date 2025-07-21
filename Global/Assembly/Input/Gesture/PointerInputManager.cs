using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MadApper.Input
{
    /// <summary>
    /// Input manager that interprets pen, mouse and touch input for mostly drag related controls.
    /// Passes pressure, tilt, twist and touch radius through to drawing components for processing.
    /// </summary>
    /// <remarks>
    /// Couple notes about the control setup:
    ///
    /// - Touch is split off from mouse and pen instead of just using `&lt;Pointer&gt;/position` etc.
    ///   in order to support multi-touch. If we just bind to <see cref="Touchscreen.position"/> and
    ///   such, we will correctly receive the primary touch but the primary touch only. So we put
    ///   bindings for pen and mouse separate to those from touch.
    /// - Mouse and pen are put into one composite. The expectation here is that they are not used
    ///   independently from another and thus don't need to be represented as separate pointer sources.
    ///   However, we could just as well have one <see cref="PointerInputComposite"/> for mice and
    ///   one for pens.
    /// - <see cref="InputAction.passThrough"/> is enabled on <see cref="PointerControls.PointerActions.point"/>.
    ///   The reason is that we want to source arbitrary many pointer inputs through one single actions.
    ///   Without pass-through, the default conflict resolution on actions would kick in and let only
    ///   one of the composite bindings through at a time.
    /// </remarks>
    public class PointerInputManager : MonoBehaviour
    {
        /// <summary>
        /// Event fired when the user presses on the screen.
        /// </summary>
        public event Action<PointerInput, double> Pressed;

        /// <summary>
        /// Event fired as the user drags along the screen.
        /// </summary>
        public event Action<PointerInput, double> Dragged;

        /// <summary>
        /// Event fired when the user releases a press.
        /// </summary>
        public event Action<PointerInput, double> Released;

        public event Action<PinchInput> Pinching;

        public event Action OnPinchingFinished;

        public event Action<float> Scrolled;


        // These are useful for debugging, especially when touch simulation is on.
        [SerializeField] private bool m_UseMouse;
        [SerializeField] private bool m_UsePen;
        [SerializeField] private bool m_UseTouch;

        private InputManagerSettingsSO settings;
        private PointerControls controls;
        private bool isDragging;
        private bool isPinching;
        private float? previousPinchDist;

        public void Setup(InputManagerSettingsSO settings)
        {
            this.settings = settings;

            controls = new PointerControls();

            SyncBindingMask();

            isPinching = false;

            controls.Enable();
            controls.pointer.Enable();

            controls.pointer.point.performed += OnAction;

            // The action isn't likely to actually cancel as we've bound it to all kinds of inputs but we still
            // hook this up so in case the entire thing resets, we do get a call.
            //    controls.pointer.point.canceled += OnAction;

            controls.pointer.scroll.performed += OnScroll;
            controls.pointer.SecondTouchContact.performed += OnPinchStarted;
            controls.pointer.SecondTouchContact.canceled += OnPinchFinished;

            controls?.Enable();

        }     

        protected virtual void OnDisable()
        {
            controls?.Disable();
        }


        private void Update()
        {
            CheckPinching();
        }

        protected void OnAction(InputAction.CallbackContext context)
        {
            if (isPinching)
                return;

            var control = context.control;
            var device = control.device;

            var isMouseInput = device is Mouse;
            var isPenInput = !isMouseInput && device is Pen;

            // Read our current pointer values.
            var drag = context.ReadValue<PointerInput>();
            if (isMouseInput)
                drag.InputId = Helpers.LeftMouseInputId;
            else if (isPenInput)
                drag.InputId = Helpers.PenInputId;

            if (drag.Contact && !isDragging)
            {
                Pressed?.Invoke(drag, context.time);
                isDragging = true;
            }
            else if (drag.Contact && isDragging)
            {
                Dragged?.Invoke(drag, context.time);
            }
            else
            {
                Released?.Invoke(drag, context.time);
                isDragging = false;
            }
        }
        void OnPinchStarted(InputAction.CallbackContext context)
        {
            if (!settings.Value.CheckPinching)
                return;

            previousPinchDist = null;
            isPinching = true;
            isDragging = false;
        }
        void OnPinchFinished(InputAction.CallbackContext context)
        {
            if (!settings.Value.CheckPinching)
                return;

            isPinching = false;
            OnPinchingFinished?.Invoke();
        }
        void CheckPinching()
        {         
            if (!isPinching)
                return;

            try
            {
                var touchPos0 = controls.pointer.Touch0Vector.ReadValue<Vector2>();
                var touchPos1 = controls.pointer.Touch1Vector.ReadValue<Vector2>();

                var distance = Vector2.Distance(touchPos0, touchPos1);

                if (!previousPinchDist.HasValue)
                    previousPinchDist = distance;

                var difference = distance - previousPinchDist.Value;

                if (difference != 0)
                {
                    var input = new PinchInput()
                    {
                        FirstTouchPosition = touchPos0,
                        SecondTouchPosition = touchPos1,
                        Value = difference,
                    };

                    Pinching?.Invoke(input);
                }

                previousPinchDist = distance;
            }
            catch (Exception) { }
        }

        private void OnScroll(InputAction.CallbackContext ctx)
        {
            if (ctx.phase != InputActionPhase.Performed)
                return;

            var scrollDistance = ctx.ReadValue<Vector2>().y;

            Scrolled?.Invoke(scrollDistance);
        }

        public Vector2 GetPointerPosition() => controls.pointer.point.ReadValue<PointerInput>().Position;
        public bool IsPinching() => isPinching;

        private void SyncBindingMask()
        {
            if (controls == null)
                return;

#if UNITY_EDITOR
            m_UseMouse = true;
#else
          //  m_UseMouse = false;
#endif

            if (m_UseMouse && m_UsePen && m_UseTouch)
            {
                controls.bindingMask = null;
                return;
            }

            controls.bindingMask = InputBinding.MaskByGroups(new[]
            {
                m_UseMouse ? "Mouse" : null,
                m_UsePen ? "Pen" : null,
                m_UseTouch ? "Touch" : null
            });
        }

        
    }
}
