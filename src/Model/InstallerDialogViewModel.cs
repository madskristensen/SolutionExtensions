using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SolutionExtensions
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected bool Set<T>(ref T existing, T value, IEqualityComparer<T> comparer = null, [CallerMemberName]string propertyName = null)
        {
            IEqualityComparer<T> realComparer = comparer ?? EqualityComparer<T>.Default;

            if (!realComparer.Equals(existing, value))
            {
                existing = value;
                OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }
    }

    public class ExtensionCategory : BindableBase
    {
        private string _name;
        private List<ExtensionVisualModel> _extensions;

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value, StringComparer.Ordinal); }
        }

        public List<ExtensionVisualModel> Extensions
        {
            get { return _extensions; }
            set { Set(ref _extensions, value); }
        }
    }

    public class ExtensionVisualModel : BindableBase
    {
        private bool _isChecked;

        public ExtensionVisualModel(IExtensionModel model, bool isEnabled)
        {
            Model = model;
            IsEnabled = isEnabled;
            _isChecked = true;
            OpenWebSiteCommand = ActionCommand.Create(OpenWebSite);
        }

        private void OpenWebSite()
        {
            if (!string.IsNullOrEmpty(Link))
            {
                System.Diagnostics.Process.Start(Link);
            }
        }

        public IExtensionModel Model { get; }

        public string Name { get { return Model.Name; } }

        public bool IsChecked
        {
            get { return _isChecked; }
            set { Set(ref _isChecked, value); }
        }

        public ICommand OpenWebSiteCommand { get; }

        public string Link { get { return Model.Link; } }

        public bool IsEnabled { get; }
    }

    public class InstallerDialogViewModel : BindableBase
    {
        private List<ExtensionCategory> _categories;

        public InstallerDialogViewModel()
        {
            _categories = new List<ExtensionCategory>();
            AllExtensions = new List<ExtensionVisualModel>();
        }

        public List<ExtensionCategory> Categories
        {
            get { return _categories; }
            set { Set(ref _categories, value); }
        }

        public List<ExtensionVisualModel> AllExtensions { get; }

        public static InstallerDialogViewModel From(IEnumerable<IExtensionModel> extensions)
        {
            var installed = ExtensionInstalledChecker.Instance.GetInstalledExtensions();
            InstallerDialogViewModel temp = new InstallerDialogViewModel();

            foreach (IGrouping<string, IExtensionModel> grouping in extensions.GroupBy(x => x.Category))
            {
                var children = grouping.Select(x => new ExtensionVisualModel(x, !installed.Any(i => i.Header.Identifier == x.ProductId))).ToList();
                temp.AllExtensions.AddRange(children);

                ExtensionCategory category = new ExtensionCategory
                {
                    Name = grouping.Key,
                    Extensions = children
                };

                temp.Categories.Add(category);
            }

            return temp;
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (parameter != null) ^ Equals(value, true) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (parameter != null) ^ Equals(value, Visibility.Visible);
        }
    }

    public class ActionCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private Action<object> _action;
        private Func<object, bool> _canExecuteFunc;
        private bool _canExecute;

        public ActionCommand(Action<object> action, Func<object, bool> canExecute, bool initialCanExecute)
        {
            _action = action;
            _canExecuteFunc = canExecute;
            _canExecute = initialCanExecute;
        }

        protected virtual void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter)
        {
            bool oldCanExecute = _canExecute;
            _canExecute = _canExecuteFunc == null || _canExecuteFunc(parameter);

            if (oldCanExecute ^ _canExecute)
            {
                OnCanExecuteChanged();
            }

            return _canExecute;
        }

        public void Execute(object parameter)
        {
            if (_action != null)
            {
                _action(parameter);
            }
        }

        public static ActionCommand Create(Action action, Func<bool> canExecute = null, bool initialCanExecute = true)
        {
            return new ActionCommand(o => action(), o => canExecute != null ? canExecute() : initialCanExecute, initialCanExecute);
        }

        public static ActionCommand Create<T>(Action<T> action, Func<T, bool> canExecute = null, bool initialCanExecute = true)
        {
            return new ActionCommand(o => action((T)o), o => canExecute != null ? canExecute((T)o) : initialCanExecute, initialCanExecute);
        }
    }
}
