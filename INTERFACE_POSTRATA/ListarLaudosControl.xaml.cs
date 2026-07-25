using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using INTERFACE_POSTRATA.Banco;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA
{
    public partial class ListarLaudosControl : UserControl
    {
        public ListarLaudosControl()
        {
            InitializeComponent();
            CarregarLaudos();
        }

        public void CarregarLaudos(string cpf = null)
        {
            try
            {
                using (MySqlConnection conn = Conexao.ObterConexao())
                {
                    // Seleciona laudos com informações do exame e do paciente
                    string sql = @"SELECT l.id_laudo, l.id_exame, l.classificacao, l.interpretacao, l.data_laudo,
                                           e.cpf_paciente, p.nome AS paciente_nome
                                    FROM laudo l
                                    INNER JOIN exame e ON e.id_exame = l.id_exame
                                    INNER JOIN paciente p ON p.cpf = e.cpf_paciente";

                    if (!string.IsNullOrEmpty(cpf)) sql += " WHERE e.cpf_paciente = @cpf";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        if (!string.IsNullOrEmpty(cpf)) cmd.Parameters.AddWithValue("@cpf", cpf);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgLaudos.ItemsSource = dt.DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar laudos: {ex.Message}");
            }
        }

        private void BtnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            CarregarLaudos(txtPesquisa.Text.Trim());
        }

        private void BtnTodos_Click(object sender, RoutedEventArgs e)
        {
            txtPesquisa.Text = string.Empty;
            CarregarLaudos();
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this) as Window1;
            if (win != null) win.ClearMainContent();
        }
    }
}
