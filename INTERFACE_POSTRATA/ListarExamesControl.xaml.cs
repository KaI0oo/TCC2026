using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using INTERFACE_POSTRATA.Banco;
using INTERFACE_POSTRATA.Helpers;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA
{
    public partial class ListarExamesControl : UserControl
    {
        private readonly bool _modoEdicao;

        public ListarExamesControl(bool modoEdicao = false)
        {
            _modoEdicao = modoEdicao;
            InitializeComponent();
            ConfigureAcesso();
            CarregarExames();
        }

        private void ConfigureAcesso()
        {
            if (_modoEdicao)
                btnEditar.Visibility = Visibility.Visible;

            if (Session.IsSecretaria)
                btnTodos.Visibility = Visibility.Collapsed;
        }

        public void CarregarExames(string cpf = null)
        {
            try
            {
                using (MySqlConnection conn = Conexao.ObterConexao())
                {
                    string sql = @"SELECT e.id_exame, e.cpf_paciente, p.nome AS paciente_nome, e.psa_total, e.psa_livre, e.densidade_psa, e.data_exame, e.caminho_pdf
                                   FROM exame e
                                   INNER JOIN paciente p ON p.cpf = e.cpf_paciente";

                    if (!string.IsNullOrEmpty(cpf)) sql += " WHERE e.cpf_paciente = @cpf";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        if (!string.IsNullOrEmpty(cpf)) cmd.Parameters.AddWithValue("@cpf", cpf);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgExames.ItemsSource = dt.DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar exames: {ex.Message}");
            }
        }

        private void BtnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            CarregarExames(txtPesquisa.Text.Trim());
        }

        private void BtnTodos_Click(object sender, RoutedEventArgs e)
        {
            txtPesquisa.Text = string.Empty;
            CarregarExames();
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (dgExames.SelectedItem is not DataRowView row)
            {
                MessageBox.Show("Selecione um exame na lista para editar.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(row["id_exame"]?.ToString(), out int idExame))
            {
                MessageBox.Show("Exame selecionado inválido.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var tela = new CadastroExame(idExame);
            tela.Show();
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this) as Window1;
            if (win != null)
                win.RestoreMainPanel();
        }
    }
}
