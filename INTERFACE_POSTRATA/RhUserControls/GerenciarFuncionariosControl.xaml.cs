using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA.RhUserControls
{
    public partial class GerenciarFuncionariosControl : UserControl
    {
        private int? _selectedRm = null;
        private bool _changingPassword = false;

        public GerenciarFuncionariosControl()
        {
            InitializeComponent();
            LoadUsers();
            AtualizarVisibilidadeCRM();
        }

        private void LoadUsers(string filtro = null)
        {
            try
            {
                var lista = new List<dynamic>();
                using (var conn = Banco.Conexao.ObterConexao())
                {
                    string sql = "SELECT rm, nome, crm, cargo FROM funcionario";
                    if (!string.IsNullOrWhiteSpace(filtro))
                        sql += " WHERE rm LIKE @q OR nome LIKE @q";
                    sql += " ORDER BY rm";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        if (!string.IsNullOrWhiteSpace(filtro))
                            cmd.Parameters.AddWithValue("@q", "%" + filtro + "%");

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new
                                {
                                    RM = reader["rm"] != DBNull.Value ? Convert.ToInt32(reader["rm"]) : 0,
                                    Nome = reader["nome"]?.ToString() ?? string.Empty,
                                    CRM = reader["crm"] != DBNull.Value ? (reader["crm"]?.ToString() ?? string.Empty) : string.Empty,
                                    Cargo = reader["cargo"]?.ToString() ?? string.Empty
                                });
                            }
                        }
                    }
                }

                dgUsers.ItemsSource = lista;
                ClearDetails();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar usuários: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Pesquisar_Click(object sender, RoutedEventArgs e)
            => LoadUsers(txtSearch.Text?.Trim());

        private void LimparPesquisa_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            LoadUsers();
        }

        private void DgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = dgUsers.SelectedItem;
            if (item == null) { ClearDetails(); return; }

            var rmProp = item.GetType().GetProperty("RM");
            var nomeProp = item.GetType().GetProperty("Nome");
            var crmProp = item.GetType().GetProperty("CRM");
            var cargoProp = item.GetType().GetProperty("Cargo");
            if (rmProp == null) return;

            int rm = (int)rmProp.GetValue(item);
            _selectedRm = rm;
            txtRM.Text = rm.ToString();
            txtNome.Text = nomeProp?.GetValue(item)?.ToString() ?? string.Empty;
            txtCRM.Text = crmProp?.GetValue(item)?.ToString() ?? string.Empty;
            var cargoVal = cargoProp?.GetValue(item)?.ToString() ?? string.Empty;
            cbCargo.SelectedIndex = -1;
            foreach (ComboBoxItem it in cbCargo.Items)
            {
                if (string.Equals(it.Content.ToString(), cargoVal, StringComparison.OrdinalIgnoreCase))
                {
                    cbCargo.SelectedItem = it;
                    break;
                }
            }

            SetPanelEnabled(true);
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

        private void CancelarEdicao_Click(object sender, RoutedEventArgs e)
            => ClearDetails();

        private void SetPanelEnabled(bool enabled)
        {
            txtRM.IsEnabled = false;
            txtNome.IsEnabled = enabled;
            txtCRM.IsEnabled = enabled;
            cbCargo.IsEnabled = enabled;
            btnSalvar.IsEnabled = enabled;
            btnAlterarSenha.IsEnabled = enabled;
            btnExcluir.IsEnabled = enabled;
        }

        private void ClearDetails()
        {
            _selectedRm = null;
            dgUsers.SelectedItem = null;
            txtRM.Text = string.Empty;
            txtNome.Text = string.Empty;
            txtCRM.Text = string.Empty;
            cbCargo.SelectedIndex = -1;
            SetPanelEnabled(false);
            if (_changingPassword) CancelarNovaSenha_Click(null, null);
            AtualizarVisibilidadeCRM();
        }

        private bool ValidateFields()
        {
            if (!_selectedRm.HasValue) { MessageBox.Show("Selecione um funcionário para salvar.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning); return false; }
            if (string.IsNullOrWhiteSpace(txtNome.Text)) { MessageBox.Show("Nome é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning); return false; }
            if (cbCargo.SelectedItem == null) { MessageBox.Show("Cargo é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning); return false; }

            string cargo = ((ComboBoxItem)cbCargo.SelectedItem).Content?.ToString();
            if (string.Equals(cargo, "MEDICO", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(txtCRM.Text))
            {
                MessageBox.Show("CRM é obrigatório para médicos.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void SalvarUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedRm.HasValue) { MessageBox.Show("Selecione um funcionário para salvar.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!ValidateFields()) return;

            try
            {
                using (var conn = Banco.Conexao.ObterConexao())
                {
                    int rm = _selectedRm.Value;
                    string novoCargo = ((ComboBoxItem)cbCargo.SelectedItem).Content.ToString();
                    object novoCrm = string.Equals(novoCargo, "MEDICO", StringComparison.OrdinalIgnoreCase)
                        ? (object)(txtCRM.Text?.Trim() ?? string.Empty)
                        : DBNull.Value;

                    using (var chkRh = new MySqlCommand("SELECT COUNT(1) FROM funcionario WHERE UPPER(cargo) = 'RH'", conn))
                    {
                        var totalRh = Convert.ToInt32(chkRh.ExecuteScalar() ?? 0);
                        string cargoAtual = null;
                        using (var q = new MySqlCommand("SELECT cargo FROM funcionario WHERE rm = @rm", conn))
                        {
                            q.Parameters.AddWithValue("@rm", rm);
                            cargoAtual = q.ExecuteScalar()?.ToString() ?? string.Empty;
                        }
                        if (string.Equals(cargoAtual, "RH", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(novoCargo, "RH", StringComparison.OrdinalIgnoreCase)
                            && totalRh <= 1)
                        {
                            MessageBox.Show("Não é permitido alterar o cargo do último usuário RH.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    string sql = "UPDATE funcionario SET nome=@nome, crm=@crm, cargo=@cargo WHERE rm=@rm";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@nome", txtNome.Text.Trim());
                        cmd.Parameters.AddWithValue("@crm", novoCrm);
                        cmd.Parameters.AddWithValue("@cargo", novoCargo);
                        cmd.Parameters.AddWithValue("@rm", rm);
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadUsers();
                MessageBox.Show("Operação realizada com sucesso.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar usuário: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AlterarSenha_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedRm.HasValue) { MessageBox.Show("Selecione um funcionário para alterar a senha.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            var confirm = MessageBox.Show("Deseja realmente alterar a senha deste funcionário?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            spChangePwd.Visibility = Visibility.Visible;
            _changingPassword = true;
        }

        private void CancelarNovaSenha_Click(object sender, RoutedEventArgs e)
        {
            txtNewSenha.Password = string.Empty;
            txtConfirmSenha.Password = string.Empty;
            spChangePwd.Visibility = Visibility.Collapsed;
            _changingPassword = false;
        }

        private void SalvarNovaSenha_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedRm.HasValue) { MessageBox.Show("Nenhum funcionário selecionado.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            string n1 = txtNewSenha.Password?.Trim() ?? string.Empty;
            string n2 = txtConfirmSenha.Password?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(n1) || string.IsNullOrWhiteSpace(n2)) { MessageBox.Show("Preencha ambos os campos de senha.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (n1 != n2) { MessageBox.Show("As senhas não conferem.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            try
            {
                using (var conn = Banco.Conexao.ObterConexao())
                using (var cmd = new MySqlCommand("UPDATE funcionario SET senha = @senha WHERE rm = @rm", conn))
                {
                    cmd.Parameters.AddWithValue("@senha", n1);
                    cmd.Parameters.AddWithValue("@rm", _selectedRm.Value);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Senha alterada com sucesso.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                CancelarNovaSenha_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao alterar senha: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExcluirUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedRm.HasValue) { MessageBox.Show("Selecione um usuário para excluir.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            int rm = _selectedRm.Value;
            var result = MessageBox.Show($"Deseja realmente excluir o usuário com RM {rm}?", "Confirmar exclusão", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var conn = Banco.Conexao.ObterConexao())
                {
                    string cargoAtual;
                    using (var q = new MySqlCommand("SELECT cargo FROM funcionario WHERE rm = @rm", conn))
                    {
                        q.Parameters.AddWithValue("@rm", rm);
                        cargoAtual = q.ExecuteScalar()?.ToString() ?? string.Empty;
                    }

                    if (string.Equals(cargoAtual, "RH", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var chk = new MySqlCommand("SELECT COUNT(1) FROM funcionario WHERE UPPER(cargo) = 'RH'", conn))
                        {
                            var totalRh = Convert.ToInt32(chk.ExecuteScalar() ?? 0);
                            if (totalRh <= 1)
                            {
                                MessageBox.Show("Não é permitido excluir o último usuário com cargo RH.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                    }

                    using (var del = new MySqlCommand("DELETE FROM funcionario WHERE rm = @rm", conn))
                    {
                        del.Parameters.AddWithValue("@rm", rm);
                        del.ExecuteNonQuery();
                    }
                }

                LoadUsers();
                MessageBox.Show("Usuário excluído.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir usuário: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
