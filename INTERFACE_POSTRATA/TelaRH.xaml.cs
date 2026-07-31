using System.Windows;
using System.Linq;

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

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Encerrar sessão atual
            INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoId = null;
            INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoName = null;
            INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoCrm = null;
            INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoCargo = null;

            // Abrir tela de login sem encerrar a aplicação
            var login = new MainWindow();
            login.Show();
            this.Close();
        }

        private void FecharPrograma_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AlterarAdministrador_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidade de alterar administrador ainda não implementada.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Conta_Click(object sender, RoutedEventArgs e)
        {
            var tela = new GerenciarContaRH();
            tela.Owner = this;
            tela.ShowDialog();
        }

        private void AlterarRHPrincipal_Click(object sender, RoutedEventArgs e)
        {
            var tela = new AlterarRHPrincipal();
            tela.Owner = this;
            tela.ShowDialog();
        }
    }
}
