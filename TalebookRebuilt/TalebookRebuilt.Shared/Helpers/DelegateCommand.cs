using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace TalebookRebuilt.Helpers
{
    public class DelegateCommand<T> : ICommand
    {
        Func<T, bool> canExecute;
        Action<T> exectue;

        public DelegateCommand(Action<T> execute) : this(null, execute) { }

        public DelegateCommand(Func<T, bool> _canExecute, Action<T> _execute)
        {
            this.canExecute = _canExecute;
            this.exectue = _execute;
        }

        #region Members

        public event EventHandler CanExecuteChanged;

        bool ICommand.CanExecute(object parameter)
        {
            try
            {
                return CanExecute((T)parameter);
            }
            catch { return false; }
        }

        void ICommand.Execute(object parameter)
        {
            Execute((T)parameter);
        }
        #endregion
        #region Public methods

        public bool CanExecute(T parameter)
        {
            return((canExecute == null) || canExecute(parameter));
        }        

        public void Execute(T parameter)
        {
            if (exectue != null)
            {
                this.exectue(parameter);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(EventArgs.Empty);
        }
        #endregion

        #region Protected methods
        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            var handler = CanExecuteChanged;
            if(handler != null)
            {
                handler(this, e);
            }
        }
        #endregion
    }
}
