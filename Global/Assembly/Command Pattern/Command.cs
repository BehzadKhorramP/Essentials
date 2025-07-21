using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BEH.Commands
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class Command<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Action<T> _undo;
        private readonly T _parameter;

        public Command(Action<T> execute, Action<T> undo, T parameter)
        {
            _execute = execute;
            _undo = undo;
            _parameter = parameter;
        }

        public void Execute() => _execute(_parameter);
        public void Undo() => _undo(_parameter);
    }


    public class BatchCommand : ICommand
    {
        private readonly List<ICommand> _commands;

        public BatchCommand(List<ICommand> commands)
        {
            _commands = commands;
        }

        public void Execute()
        {
            foreach (var command in _commands)
            {
                command.Execute();
            }
        }

        public void Undo()
        {
            for (int i = _commands.Count - 1; i >= 0; i--) // Undo in reverse order
            {
                _commands[i].Undo();
            }
        }
    }

}
