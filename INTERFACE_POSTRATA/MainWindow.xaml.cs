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

                // Autenticação real contra tabela medico
                using (var conn = INTERFACE_POSTRATA.Banco.Conexao.ObterConexao())
                {
                    string sql = @"SELECT rm, nome, cargo FROM medico WHERE rm = @rm AND senha = @senha";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@rm", usuario);
                        cmd.Parameters.AddWithValue("@senha", senha);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                lblLoginError.Text = "Usuário ou senha inválidos.";
                                lblLoginError.Visibility = System.Windows.Visibility.Visible;
                                return;
                            }

                            string nome = reader["nome"]?.ToString() ?? string.Empty;
                            string cargo = reader["cargo"]?.ToString() ?? string.Empty;

                            // Criar sessão com informações básicas
                            INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoId = int.TryParse(reader["rm"]?.ToString(), out int rmVal) ? rmVal : 0;
                            INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoName = nome;

                            // Abrir telas conforme cargo
                            if (cargo.Equals("RH", System.StringComparison.OrdinalIgnoreCase))
                            {
                                var tela = new TelaRH();
                                tela.Show();
                            }
                            else if (cargo.Equals("Medico", System.StringComparison.OrdinalIgnoreCase))
                            {
                                // Médico usa a janela principal (Window1)
                                var tela = new Window1();
                                tela.Show();
                            }
                            else if (cargo.Equals("Secretaria", System.StringComparison.OrdinalIgnoreCase))
                            {
                                var tela = new TelaSecretaria();
                                tela.Show();
                            }
                            else
                            {
                                // Cargo desconhecido: abrir janela principal genérica
                                INTERFACE_POSTRATA.Helpers.NavigationHelper.ShowMainWindow();
                            }

                            this.Close();
                        }
                    }
                }
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