using System;
using UnityEngine;

namespace MadApper.Moves {
    public class MovesSystemUI : MonoBehaviour {
        public static Action ShowMovesCounter;
        public static Action HideMovesCounter;
        [SerializeField]
        private MoveCounterUI moveCounter;
        private void OnEnable() {
            MovesSystem.OnMovesInitialized += Initialize;
            MovesSystem.OnMovesChanged += UpdateMoves;
            MovesSystem.OnMovesReset += OnReset;
            ShowMovesCounter += Show;
            HideMovesCounter += Hide;
        }

        private void OnDisable() {
            ShowMovesCounter -= Show;
            HideMovesCounter -= Hide;
            MovesSystem.OnMovesInitialized -= Initialize;
            MovesSystem.OnMovesChanged -= UpdateMoves;
            MovesSystem.OnMovesReset -= OnReset;
        }
        private void Hide() {
            moveCounter.gameObject.SetActive(false);
        }
        private void Show() {
            moveCounter.gameObject.SetActive(true);
        }

        public void Initialize(int maxMoves) {
            if (moveCounter == null) {
                return;
            }

            moveCounter.OnCreated(maxMoves);
        }

        public void UpdateMoves(int remainingMoves) {
            if (moveCounter != null) {
                moveCounter.OnUpdate(remainingMoves);
            }
        }

        public void OnReset() {
        }
    }
}