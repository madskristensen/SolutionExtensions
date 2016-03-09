using System.Windows;

namespace SolutionExtensions
{
    public partial class InstallerProgress : Window
    {
        public InstallerProgress(int total, string message)
        {
            Loaded += delegate
            {
                bar.Maximum = total;
                lblText.Content = message;
            };

            InitializeComponent();
        }

        public void SetMessage(string message)
        {
            lblText.Content = message;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
