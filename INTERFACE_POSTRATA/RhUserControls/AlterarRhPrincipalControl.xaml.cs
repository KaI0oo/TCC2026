using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA.RhUserControls
{
    public partial class AlterarRhPrincipalControl : UserControl
    {
        public AlterarRhPrincipalControl()
        {
            InitializeComponent();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            txtTargetRM.Text = string.Empty;
            txtTargetNome.Text = string.Empty;
            txtCurrentRHPassword.Password = string.Empty;
        }

        private void Confirmar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTargetRM.Text) || !int.TryParse(txtTargetRM.Text.Trim(), out int targetRm))
                {
                    MessageBox.Show("RM inválido.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCurrentRHPassword.Password))
                {
                    MessageBox.Show("Informe a senha do RH atual.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!Helpers.Session.CurrentFuncionarioId.HasValue)
                {
                    MessageBox.Show("Usuário atual não identificado na sessão.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int currentRm = Helpers.Session.CurrentFuncionarioId.Value;

                using (var conn = Banco.Conexao.ObterConexao())
                {
                    using (var cmd = new MySqlCommand("SELECT senha, cargo FROM funcionario WHERE rm = @rm", conn))
                    {
                        cmd.Parameters.AddWithValue("@rm", currentRm);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("Usuário atual não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            string senhaAtual = reader["senha"]?.ToString() ?? string.Empty;
                            string cargoAtual = reader["cargo"]?.ToString() ?? string.Empty;
                            if (!cargoAtual.Equals("RH", StringComparison.OrdinalIgnoreCase))
                            {
                                MessageBox.Show("A operação requer que o usuário atual seja RH principal.", "Permissão", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                            if (!senhaAtual.Equals(txtCurrentRHPassword.Password))
                            {
                                MessageBox.Show("Senha do RH atual incorreta.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                    }

                    using (var cmd2 = new MySqlCommand("SELECT rm, cargo FROM funcionario WHERE rm = @rm", conn))
                    {
                        cmd2.Parameters.AddWithValue("@rm", targetRm);
                        using (var r2 = cmd2.ExecuteReader())
                        {
                            if (!r2.Read())
                            {
                                MessageBox.Show("Funcionário informado não existe.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                    }

                    var confirm = MessageBox.Show($"Confirma alterar o RH principal atual (RM {currentRm}) para MÉDICO e definir RM {targetRm} como novo RH?\nApenas um RH Principal poderá existir.", "Confirmar alteração", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (confirm != MessageBoxResult.Yes) return;

                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            using (var u1 = new MySqlCommand("UPDATE funcionario SET cargo = @cargo WHERE rm = @rm", conn, tran))
                            {
                                u1.Parameters.AddWithValue("@cargo", "MEDICO");
                                u1.Parameters.AddWithValue("@rm", currentRm);
                                u1.ExecuteNonQuery();
                            }
                            using (var u2 = new MySqlCommand("UPDATE funcionario SET cargo = @cargo WHERE rm = @rm", conn, tran))
                            {
                                u2.Parameters.AddWithValue("@cargo", "RH");
                                u2.Parameters.AddWithValue("@rm", targetRm);
                                u2.ExecuteNonQuery();
                            }
                            tran.Commit();
                            MessageBox.Show("RH principal alterado com sucesso.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                            Cancelar_Click(null, null);
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            MessageBox.Show("Erro ao atualizar cargos: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
