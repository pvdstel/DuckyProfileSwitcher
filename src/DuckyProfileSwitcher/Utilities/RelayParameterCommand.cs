using System;
using System.Windows.Input;

namespace DuckyProfileSwitcher.Utilities
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> execute;
        private readonly Func<T?, bool>? canExecute;

        private event EventHandler? CanExecuteChangedInternal;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                CanExecuteChangedInternal += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
                CanExecuteChangedInternal -= value;
            }
        }

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = default)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (canExecute == default)
            {
                return true;
            }

            if (parameter is T dt)
            {
                return canExecute(dt);
            }
            else
            {
                return canExecute(default);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChangedInternal?.Invoke(this, new EventArgs());
        }

        public void Execute(object parameter)
        {
            if (parameter is T dt)
            {
                execute(dt);
            }
            else
            {
                execute(default);
            }
        }
    }
}
