using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace INTERFACE_POSTRATA
{
    public partial class RhMainWindow : Window
    {
        public RhMainWindow()
        {
            InitializeComponent();
            ApplySessionHeader();
        }

        private void ApplySessionHeader()
        {
            try
            {
                var nome = Helpers.Session.CurrentFuncionarioName ?? "RH";
                var cargo = Helpers.Session.CurrentFuncionarioCargo ?? "Recursos Humanos";
                txtRhHeaderName.Text = nome;
                txtRhHeaderCargo.Text = cargo;
                txtRhWelcome.Text = $"Bem-vindo(a), {nome}";
            }
            catch
            {
                // Cabeçalho é puramente visual — falhas aqui não devem bloquear a janela.
            }
        }

        public void SetRhMainContent(UIElement element)
        {
            RhMainContent.Content = element;
        }

        // ============ HANDLERS DO MENU ============

        private void CadastrarProfissional_Click(object sender, RoutedEventArgs e)
        {
            SetRhMainContent(new RhUserControls.CadastrarProfissionalControl());
        }

        private void GerenciarFuncionarios_Click(object sender, RoutedEventArgs e)
        {
            SetRhMainContent(new RhUserControls.GerenciarFuncionariosControl());
        }

        private void AlterarRhPrincipal_Click(object sender, RoutedEventArgs e)
        {
            SetRhMainContent(new RhUserControls.AlterarRhPrincipalControl());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // 1) Garante/obtém uma única MainWindow e MOSTRA-A PRIMEIRO.
            //    Isso evita que o WPF desligue a aplicação quando fecharmos
            //    a RhMainWindow (ShutdownMode padrão = OnLastWindowClose).
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

            // 2) Fecha a RhMainWindow (a janela atual).
            this.Close();

            // 3) Fecha também a TelaRH oculta, se existir.
            foreach (Window w in Application.Current.Windows)
            {
                if (w is TelaRH tr)
                {
                    tr.Close();
                    break;
                }
            }

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

        private void FecharPrograma_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
