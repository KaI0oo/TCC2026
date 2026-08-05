using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA
{
    public partial class CadastroProfissional : Window
    {
        public CadastroProfissional()
        {
            InitializeComponent();
        }

        private void Salvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string rm = txtRM.Text?.Trim();
                string nome = txtNome.Text?.Trim();
                string crm = txtCRM.Text?.Trim();
                string senha = txtSenha.Password?.Trim();
                string cargo = ((System.Windows.Controls.ComboBoxItem)cbCargo.SelectedItem)?.Content?.ToString();

                if (string.IsNullOrEmpty(rm) || string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(senha) || string.IsNullOrEmpty(cargo))
                {
                    lblStatus.Text = "Todos os campos são obrigatórios.";
                    return;
                }

                using (var conn = Banco.Conexao.ObterConexao())
                {
                    string sql = @"INSERT INTO medico (rm, nome, crm, senha, cargo) VALUES (@rm, @nome, @crm, @senha, @cargo)";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@rm", rm);
                        cmd.Parameters.AddWithValue("@nome", nome);
                        cmd.Parameters.AddWithValue("@crm", crm);
                        cmd.Parameters.AddWithValue("@senha", senha);
                        cmd.Parameters.AddWithValue("@cargo", cargo);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Profissional cadastrado com sucesso.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erro: " + ex.Message;
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
