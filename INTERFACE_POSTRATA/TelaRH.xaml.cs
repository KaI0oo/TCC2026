using System.Linq;
using System.Windows;

namespace INTERFACE_POSTRATA
{
    public partial class TelaRH : Window
    {
        public TelaRH()
        {
            InitializeComponent();
        }

        private void AbrirSistema_Click(object sender, RoutedEventArgs e)
        {
            // Oculta a tela inicial do RH (não fecha) para evitar
            // múltiplas instâncias e permitir reutilização.
            this.Hide();

            // Reaproveita a RhMainWindow já aberta (se houver) — sem janelas duplicadas.
            var existing = Application.Current.Windows.OfType<RhMainWindow>().FirstOrDefault();
            if (existing != null)
            {
                if (!existing.IsVisible) existing.Show();
                existing.Activate();
                return;
            }

            var main = new RhMainWindow();
            main.Show();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // 1) Garante/obtém uma única MainWindow e MOSTRA-A PRIMEIRO.
            //    Sem isso, fechar a TelaRH/RhMainWindow com ShutdownMode
            //    OnLastWindowClose encerraria a aplicação.
            MainWindow login = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (login == null)
            {
                login = new MainWindow();
            }
            if (!login.IsVisible)
            {
                login.Show();
            }
            login.Activate();

            // 2) Fecha a RhMainWindow aberta (se houver).
            foreach (Window w in Application.Current.Windows)
            {
                if (w is RhMainWindow rh)
                {
                    rh.Close();
                    break;
                }
            }

            // 3) Fecha esta TelaRH.
            this.Close();

            // 4) Garante que reste apenas uma MainWindow.
            var allLogins = Application.Current.Windows
                .OfType<MainWindow>()
                .ToList();
            for (int i = 1; i < allLogins.Count; i++)
            {
                allLogins[i].Close();
            }

            // 5) Limpa a sessão por último.
            Helpers.Session.CurrentFuncionarioId = null;
            Helpers.Session.CurrentFuncionarioName = null;
            Helpers.Session.CurrentFuncionarioCrm = null;
            Helpers.Session.CurrentFuncionarioCargo = null;
        }

        private void Fechar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
