using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatUI
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void SetProperty<T>(ref T member, T val, [CallerMemberName] string propertyName = null)
        {
            SetPropertyExplicit(ref member, val, propertyName);
        }

        protected virtual void SetPropertyExplicit<T>(ref T member, T val, string propertyName = null)
        {
            if (Equals(member, val))
            {
                return;
            }
            member = val;
            _RaisePropertyChange(propertyName);
        }

        protected virtual void SetPropertyWithAction<T>(ref T member, T val, Action<T> onChange, [CallerMemberName] string propertyName = null)
        {
            _SetPropertyWithAction(ref member, val, onChange, propertyName);
        }

        protected virtual void SetPropertyWithActionTask<T>(ref T member, T val, Func<T, Task> onChange, [CallerMemberName] string propertyName = null)
        {
            _SetPropertyWithAction(ref member, val, x => onChange(x), propertyName);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChangedExplicit(propertyName);
        }

        protected virtual void OnPropertyChangedExplicit(string propertyName = null)
        {
            _RaisePropertyChange(propertyName);
        }

        private void _SetPropertyWithAction<T>(ref T member, T val, Action<T> onChange, string propertyName)
        {
            if (object.Equals(member, val))
            {
                return;
            }
            member = val;
            _RaisePropertyChange(propertyName);
            onChange(val);
        }

        private void _RaisePropertyChange(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _executeMethod;
        private readonly Func<bool> _canExecuteMethodFunc;

        public RelayCommand(Action executeMethod)
        {
            _executeMethod = executeMethod;
        }

        public RelayCommand(Action executeMethod, Func<bool> canExecuteMethodFunc)
        {
            _executeMethod = executeMethod;
            _canExecuteMethodFunc = canExecuteMethodFunc;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteMethodFunc?.Invoke() ?? _executeMethod != null;
        }

        public void Execute(object parameter)
        {
            _executeMethod?.Invoke();
        }

        public event EventHandler CanExecuteChanged = delegate { };
    }
}
