using System;
using System.Windows.Input;

namespace TinkoffTrader.ViewModels
{
    public class BaseCommand : ICommand
    {
        private Action Action { get; }
        
        private Action<object> ActionWithParameter { get; }

        private bool _disabled;

        /// <summary>
        /// True, если выполнение команды запрещено
        /// </summary>
        public bool Disabled
        {
            get => _disabled;
            set
            {
                if (_disabled == value) return;

                _disabled = value;

                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        } 

        /// <summary>
        /// Базовая команда
        /// </summary>
        /// <param name="action">Действие</param>
        public BaseCommand(Action action)
        {
            Action = action;
        }

        /// <summary>
        /// Базовая команда
        /// </summary>
        /// <param name="action">Действие с параметром</param>
        public BaseCommand(Action<object> action)
        {
            ActionWithParameter = action;
        }

        #region Implementation of ICommand

        /// <inheritdoc />
        public bool CanExecute(object parameter)
        {
            return !Disabled;
        }

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            if (Action != null)
            {
                Action.Invoke();
            }
            else
            {
                ActionWithParameter.Invoke(parameter);
            }
        }

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged;

        #endregion
    }
}
