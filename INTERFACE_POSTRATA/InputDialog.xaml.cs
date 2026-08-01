using System.Windows;

namespace INTERFACE_POSTRATA
{
    public partial class InputDialog : Window
    {
        public string Valor { get; private set; }
        public string Prompt
        {
            get => txtPrompt?.Text ?? string.Empty;
            set { if (txtPrompt != null) txtPrompt.Text = value; }
        }

        public InputDialog()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Valor = txtValor.Text?.Trim();
            this.DialogResult = true;
            this.Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
