using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace INTERFACE_POSTRATA
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lblLoginError.Visibility = System.Windows.Visibility.Collapsed;

                string usuario = txtUsuario.Text?.Trim();
                string senha = txtSenha.Password?.Trim();

                if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(senha))
                {
                    lblLoginError.Text = "Usuário e senha são obrigatórios.";
                    lblLoginError.Visibility = System.Windows.Visibility.Visible;
                    return;
                }

                // Aqui você pode implementar autenticação real contra o banco.
                // Para agora, aceitar qualquer par usuário/senha e criar sessão.
                INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoId = 1; // Placeholder (substituir por id do DB após autenticação)
                INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoName = usuario;

                INTERFACE_POSTRATA.Helpers.NavigationHelper.ShowMainWindow();
                this.Close();
            }
            catch (System.Exception ex)
            {
                lblLoginError.Text = "Erro ao tentar realizar login: " + ex.Message;
                lblLoginError.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void Sair_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}