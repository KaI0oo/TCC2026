using System.Linq;
using System.Windows;

namespace INTERFACE_POSTRATA
{
    public partial class TelaSecretaria : Window
    {
        public TelaSecretaria()
        {
            InitializeComponent();
        }

        private void AbrirSistema_Click(object sender, RoutedEventArgs e)
        {
            // Oculta a tela inicial (não fecha) — evita múltiplas instâncias.
            this.Hide();

            // Reaproveita Window1 se já existir, sem janelas duplicadas.
            var existing = Application.Current.Windows.OfType<Window1>().FirstOrDefault();
            if (existing != null)
            {
                if (!existing.IsVisible) existing.Show();
                existing.Activate();
                return;
            }

            Helpers.NavigationHelper.ShowMainWindow();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Limpa a sessão
            Helpers.Session.CurrentFuncionarioId = null;
            Helpers.Session.CurrentFuncionarioName = null;
            Helpers.Session.CurrentFuncionarioCrm = null;
            Helpers.Session.CurrentFuncionarioCargo = null;

            // Fecha qualquer Window1 aberta (shell principal)
            foreach (Window w in Application.Current.Windows)
            {
                if (w is Window1 win)
                {
                    win.Close();
                    break;
                }
            }

            // Fecha esta tela
            this.Close();

            // Abre (ou reativa) o Login
            var login = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (login != null)
            {
                if (!login.IsVisible) login.Show();
                login.Activate();
            }
            else
            {
                new MainWindow().Show();
            }
        }

        private void Fechar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
