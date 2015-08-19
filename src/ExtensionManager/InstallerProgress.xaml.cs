using System.Windows;
using System.Windows.Media;

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
            //bar.Value += 1;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
