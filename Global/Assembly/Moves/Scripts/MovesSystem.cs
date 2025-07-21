using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace MadApper.Moves
{    public class MovesSystemCollection : StaticCollection<MovesSystem>
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnInitializeOnLoad()
        {
            OnInitializeOnLoadImpl(onSceneChanged: (scene) =>
            {
                MovesSystem.Reset();
            });
        }
    }
    public class MovesSystem
    {
        // Events
        public static event Action OnMovesExhausted;
        public static event Action<int> OnMovesChanged;
        public static event Action<int> OnMovesInitialized;
        public static event Action OnMovesReset;
        
        private static int s_MaxMoves;
        private static int s_CurrentMoves;
        private static bool s_IsInitialized;
        
        public static bool HasMovesLeft => s_CurrentMoves < s_MaxMoves;
        public static int RemainingMoves => s_MaxMoves - s_CurrentMoves;

        public static void Initialize(int moves)
        {
            s_MaxMoves = Mathf.Max(0, moves);
            s_CurrentMoves = 0;
            s_IsInitialized = true;
            
            OnMovesInitialized?.Invoke(moves);
            OnMovesChanged?.Invoke(RemainingMoves);
        }
        
        public static void TryAddMoves(int moves)
        {
            if (!s_IsInitialized || moves <= 0) {
                return;
            }
            
            s_CurrentMoves = Mathf.Max(0, s_CurrentMoves - moves);
            
            OnMovesChanged?.Invoke(RemainingMoves);
        }

        public static bool TryUseMove()
        {
            if (!s_IsInitialized || !HasMovesLeft) {
                return false;
            }
            
            s_CurrentMoves++;
            
            OnMovesChanged?.Invoke(RemainingMoves);
            
            if (!HasMovesLeft)
            {
                OnMovesExhausted?.Invoke();
            }
            
            return true;
        }

        public static void Reset()
        {
            s_MaxMoves = 0;
            s_CurrentMoves = 0;
            s_IsInitialized = false;
            
            OnMovesReset?.Invoke();
        }
    }
}