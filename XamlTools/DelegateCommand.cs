using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Boredbone.XamlTools
{
    public class DelegateCommand : ICommand
    {
        System.Action execute;
        System.Func<bool> canExecute;

        public event System.EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public DelegateCommand(Action execute) : this(execute, () => true) { }
        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }
        public void Execute(object parameter)
        {
            this.execute();
        }
        public bool CanExecute(object parameter)
        {
            return this.canExecute();
        }
    }
    public class DelegateCommand<T> : ICommand
    {

        private readonly Action<T> execute;
        private readonly Func<bool> canExecute;

        public event System.EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public DelegateCommand(Action<T> execute) : this(execute, () => true) { }

        public DelegateCommand(Action<T> execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public void Execute(object parameter)
        {
            if (parameter is T || parameter is null)
            {
                this.execute((T)parameter);
            }
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute();
        }

    }
}
