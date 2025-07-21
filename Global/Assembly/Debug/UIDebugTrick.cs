using BEH;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MadApper
{
    public class UIDebugTrick : MonoBehaviour
    {
        [SerializeField] int[] code = new int[] { 0, 1, 2, 0, 1 };
        [SerializeField][ReadOnly][AutoGetInChildren] List<Button> m_Buttons;

        [SerializeField] UnityEventDelay m_OnCorrect;

        List<ButtonWithCode> buttonWithCodes;
        List<int> inputCode;
        DateTime? timeStarted;
        float timeValid = 2;

      //  bool isCorrectFound;

        private void Awake()
        {
            buttonWithCodes = new List<ButtonWithCode>();
            inputCode = new List<int>();

            for (int i = 0; i < m_Buttons.Count; i++)
            {
                var button = m_Buttons[i];
                buttonWithCodes.Add(new ButtonWithCode(system: this, button: button, code: i));
            }
        }


       

        void OnTapped(int index)
        {
            //if (isCorrectFound)
            //    return;

            if (!timeStarted.HasValue)
                timeStarted = DateTime.Now;

            var diff = (DateTime.Now - timeStarted.Value).TotalSeconds;

            if (diff > timeValid)
            {
                timeStarted = null;
                inputCode.Clear();
            }

            inputCode.Add(index);

            if (IsCodeCorrect())
            {
                this.Log("correct debug code!");
               // isCorrectFound = true;
                m_OnCorrect?.Invoke();
            }

            bool IsCodeCorrect()
            {               
                if (inputCode.Count < code.Length)
                    return false;

                for (int i = 0; i < code.Length; i++)
                {
                    var c = code[i];
                    var @in = inputCode[i];

                    if (c != @in)
                        return false;
                }

                return true;
            }
        }



        class ButtonWithCode
        {
            UIDebugTrick system;
            int code;

            public ButtonWithCode(UIDebugTrick system, Button button, int code)
            {
                this.system = system;
                this.code = code;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnTapped);
            }

            private void OnTapped()
            {
                system.OnTapped(code);
            }
        }
    }
}
