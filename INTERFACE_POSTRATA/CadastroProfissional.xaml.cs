using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA
{
    public partial class CadastroProfissional : Window
    {
        public CadastroProfissional()
        {
            InitializeComponent();
            AtualizarVisibilidadeCRM();
        }

        private void CbCargo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AtualizarVisibilidadeCRM();
        }

        private void AtualizarVisibilidadeCRM()
        {
            string cargo = ((ComboBoxItem)cbCargo.SelectedItem)?.Content?.ToString();
            bool mostrar = string.Equals(cargo, "MEDICO", StringComparison.OrdinalIgnoreCase);

            lblCRM.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
            txtCRM.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;

            if (!mostrar) txtCRM.Clear();
        }

        private void Salvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string usuario = txtUsuario.Text?.Trim();
                string nome = txtNome.Text?.Trim();
                string senha = txtSenha.Password?.Trim();
                string cargo = ((ComboBoxItem)cbCargo.SelectedItem)?.Content?.ToString();
                string crm = string.Equals(cargo, "MEDICO", StringComparison.OrdinalIgnoreCase)
                    ? (txtCRM.Text?.Trim() ?? string.Empty)
                    : null;

                if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(senha) || string.IsNullOrEmpty(cargo))
                {
                    lblStatus.Text = "Todos os campos são obrigatórios.";
                    return;
                }

                if (string.Equals(cargo, "MEDICO", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(crm))
                {
                    lblStatus.Text = "CRM é obrigatório para médicos.";
                    return;
                }

                using (var conn = Banco.Conexao.ObterConexao())
                {
                    string sql = @"INSERT INTO funcionario (usuario, nome, crm, senha, cargo) VALUES (@usuario, @nome, @crm, @senha, @cargo)";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuario", usuario);
                        cmd.Parameters.AddWithValue("@nome", nome);
                        cmd.Parameters.AddWithValue("@crm", (object)crm ?? DBNull.Value);
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
