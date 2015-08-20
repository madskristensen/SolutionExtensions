using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SolutionExtensions
{
    /// <summary>
    /// Interaction logic for InstallerDialog.xaml
    /// </summary>
    public partial class InstallerDialog : Window
    {
        private IEnumerable<IExtensionModel> _missingExtensions;
        private ExtensionFileModel _extensionFileModel;

        public InstallerDialog(ExtensionFileModel fileModel, IEnumerable<IExtensionModel> missingExtensions)
        {
            InitializeComponent();
            _extensionFileModel = fileModel;
            _missingExtensions = missingExtensions;

            Loaded += OnLoaded;
        }

        public InstallerDialog(IEnumerable<IExtensionModel> missingExtensions)
        {
            InitializeComponent();
            _missingExtensions = missingExtensions;

            Loaded += OnLoaded;
        }

        public IEnumerable<IExtensionModel> SelectedExtensions { get; private set; }

        public bool NeverShowAgainForSolution
        {
            get { return nevershow.IsChecked.Value; }
            set { nevershow.IsChecked = value; }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            btnInstall.Focus();
            if (_extensionFileModel != null)
            {
                AddExtensionModels();
            }
            else 
            {
                AddSuggestionModels();
            }
        }

        private void AddExtensionModels()
        {
            foreach (var category in _extensionFileModel.Extensions.Keys)
            {
                Label label = new Label();
                label.Content = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(category);
                label.FontWeight = FontWeights.Bold;
                label.HorizontalAlignment = HorizontalAlignment.Stretch;
                panel.Children.Add(label);

                var models = _extensionFileModel.Extensions[category];

                foreach (var model in models)
                {
                    CheckBox box = new CheckBox();
                    box.Content = model.Name;
                    box.Tag = model;
                    box.Margin = new Thickness(10, 0, 0, 5);
                    box.IsChecked = true;
                    box.ToolTip = model.Description;
                    box.IsEnabled = _missingExtensions.Contains(model);

                    if (!box.IsEnabled)
                        box.Content = box.Content + " (already installed)";

                    panel.Children.Add(box);
                }
            }
        }

        private void AddSuggestionModels()
        {
            txtNever.Text = "Never show again for this file type";

            foreach (IExtensionModel model in _missingExtensions)
            {
                CheckBox box = new CheckBox();
                box.Content = model.Name;
                box.Tag = model;
                box.Margin = new Thickness(10, 0, 0, 5);
                box.IsChecked = true;
                box.ToolTip = model.Description;

                panel.Children.Add(box);
            }
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            List<IExtensionModel> list = new List<IExtensionModel>();

            foreach (CheckBox box in panel.Children.OfType<CheckBox>())
            {
                if (box == null || !box.IsEnabled || !box.IsChecked.Value)
                    continue;

                list.Add((IExtensionModel)box.Tag);
            }

            SelectedExtensions = list;

            DialogResult = true;
            Close();
        }
    }
}
