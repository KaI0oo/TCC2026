using System.Windows;

namespace INTERFACE_POSTRATA
{
    public partial class PasswordPrompt : Window
    {
        public string Password => pwd?.Password ?? string.Empty;

        public PasswordPrompt()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
