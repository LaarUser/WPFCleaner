using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace WpfProjectCleanerMvvmNet5
{
    public class ViewModelBase : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Func<object, bool> _canExecute;
        public RelayCommand( Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute(object parameter)
        {
            if(_canExecute != null)
                return _canExecute(parameter);
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
    //public class RelayCommand<T> : ICommand
    //{
    //    private static bool CanExecute(T parameter)
    //    {
    //        return true;
    //    }
    //    readonly Action<T> _execute;

    //    readonly Func<T, bool> _canExecute;

    //    public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
    //    {
    //        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    //        _canExecute = canExecute ?? CanExecute;
    //    }
    //    public bool CanExecute(object parameter)
    //    {
    //        if (_canExecute != null)
    //            return _canExecute(RelayCommand<T>.TranslateParameter(parameter));
    //        return true;
    //    }
    //    public event EventHandler CanExecuteChanged
    //    {
    //        add
    //        {
    //            if (_canExecute != null)
    //                CommandManager.RequerySuggested += value;
    //        }
    //        remove
    //        {
    //            if (_canExecute != null)
    //                CommandManager.RequerySuggested -= value;
    //        }
    //    }
    //    public void Execute(object parameter)
    //    {
    //        _execute?.Invoke(RelayCommand<T>.TranslateParameter(parameter));
    //    }
    //    private static T TranslateParameter(object parameter)
    //    {

    //      T value;

    //       if (parameter != null && typeof(T).IsEnum)
    //            value = (T)Enum.Parse(typeof(T),(string)parameter);
    //        else
    //            value = (T)parameter;

    //        return value;
    //    }
    //}

    //public class RelayCommand : RelayCommand<object>
    //{
    //    public RelayCommand(Action execute,
    //    Func<bool> canExecute = null)
    //    : base(obj => execute(),
    //    (canExecute == null ? null :
    //    new Func<object, bool>(obj => canExecute())))
    //    {
    //    }
    //}
}
