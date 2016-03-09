using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SolutionExtensions
{
    public partial class InstallerDialog : Window
    {
        private IEnumerable<IExtensionModel> _missingExtensions;

        public InstallerDialog(IEnumerable<IExtensionModel> missingExtensions)
        {
            InitializeComponent();
            _missingExtensions = missingExtensions;

            Loaded += OnLoaded;
            ViewModel = InstallerDialogViewModel.From(missingExtensions);
        }

        public InstallerDialogViewModel ViewModel
        {
            get { return DataContext as InstallerDialogViewModel; }
            set { DataContext = value; }
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

            if (_missingExtensions.FirstOrDefault() is SuggestionModel)
            {
                txtNever.Text = "Never show again for this file type";
                lblHeadline.Text = "These extensions provide features for the file type";
            }

            //AddExtensionModels();
        }

        //private void AddExtensionModels()
        //{
        //    string category = null;
        //    var installed = ExtensionInstalledChecker.Instance.GetInstalledExtensions();
        //    var extensions = _missingExtensions.OrderBy(e => e.Category).ThenBy(e => e.Name);

        //    foreach (var ext in extensions)
        //    {
        //        if (ext.Category != category)
        //        {
        //            Label label = new Label();
        //            label.Content = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ext.Category);
        //            label.FontWeight = FontWeights.Bold;
        //            label.HorizontalAlignment = HorizontalAlignment.Stretch;
        //            panel.Children.Add(label);
        //            category = ext.Category;
        //        }

        //        CheckBox box = new CheckBox();
        //        box.Content = ext.Name;
        //        box.Tag = ext;
        //        box.Margin = new Thickness(10, 0, 0, 5);
        //        box.IsChecked = true;
        //        box.ToolTip = ext.Description;
        //        box.IsEnabled = !installed.Any(i => i.Header.Identifier == ext.ProductId);

        //        if (!box.IsEnabled)
        //        {
        //            box.Content = box.Content + " (already installed)";
        //            ToolTipService.SetShowOnDisabled(box, true);
        //        }

        //        panel.Children.Add(box);
        //    }
        //}

        //private void AddSuggestionModels()
        //{
        //    txtNever.Text = "Never show again for this file type";
        //    lblHeadline.Text = "These extensions provide features for the file type";

        //    Label label = new Label();
        //    label.Content = "Extensions";
        //    label.FontWeight = FontWeights.Bold;
        //    label.HorizontalAlignment = HorizontalAlignment.Stretch;
        //    panel.Children.Add(label);

        //    foreach (IExtensionModel model in _missingExtensions)
        //    {
        //        CheckBox box = new CheckBox();
        //        box.Content = model.Name;
        //        box.Tag = model;
        //        box.Margin = new Thickness(10, 0, 0, 5);
        //        box.IsChecked = true;
        //        box.ToolTip = model.Description;

        //        panel.Children.Add(box);
        //    }
        //}

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            List<IExtensionModel> list = ViewModel.AllExtensions.Where(x => x.IsEnabled && x.IsChecked).Select(x => x.Model).ToList();

            SelectedExtensions = list;

            DialogResult = true;
            Close();
        }
    }
}
