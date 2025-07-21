using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BEH.Commands
{
    public class CommandSystem : MonoBehaviour
    {
        [ShowInInspector] private readonly Stack<ICommand> _undoStack = new();
        [ShowInInspector] private readonly Stack<ICommand> _redoStack = new();

        public static int s_UndoUsed;

        public static Action<CommandSystem> onCommandsStackChanged;

        #region Getters Setters
        public Stack<ICommand> GetUndoCommands() => _undoStack;
        public Stack<ICommand> GetRedoCommands() => _redoStack;

        #endregion
        public void z_ResetStacks()
        {
            s_UndoUsed = 0;
            _undoStack.Clear();
            _redoStack.Clear();
            onCommandsStackChanged?.Invoke(this);
        }

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear(); // Clear redo stack on new command
            onCommandsStackChanged?.Invoke(this);
        }

        public void ExecuteBatch(List<ICommand> commands)
        {
            ICommand batch = new BatchCommand(commands);
            ExecuteCommand(batch);
        }

        [Button]
        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                ICommand command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);
                onCommandsStackChanged?.Invoke(this);
                s_UndoUsed++;
            }
        }

        [Button]
        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                ICommand command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);
                onCommandsStackChanged?.Invoke(this);
            }
        }
    }
}
