using System.Windows;

namespace INTERFACE_POSTRATA
{
    public partial class TelaSecretaria : Window
    {
        public TelaSecretaria()
        {
            InitializeComponent();
        }

        private void OpenSystem_Click(object sender, RoutedEventArgs e)
        {
            Helpers.NavigationHelper.ShowMainWindow();
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var win = Application.Current.Windows.OfType<Window1>().FirstOrDefault();
            if (win != null)
            {
                win.RestoreMainPanel();
            }
            this.Close();
        }
    }
}
