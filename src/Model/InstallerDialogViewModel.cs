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

        protected void OnPropertyChanged(string propertyName)
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
        private bool? _isChecked;
        private bool _skipApplyCheckedStateChange;
        private bool _suppressCheckedStateRecalculate;

        public string Name
        {
            get { return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_name); }
            set { Set(ref _name, value, StringComparer.Ordinal); }
        }

        public List<ExtensionVisualModel> Extensions
        {
            get { return _extensions; }
            set
            {
                if (Set(ref _extensions, value))
                {
                    UpdateCheckedState();
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        public int CheckedCount
        {
            get { return Extensions.Count(x => x.IsChecked && x.IsEnabled); }
        }

        public int AvailableCount
        {
            get { return Extensions.Count(x => x.IsEnabled); }
        }

        public int InstalledCount
        {
            get { return Extensions.Count(x => !x.IsEnabled); }
        }

        public bool IsExpanded { get; set; }

        public bool IsEnabled
        {
            get { return Extensions.Any(x => x.IsEnabled); }
        }

        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (Set(ref _isChecked, value) && value.HasValue && !_skipApplyCheckedStateChange)
                {
                    _suppressCheckedStateRecalculate = true;

                    foreach (ExtensionVisualModel model in Extensions.Where(x => x.IsEnabled))
                    {
                        model.IsChecked = value.Value;
                    }

                    _suppressCheckedStateRecalculate = false;

                    OnPropertyChanged("CheckedCount");
                    OnPropertyChanged("AvailableCount");
                    OnPropertyChanged("InstalledCount");
                }
            }
        }

        public void UpdateCheckedState()
        {
            if (_suppressCheckedStateRecalculate)
            {
                return;
            }

            _skipApplyCheckedStateChange = true;
            UpdateCheckedStateInternal();
            _skipApplyCheckedStateChange = false;
        }

        private void UpdateCheckedStateInternal()
        {
            if (Extensions == null)
            {
                IsChecked = true;
                return;
            }

            IEnumerator<ExtensionVisualModel> enumerator = Extensions.Where(x => x.IsEnabled).GetEnumerator();

            if (!enumerator.MoveNext())
            {
                IsChecked = true;
                return;
            }

            bool isChecked = enumerator.Current.IsChecked;

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.IsChecked != isChecked)
                {
                    IsChecked = null;
                    return;
                }
            }

            IsChecked = isChecked;
        }
    }

    public class ExtensionVisualModel : BindableBase
    {
        private bool _isChecked;
        private readonly ExtensionCategory _owner;

        public ExtensionVisualModel(ExtensionCategory owner, IExtensionModel model, bool isChecked, bool isEnabled)
        {
            Model = model;
            IsEnabled = isEnabled;
            _isChecked = isChecked || !isEnabled;
            _owner = owner;
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
            set
            {
                if (Set(ref _isChecked, value))
                {
                    _owner.UpdateCheckedState();
                }
            }
        }

        public ICommand OpenWebSiteCommand { get; }

        public string Link { get { return Model.Link; } }

        public bool IsEnabled { get; }
    }

    public class CategoryConfiguration
    {
        public static readonly CategoryConfiguration DefaultConfiguration = new CategoryConfiguration(null, 0, true, true);

        public CategoryConfiguration(string categoryName, int sortPriority, bool defaultCheckedState, bool defaultExpandedState)
        {
            CategoryName = categoryName;
            SortPriority = sortPriority;
            DefaultCheckedState = defaultCheckedState;
            DefaultExpandedState = defaultExpandedState;
        }

        public string CategoryName { get; }

        public int SortPriority { get; }

        public bool DefaultCheckedState { get; }

        public bool DefaultExpandedState { get; }
    }

    public class InstallerDialogViewModel : BindableBase
    {
        private List<ExtensionCategory> _categories;

        private static Dictionary<string, CategoryConfiguration> _categoryConfigurations = new Dictionary<string, CategoryConfiguration>
        {
            {"General", new CategoryConfiguration("General", 100, false, false)}
        };

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

        private static CategoryConfiguration GetConfig(string name)
        {
            CategoryConfiguration config;
            if (!_categoryConfigurations.TryGetValue(name, out config))
            {
                config = CategoryConfiguration.DefaultConfiguration;
            }

            return config;
        }

        public static InstallerDialogViewModel From(IEnumerable<IExtensionModel> extensions)
        {
            var installed = ExtensionInstalledChecker.Instance.GetInstalledExtensions();
            InstallerDialogViewModel temp = new InstallerDialogViewModel();

            foreach (IGrouping<string, IExtensionModel> grouping in extensions.GroupBy(x => x.Category).OrderBy(x => GetConfig(x.Key).SortPriority).ThenBy(x => x.Key))
            {
                CategoryConfiguration config = GetConfig(grouping.Key);
                ExtensionCategory category = new ExtensionCategory
                {
                    IsExpanded = config.DefaultExpandedState,
                    Name = grouping.Key
                };

                var children = grouping.Select(x => new ExtensionVisualModel(category, x, config.DefaultCheckedState, installed.All(i => i.Header.Identifier != x.ProductId))).ToList();
                category.Extensions = children;
                temp.AllExtensions.AddRange(children);

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
        private readonly Action<object> _action;
        private readonly Func<object, bool> _canExecuteFunc;
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
