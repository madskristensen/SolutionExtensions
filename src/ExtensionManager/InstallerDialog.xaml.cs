using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SolutionExtensions
{
    /// <summary>
    /// Interaction logic for InstallerDialog.xaml
    /// </summary>
    public partial class InstallerDialog : Window
    {
        private IEnumerable<ExtensionModel> _missingExtensions;
        private ExtensionFileModel _fileModel;

        public InstallerDialog(ExtensionFileModel fileModel, IEnumerable<ExtensionModel> missingExtensions)
        {
            InitializeComponent();
            _fileModel = fileModel;
            _missingExtensions = missingExtensions;

            Loaded += OnLoaded;
        }

        public IEnumerable<ExtensionModel> SelectedExtensions { get; private set; }

        public bool NeverShowAgainForSolution
        {
            get { return nevershow.IsChecked.Value; }
            set { nevershow.IsChecked = value; }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            btnInstall.Focus();


            foreach (var category in _fileModel.Extensions.Keys)
            {
                Label label = new Label();
                label.Content = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(category);
                label.FontWeight = FontWeights.Bold;
                label.HorizontalAlignment = HorizontalAlignment.Stretch;
                panel.Children.Add(label);

                var models = _fileModel.Extensions[category];

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

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            List<ExtensionModel> list = new List<ExtensionModel>();

            foreach (CheckBox box in panel.Children.OfType<CheckBox>())
            {
                if (box == null || !box.IsEnabled || !box.IsChecked.Value)
                    continue;

                list.Add((ExtensionModel)box.Tag);
            }

            SelectedExtensions = list;

            DialogResult = true;
            Close();
        }
    }
}
