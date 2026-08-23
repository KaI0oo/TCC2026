using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using INTERFACE_POSTRATA.Banco;
using INTERFACE_POSTRATA.Helpers;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA
{
    public partial class ListarPacientesControl : UserControl
    {
        public ListarPacientesControl()
        {
            InitializeComponent();
            if (Session.IsSecretaria)
                btnAtualizar.Visibility = Visibility.Collapsed;
            CarregarPacientes();
        }

        public void CarregarPacientes(string filtro = null)
        {
            try
            {
                using (MySqlConnection conn = Conexao.ObterConexao())
                {
                    string sql = "SELECT cpf, nome, idade, telefone FROM paciente";
                    if (!string.IsNullOrEmpty(filtro))
                    {
                        sql += " WHERE nome LIKE @filtro OR cpf LIKE @filtro";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        if (!string.IsNullOrEmpty(filtro)) cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgPacientes.ItemsSource = dt.DefaultView;
                        }
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar pacientes: {ex.Message}");
            }
        }

        private void BtnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            CarregarPacientes(txtPesquisa.Text.Trim());
        }

        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            txtPesquisa.Text = string.Empty;
            CarregarPacientes();
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
