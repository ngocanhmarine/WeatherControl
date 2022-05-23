using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WeatherControl.Control2.Commands
{
    public class WCActionCommand : ICommand
    {
        private readonly Action<object> action;
        private readonly Predicate<object> canExecute;
        public WCActionCommand(Action<object> action) : this(action, null) { }
        public WCActionCommand(Action<object> action, Predicate<object> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (canExecute(parameter))
            {
                action(parameter);
            }
        }
    }
}
