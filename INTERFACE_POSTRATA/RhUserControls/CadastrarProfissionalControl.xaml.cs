using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA.RhUserControls
{
    public partial class CadastrarProfissionalControl : UserControl
    {
        public CadastrarProfissionalControl()
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

            spCRM.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;

            if (!mostrar) txtCRM.Clear();
        }

        private void Salvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string rm = txtRM.Text?.Trim();
                string nome = txtNome.Text?.Trim();
                string senha = txtSenha.Password?.Trim();
                string cargo = ((ComboBoxItem)cbCargo.SelectedItem)?.Content?.ToString();
                string crm = string.Equals(cargo, "MEDICO", StringComparison.OrdinalIgnoreCase)
                    ? (txtCRM.Text?.Trim() ?? string.Empty)
                    : null;

                if (string.IsNullOrEmpty(rm) || string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(senha) || string.IsNullOrEmpty(cargo))
                {
                    lblStatus.Foreground = System.Windows.Media.Brushes.Red;
                    lblStatus.Text = "Todos os campos são obrigatórios.";
                    return;
                }

                if (string.Equals(cargo, "MEDICO", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(crm))
                {
                    lblStatus.Foreground = System.Windows.Media.Brushes.Red;
                    lblStatus.Text = "CRM é obrigatório para médicos.";
                    return;
                }

                using (var conn = Banco.Conexao.ObterConexao())
                {
                    string sql = @"INSERT INTO funcionario (rm, nome, crm, senha, cargo) VALUES (@rm, @nome, @crm, @senha, @cargo)";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@rm", rm);
                        cmd.Parameters.AddWithValue("@nome", nome);
                        cmd.Parameters.AddWithValue("@crm", (object)crm ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@senha", senha);
                        cmd.Parameters.AddWithValue("@cargo", cargo);
                        cmd.ExecuteNonQuery();
                    }
                }

                lblStatus.Foreground = System.Windows.Media.Brushes.Green;
                lblStatus.Text = "Profissional cadastrado com sucesso.";
                LimparCampos();
                MessageBox.Show("Profissional cadastrado com sucesso.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Foreground = System.Windows.Media.Brushes.Red;
                lblStatus.Text = "Erro: " + ex.Message;
            }
        }

        private void Limpar_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
        }

        private void LimparCampos()
        {
            txtRM.Clear();
            txtNome.Clear();
            txtCRM.Clear();
            txtSenha.Clear();
            cbCargo.SelectedIndex = -1;
            lblStatus.Text = string.Empty;
            AtualizarVisibilidadeCRM();
        }
    }
}
