using System.Windows;

namespace INTERFACE_POSTRATA
{
    public partial class TelaRH : Window
    {
        public TelaRH()
        {
            InitializeComponent();
        }

        private void OpenSystem_Click(object sender, RoutedEventArgs e)
        {
            // abrir janela principal padrão
            Helpers.NavigationHelper.ShowMainWindow();
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // Restaurar o painel principal da janela principal (Window1) se existir
            var win = Application.Current.Windows.OfType<Window1>().FirstOrDefault();
            if (win != null)
            {
                win.RestoreMainPanel();
            }
            this.Close();
        }

        private void CadastroProfissionais_Click(object sender, RoutedEventArgs e)
        {
            var cad = new CadastroProfissional();
            cad.Owner = this;
            cad.ShowDialog();
        }

        private void AlterarAdministrador_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidade de alterar administrador ainda não implementada.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Conta_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Gerenciar conta (em breve).", "Conta", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
