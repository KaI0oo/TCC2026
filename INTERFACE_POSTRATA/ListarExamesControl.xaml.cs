using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using INTERFACE_POSTRATA.Banco;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA
{
    public partial class ListarExamesControl : UserControl
    {
        public ListarExamesControl()
        {
            InitializeComponent();
            CarregarExames();
        }

        public void CarregarExames(string cpf = null)
        {
            try
            {
                using (MySqlConnection conn = Conexao.ObterConexao())
                {
                    // Seleciona exames e faz JOIN com paciente para obter nome
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

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this) as Window1;
            if (win != null)
            {
                // Restaurar o painel principal usando o mesmo método do menu Início
                win.RestoreMainPanel();
            }
        }
    }
}
