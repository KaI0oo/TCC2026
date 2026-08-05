using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using INTERFACE_POSTRATA.Banco;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA
{
    public partial class ListarAnamnesesControl : UserControl
    {
        public ListarAnamnesesControl()
        {
            InitializeComponent();
            CarregarAnamneses();
        }

        public void CarregarAnamneses(string cpf = null)
        {
            try
            {
                using (MySqlConnection conn = Conexao.ObterConexao())
                {
                    string sql = @"SELECT id_anamnese, cpf_paciente, possui_doenca, doencas, inicio_tratamento, fim_tratamento FROM anamnese";
                    if (!string.IsNullOrEmpty(cpf)) sql += " WHERE cpf_paciente = @cpf";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        if (!string.IsNullOrEmpty(cpf)) cmd.Parameters.AddWithValue("@cpf", cpf);
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgAnamneses.ItemsSource = dt.DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar anamneses: {ex.Message}");
            }
        }

        private void BtnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            CarregarAnamneses(txtPesquisa.Text.Trim());
        }

        private void BtnTodos_Click(object sender, RoutedEventArgs e)
        {
            txtPesquisa.Text = string.Empty;
            CarregarAnamneses();
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this) as Window1;
            if (win != null) win.RestoreMainPanel();
        }
    }
}
