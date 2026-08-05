using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using INTERFACE_POSTRATA.Banco;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA
{
    public partial class GerenciarContaRH : Window
    {
        private int? _selectedRm = null;
        private bool _changingPassword = false;

        public GerenciarContaRH()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers(string filtro = null)
        {
            try
            {
                var lista = new List<dynamic>();
                using (var conn = Conexao.ObterConexao())
                {
                    string sql = "SELECT rm, nome, crm, cargo FROM medico";
                    if (!string.IsNullOrWhiteSpace(filtro))
                    {
                        sql += " WHERE rm LIKE @q OR nome LIKE @q";
                    }
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
                                    CRM = reader["crm"]?.ToString() ?? string.Empty,
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
        {
            LoadUsers(txtSearch.Text?.Trim());
        }

        private void LimparPesquisa_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            LoadUsers();
        }

        private void DgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = dgUsers.SelectedItem;
            if (item == null)
            {
                ClearDetails();
                return;
            }

            // usar reflection dinâmica
            var rmProp = item.GetType().GetProperty("RM");
            var nomeProp = item.GetType().GetProperty("Nome");
            var crmProp = item.GetType().GetProperty("CRM");
            var cargoProp = item.GetType().GetProperty("Cargo");

            if (rmProp == null) return;

            int rm = (int)rmProp.GetValue(item);
            _selectedRm = rm;
            txtRM.Text = rm.ToString();
            txtRM.IsEnabled = false;
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
            // não carregar senha
            // habilitar botão Alterar Senha
            try { btnAlterarSenha.IsEnabled = true; } catch { }
        }

        private void NovoUsuario_Click(object sender, RoutedEventArgs e)
        {
            _selectedRm = null;
            txtRM.IsEnabled = true;
            txtRM.Text = string.Empty;
            txtNome.Text = string.Empty;
            txtCRM.Text = string.Empty;
            cbCargo.SelectedIndex = -1;
            dgUsers.SelectedItem = null;
            try { btnAlterarSenha.IsEnabled = false; } catch { }
        }

        private void CancelarEdicao_Click(object sender, RoutedEventArgs e)
        {
            ClearDetails();
        }

        private void ClearDetails()
        {
            _selectedRm = null;
            txtRM.IsEnabled = true;
            txtRM.Text = string.Empty;
            txtNome.Text = string.Empty;
            txtCRM.Text = string.Empty;
            cbCargo.SelectedIndex = -1;
            try { btnAlterarSenha.IsEnabled = false; } catch { }
        }

        private bool ValidateFields(bool isNew)
        {
            if (isNew)
            {
                if (string.IsNullOrWhiteSpace(txtRM.Text) || !int.TryParse(txtRM.Text.Trim(), out int v) || v <= 0)
                {
                    MessageBox.Show("RM inválido.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Nome é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cbCargo.SelectedItem == null)
            {
                MessageBox.Show("Cargo é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void SalvarUsuario_Click(object sender, RoutedEventArgs e)
        {
            bool isNew = !_selectedRm.HasValue;
            if (!ValidateFields(isNew)) return;

            try
            {
                using (var conn = Conexao.ObterConexao())
                {
                    if (isNew)
                    {
                        int rm = int.Parse(txtRM.Text.Trim());
                        // verificar existência
                        using (var chk = new MySqlCommand("SELECT COUNT(1) FROM medico WHERE rm = @rm", conn))
                        {
                            chk.Parameters.AddWithValue("@rm", rm);
                            var count = Convert.ToInt32(chk.ExecuteScalar() ?? 0);
                            if (count > 0)
                            {
                                MessageBox.Show("RM já cadastrado.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                        string cargo = ((ComboBoxItem)cbCargo.SelectedItem).Content.ToString();

                        // solicitar senha via prompt modal (senha não deve ficar visível no painel)
                        var prompt = new PasswordPrompt();
                        prompt.Owner = this;
                        if (prompt.ShowDialog() != true || string.IsNullOrWhiteSpace(prompt.Password))
                        {
                            MessageBox.Show("Senha é obrigatória para novo usuário.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        string senha = prompt.Password.Trim();

                        using (var cmd = new MySqlCommand("INSERT INTO medico (rm, nome, crm, senha, cargo) VALUES (@rm,@nome,@crm,@senha,@cargo)", conn))
                        {
                            cmd.Parameters.AddWithValue("@rm", rm);
                            cmd.Parameters.AddWithValue("@nome", txtNome.Text.Trim());
                            cmd.Parameters.AddWithValue("@crm", txtCRM.Text.Trim());
                            cmd.Parameters.AddWithValue("@senha", senha);
                            cmd.Parameters.AddWithValue("@cargo", cargo);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        int rm = _selectedRm.Value;
                        string novoCargo = ((ComboBoxItem)cbCargo.SelectedItem).Content.ToString();

                        // Se estiver mudando cargo de um RH para outro cargo, verificar se existe outro RH
                        using (var chkRh = new MySqlCommand("SELECT COUNT(1) FROM medico WHERE UPPER(cargo) = 'RH'", conn))
                        {
                            var totalRh = Convert.ToInt32(chkRh.ExecuteScalar() ?? 0);

                            // descobrir cargo atual do usuário selecionado
                            string cargoAtual = null;
                            using (var q = new MySqlCommand("SELECT cargo FROM medico WHERE rm = @rm", conn))
                            {
                                q.Parameters.AddWithValue("@rm", rm);
                                cargoAtual = q.ExecuteScalar()?.ToString() ?? string.Empty;
                            }

                            if (string.Equals(cargoAtual, "RH", StringComparison.OrdinalIgnoreCase) && !string.Equals(novoCargo, "RH", StringComparison.OrdinalIgnoreCase) && totalRh <= 1)
                            {
                                MessageBox.Show("Não é permitido alterar o cargo do último usuário RH.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }

                        // montar UPDATE; senha não é alterada aqui (usar Alterar Senha)
                        string sql = "UPDATE medico SET nome=@nome, crm=@crm, cargo=@cargo WHERE rm=@rm";

                        using (var cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@nome", txtNome.Text.Trim());
                            cmd.Parameters.AddWithValue("@crm", txtCRM.Text.Trim());
                            cmd.Parameters.AddWithValue("@cargo", novoCargo);
                            cmd.Parameters.AddWithValue("@rm", rm);
                            cmd.ExecuteNonQuery();
                        }
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
            if (!_selectedRm.HasValue)
            {
                MessageBox.Show("Selecione um funcionário para alterar a senha.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("Deseja realmente alterar a senha deste funcionário?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            // mostrar painel de mudança de senha
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
            if (!_selectedRm.HasValue)
            {
                MessageBox.Show("Nenhum funcionário selecionado.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string n1 = txtNewSenha.Password?.Trim() ?? string.Empty;
            string n2 = txtConfirmSenha.Password?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(n1) || string.IsNullOrWhiteSpace(n2))
            {
                MessageBox.Show("Preencha ambos os campos de senha.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (n1 != n2)
            {
                MessageBox.Show("As senhas não conferem.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = Conexao.ObterConexao())
                using (var cmd = new MySqlCommand("UPDATE medico SET senha = @senha WHERE rm = @rm", conn))
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
            if (!_selectedRm.HasValue)
            {
                MessageBox.Show("Selecione um usuário para excluir.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int rm = _selectedRm.Value;
            var result = MessageBox.Show($"Deseja realmente excluir o usuário com RM {rm}?", "Confirmar exclusão", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var conn = Conexao.ObterConexao())
                {
                    // se for RH, verificar não ser o último
                    string cargoAtual;
                    using (var q = new MySqlCommand("SELECT cargo FROM medico WHERE rm = @rm", conn))
                    {
                        q.Parameters.AddWithValue("@rm", rm);
                        cargoAtual = q.ExecuteScalar()?.ToString() ?? string.Empty;
                    }

                    if (string.Equals(cargoAtual, "RH", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var chk = new MySqlCommand("SELECT COUNT(1) FROM medico WHERE UPPER(cargo) = 'RH'", conn))
                        {
                            var totalRh = Convert.ToInt32(chk.ExecuteScalar() ?? 0);
                            if (totalRh <= 1)
                            {
                                MessageBox.Show("Não é permitido excluir o último usuário com cargo RH.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                    }

                    using (var del = new MySqlCommand("DELETE FROM medico WHERE rm = @rm", conn))
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

        private void Fechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
