using System.Windows;
using INTERFACE_POSTRATA.Banco;
using MySql.Data.MySqlClient;
namespace INTERFACE_POSTRATA
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            try
            {
                MySqlConnection conn = Conexao.ObterConexao();

                //MessageBox.Show("Banco conectado!");

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            // Atualizar cabeçalho com informações da sessão, quando disponível
            try
            {
                var nome = INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoName ?? string.Empty;
                var crm = INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoCrm ?? string.Empty;
                var cargo = INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoCargo ?? string.Empty;

                string prefixo = "";
                if (cargo.Equals("MEDICO", StringComparison.OrdinalIgnoreCase) || cargo.Equals("Medico", StringComparison.OrdinalIgnoreCase))
                {
                    // tentativa simples de inferir gênero pelo nome (se terminar com 'a' usar Dra.)
                    prefixo = (nome.EndsWith("a", StringComparison.OrdinalIgnoreCase)) ? "Dra." : "Dr.";
                }
                else if (cargo.Equals("RH", StringComparison.OrdinalIgnoreCase))
                {
                    prefixo = "RH";
                }
                else if (cargo.Equals("SECRETARIA", StringComparison.OrdinalIgnoreCase) || cargo.Equals("Secretaria", StringComparison.OrdinalIgnoreCase))
                {
                    prefixo = "Secretária";
                }

                if (!string.IsNullOrWhiteSpace(nome))
                {
                    var tbName = this.FindName("txtHeaderName") as System.Windows.Controls.TextBlock;
                    var tbCrm = this.FindName("txtHeaderCRM") as System.Windows.Controls.TextBlock;
                    var tbWelcome = this.FindName("txtWelcome") as System.Windows.Controls.TextBlock;
                    if (tbName != null)
                        tbName.Text = string.IsNullOrWhiteSpace(prefixo) ? nome : prefixo + " " + nome;
                    if (tbCrm != null)
                        tbCrm.Text = string.IsNullOrWhiteSpace(crm) ? string.Empty : "CRM " + crm;
                    if (tbWelcome != null)
                        tbWelcome.Text = "Bem-vindo, " + (string.IsNullOrWhiteSpace(prefixo) ? nome : prefixo + " " + nome);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Window1.Session] {ex}");
            }

            // configurar visibilidade do menu conforme cargo
            ConfigureMenuByRole();
        }

        private void ConfigureMenuByRole()
        {
            try
            {
                var cargo = INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoCargo ?? string.Empty;
                bool isMedico = cargo.Equals("MEDICO", System.StringComparison.OrdinalIgnoreCase);
                bool isSecretaria = cargo.Equals("SECRETARIA", System.StringComparison.OrdinalIgnoreCase);

                // Mostrar/ocultar grupos
                var cadastro = this.FindName("tvCadastroGroup") as System.Windows.Controls.TreeViewItem;
                var ia = this.FindName("tvIAGroup") as System.Windows.Controls.TreeViewItem;
                var consultas = this.FindName("tvConsultasGroup") as System.Windows.Controls.TreeViewItem;
                var edicao = this.FindName("tvEdicaoGroup") as System.Windows.Controls.TreeViewItem;
                var exclusao = this.FindName("tvExclusaoGroup") as System.Windows.Controls.TreeViewItem;
                var conta = this.FindName("tvContaGroup") as System.Windows.Controls.TreeViewItem;
                var inicio = this.FindName("tvInicioGroup") as System.Windows.Controls.TreeViewItem;

                if (isMedico)
                {
                    // médico vê tudo
                    if (cadastro != null) cadastro.Visibility = System.Windows.Visibility.Visible;
                    if (ia != null) ia.Visibility = System.Windows.Visibility.Visible;
                    if (consultas != null) consultas.Visibility = System.Windows.Visibility.Visible;
                    if (edicao != null) edicao.Visibility = System.Windows.Visibility.Visible;
                    if (exclusao != null) exclusao.Visibility = System.Windows.Visibility.Visible;
                    if (conta != null) conta.Visibility = System.Windows.Visibility.Visible;
                    if (inicio != null) inicio.Visibility = System.Windows.Visibility.Visible;
                }
                else if (isSecretaria)
                {
                    // secretária: manter apenas Início, Cadastro->Cadastrar Paciente e Conta
                    if (cadastro != null)
                    {
                        // esconder todos itens, exceto o primeiro (Cadastrar Paciente)
                        foreach (var item in cadastro.Items)
                        {
                            if (item is System.Windows.Controls.TreeViewItem t)
                            {
                                t.Visibility = System.Windows.Visibility.Collapsed;
                            }
                        }
                        // exibir somente primeiro
                        if (cadastro.Items.Count > 0 && cadastro.Items[0] is System.Windows.Controls.TreeViewItem first)
                            first.Visibility = System.Windows.Visibility.Visible;
                    }
                    if (ia != null) ia.Visibility = System.Windows.Visibility.Collapsed;
                    if (consultas != null) consultationsHide(consultas);
                    if (edicao != null) edicao.Visibility = System.Windows.Visibility.Collapsed;
                    if (exclusao != null) exclusao.Visibility = System.Windows.Visibility.Collapsed;
                    if (conta != null) conta.Visibility = System.Windows.Visibility.Visible;
                    if (inicio != null) inicio.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    // padrão: mostrar tudo
                    if (cadastro != null) cadastro.Visibility = System.Windows.Visibility.Visible;
                    if (ia != null) ia.Visibility = System.Windows.Visibility.Visible;
                    if (consultas != null) consultationsShow(consultas);
                    if (edicao != null) edicao.Visibility = System.Windows.Visibility.Visible;
                    if (exclusao != null) exclusao.Visibility = System.Windows.Visibility.Visible;
                    if (conta != null) conta.Visibility = System.Windows.Visibility.Visible;
                    if (inicio != null) inicio.Visibility = System.Windows.Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Window1.ConfigureMenuByRole] {ex}");
            }
        }

        private void consultationsHide(System.Windows.Controls.TreeViewItem consultas)
        {
            // hide all children
            foreach (var it in consultas.Items)
            {
                if (it is System.Windows.Controls.TreeViewItem t) t.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void consultationsShow(System.Windows.Controls.TreeViewItem consultas)
        {
            foreach (var it in consultas.Items)
            {
                if (it is System.Windows.Controls.TreeViewItem t) t.Visibility = System.Windows.Visibility.Visible;
            }
        }

        // CADASTRO

        private void CadastrarPaciente_Click(object sender, RoutedEventArgs e)
        {
            new CadastroPaciente().Show();
        }

        private void CadastrarAnamnese_Click(object sender, RoutedEventArgs e)
        {
            new CadastroAnamnese().Show();
        }

        private void CadastrarExame_Click(object sender, RoutedEventArgs e)
        {
            new CadastroExame().Show();
        }

        // LAUDOS

        private void GerarLaudo_Click(object sender, RoutedEventArgs e)
        {
            CadastroExame tela = new CadastroExame();
            tela.Show();
            this.Close();
        }

        // CONSULTAS - PACIENTES

        private void BuscarPaciente_Click(object sender, RoutedEventArgs e)
        {
            // Abrir diálogo para digitar CPF e mostrar apenas esse paciente
            var input = new InputDialog();
            input.Owner = this;
            input.Prompt = "Digite o CPF do paciente:";
            if (input.ShowDialog() == true && !string.IsNullOrEmpty(input.Valor))
            {
                var ctrl = new ListarPacientesControl();
                ctrl.CarregarPacientes(input.Valor);
                SetMainContent(ctrl);
            }
        }

        private void BuscarTodosPacientes_Click(object sender, RoutedEventArgs e)
        {
            SetMainContent(new ListarPacientesControl());
        }

        // CONSULTAS - EXAMES

        private void BuscarExamePaciente_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog();
            input.Owner = this;
            input.Prompt = "Informe o CPF do paciente para buscar exames:";
            if (input.ShowDialog() == true && !string.IsNullOrEmpty(input.Valor))
            {
                var ctrl = new ListarExamesControl();
                ctrl.CarregarExames(input.Valor);
                SetMainContent(ctrl);
            }
        }

        private void BuscarTodosExames_Click(object sender, RoutedEventArgs e)
        {
            SetMainContent(new ListarExamesControl());
        }

        private void BuscarTodosExamesPaciente_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog();
            input.Owner = this;
            input.Prompt = "Informe o CPF do paciente (exames):";
            if (input.ShowDialog() == true && !string.IsNullOrEmpty(input.Valor))
            {
                var ctrl = new ListarExamesControl();
                ctrl.CarregarExames(input.Valor);
                SetMainContent(ctrl);
            }
        }

        private void ListarExames_Click(object sender, RoutedEventArgs e)
        {
            SetMainContent(new ListarExamesControl());
        }
        private void TreinarIA_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tela de treinamento da IA ainda não criada.");
        }
        // CONSULTAS - LAUDOS

        private void BuscarLaudoPaciente_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog();
            input.Owner = this;
            input.Prompt = "Informe o CPF do paciente para buscar laudos:";
            if (input.ShowDialog() == true && !string.IsNullOrEmpty(input.Valor))
            {
                var ctrl = new ListarLaudosControl();
                ctrl.CarregarLaudos(input.Valor);
                SetMainContent(ctrl);
            }
        }

        private void BuscarTodosLaudosPaciente_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog();
            input.Owner = this;
            input.Prompt = "Informe o CPF do paciente (laudos):";
            if (input.ShowDialog() == true && !string.IsNullOrEmpty(input.Valor))
            {
                var ctrl = new ListarLaudosControl();
                ctrl.CarregarLaudos(input.Valor);
                SetMainContent(ctrl);
            }
        }

        private void BuscarTodosLaudos_Click(object sender, RoutedEventArgs e)
        {
            SetMainContent(new ListarLaudosControl());
        }

        // CONTA

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Abrir a tela de Login (MainWindow) e fechar apenas esta janela
            var login = new MainWindow();
            login.Show();
            this.Close();
        }

        // EDIÇÃO
        private void EditarPaciente_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog();
            input.Owner = this;
            input.Prompt = "Informe o CPF do paciente a editar:";
            if (input.ShowDialog() == true && !string.IsNullOrEmpty(input.Valor))
            {
                var win = new CadastroPaciente(input.Valor);
                win.Show();
            }
        }

        private void EditarAnamnese_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog();
            input.Owner = this;
            input.Prompt = "Informe o CPF do paciente para carregar anamneses:";
            if (input.ShowDialog() == true && !string.IsNullOrEmpty(input.Valor))
            {
                var ctrl = new ListarAnamnesesControl();
                ctrl.CarregarAnamneses(input.Valor);
                SetMainContent(ctrl);
            }
        }

        private void EditarExame_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog();
            input.Owner = this;
            input.Prompt = "Informe o CPF do paciente para carregar exames:";
            if (input.ShowDialog() == true && !string.IsNullOrEmpty(input.Valor))
            {
                var ctrl = new ListarExamesControl();
                ctrl.CarregarExames(input.Valor);
                SetMainContent(ctrl);
            }
        }

        // EXCLUSÃO
        private void ExcluirPaciente_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog();
            input.Owner = this;
            input.Prompt = "Informe o CPF do paciente a excluir:";
            if (input.ShowDialog() != true || string.IsNullOrWhiteSpace(input.Valor)) return;
            string cpf = input.Valor.Trim();

            // 1ª confirmação
            var confirm = MessageBox.Show(
                $"Confirma exclusão do paciente {cpf}?",
                "Confirmar exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            // 2ª confirmação — destino dos registros relacionados
            var cascade = MessageBox.Show(
                "Como tratar os registros relacionados (anamneses, exames, laudos)?\n\n" +
                "• Sim — exclui tudo em cascata.\n" +
                "• Não — mantém os registros como histórico (cpf_paciente = NULL).\n" +
                "• Cancelar — aborta a exclusão do paciente.",
                "Registros relacionados",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            if (cascade == MessageBoxResult.Cancel) return;

            bool deleteCascade = cascade == MessageBoxResult.Yes;

            try
            {
                using (var conn = Conexao.ObterConexao())
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        if (deleteCascade)
                        {
                            // remover laudos relacionados
                            new MySqlCommand(
                                "DELETE l FROM laudo l JOIN exame e ON l.id_exame = e.id_exame WHERE e.cpf_paciente = @cpf",
                                conn, tran)
                                .WithParam("@cpf", cpf)
                                .ExecuteNonQuery();

                            // remover exames
                            new MySqlCommand(
                                "DELETE FROM exame WHERE cpf_paciente = @cpf",
                                conn, tran)
                                .WithParam("@cpf", cpf)
                                .ExecuteNonQuery();

                            // remover anamneses
                            new MySqlCommand(
                                "DELETE FROM anamnese WHERE cpf_paciente = @cpf",
                                conn, tran)
                                .WithParam("@cpf", cpf)
                                .ExecuteNonQuery();
                        }
                        else
                        {
                            // preservar como histórico: desvincula cpf_paciente
                            // (requer que a coluna aceite NULL; em caso de constraint NOT NULL
                            //  a operação falha, o rollback é disparado e a mensagem abaixo é exibida)
                            new MySqlCommand(
                                "UPDATE anamnese SET cpf_paciente = NULL WHERE cpf_paciente = @cpf",
                                conn, tran)
                                .WithParam("@cpf", cpf)
                                .ExecuteNonQuery();

                            new MySqlCommand(
                                "UPDATE exame SET cpf_paciente = NULL WHERE cpf_paciente = @cpf",
                                conn, tran)
                                .WithParam("@cpf", cpf)
                                .ExecuteNonQuery();

                            // laudos são vinculados por id_exame e mantidos como histórico
                        }

                        // deletar paciente
                        int affected = new MySqlCommand(
                            "DELETE FROM paciente WHERE cpf = @cpf",
                            conn, tran)
                            .WithParam("@cpf", cpf)
                            .ExecuteNonQuery();

                        if (affected == 0)
                        {
                            tran.Rollback();
                            MessageBox.Show(
                                $"Paciente {cpf} não encontrado. Nada foi removido.",
                                "Aviso",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            return;
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }

                MessageBox.Show(
                    deleteCascade
                        ? "Paciente e registros relacionados excluídos."
                        : "Paciente excluído. Registros relacionados foram preservados como histórico.",
                    "OK",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao excluir paciente: " + ex.Message +
                        "\n\nSe a opção escolhida foi 'preservar histórico', talvez a coluna cpf_paciente não permita NULL. " +
                        "Nesse caso, utilize a opção 'excluir tudo em cascata'.",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExcluirAnamnese_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog();
            input.Owner = this;
            input.Prompt = "Informe o ID da anamnese a excluir:";
            if (input.ShowDialog() == true && !string.IsNullOrEmpty(input.Valor))
            {
                if (!int.TryParse(input.Valor, out int id)) { MessageBox.Show("ID inválido."); return; }
                var confirm = MessageBox.Show($"Confirma exclusão da anamnese {id}?", "Confirmar exclusão", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) return;
                try
                {
                    using (var conn = Conexao.ObterConexao())
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            int affected;
                            using (var cmd = new MySqlCommand("DELETE FROM anamnese WHERE id_anamnese = @id", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                affected = cmd.ExecuteNonQuery();
                            }

                            if (affected == 0)
                            {
                                tran.Rollback();
                                MessageBox.Show($"Anamnese {id} não encontrada. Nada foi removido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }

                            tran.Commit();
                            MessageBox.Show("Anamnese excluída.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao excluir anamnese: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExcluirExame_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog();
            input.Owner = this;
            input.Prompt = "Informe o ID do exame a excluir:";
            if (input.ShowDialog() == true && !string.IsNullOrEmpty(input.Valor))
            {
                if (!int.TryParse(input.Valor, out int id)) { MessageBox.Show("ID inválido."); return; }
                var confirm = MessageBox.Show($"Confirma exclusão do exame {id}? O laudo associado também será removido.", "Confirmar exclusão", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) return;
                try
                {
                    using (var conn = Conexao.ObterConexao())
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            // deletar laudos relacionados
                            using (var cmd = new MySqlCommand("DELETE FROM laudo WHERE id_exame = @id", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                cmd.ExecuteNonQuery();
                            }
                            // deletar exame
                            int affected;
                            using (var cmd2 = new MySqlCommand("DELETE FROM exame WHERE id_exame = @id", conn, tran))
                            {
                                cmd2.Parameters.AddWithValue("@id", id);
                                affected = cmd2.ExecuteNonQuery();
                            }

                            if (affected == 0)
                            {
                                tran.Rollback();
                                MessageBox.Show($"Exame {id} não encontrado. Nada foi removido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }

                            tran.Commit();
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                    MessageBox.Show("Exame e laudo associados excluídos.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao excluir exame: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExcluirLaudo_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputDialog();
            input.Owner = this;
            input.Prompt = "Informe o ID do laudo a excluir:";
            if (input.ShowDialog() == true && !string.IsNullOrEmpty(input.Valor))
            {
                if (!int.TryParse(input.Valor, out int id)) { MessageBox.Show("ID inválido."); return; }
                var confirm = MessageBox.Show($"Confirma exclusão do laudo {id}?", "Confirmar exclusão", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) return;
                try
                {
                    using (var conn = Conexao.ObterConexao())
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            int affected;
                            using (var cmd = new MySqlCommand("DELETE FROM laudo WHERE id_laudo = @id", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                affected = cmd.ExecuteNonQuery();
                            }

                            if (affected == 0)
                            {
                                tran.Rollback();
                                MessageBox.Show($"Laudo {id} não encontrado. Nada foi removido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }

                            tran.Commit();
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                    MessageBox.Show("Laudo excluído.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao excluir laudo: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Sair_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Métodos públicos para manipular o conteúdo do painel principal
        public void SetMainContent(System.Windows.UIElement element)
        {
            var contentControl = this.FindName("MainContent") as System.Windows.Controls.ContentControl;
            if (contentControl != null)
            {
                contentControl.Content = element;
            }
        }

        public void ClearMainContent()
        {
            var contentControl = this.FindName("MainContent") as System.Windows.Controls.ContentControl;
            if (contentControl != null)
            {
                contentControl.Content = null;
            }
        }

        // NOVOS HANDLERS: exibir conteúdo no painel principal
        private void ConsultarTudo_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar pacientes e exames lado a lado
            var grid = new System.Windows.Controls.Grid();
            grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });

            var pacientesCtrl = new ListarPacientesControl();
            System.Windows.Controls.Grid.SetColumn(pacientesCtrl, 0);
            grid.Children.Add(pacientesCtrl);

            var examesCtrl = new ListarExamesControl();
            System.Windows.Controls.Grid.SetColumn(examesCtrl, 1);
            grid.Children.Add(examesCtrl);

            SetMainContent(grid);
        }

        private void ConsultarPaciente_Click(object sender, RoutedEventArgs e)
        {
            SetMainContent(new ListarPacientesControl());
        }

        private void ConsultarExames_Click(object sender, RoutedEventArgs e)
        {
            SetMainContent(new ListarExamesControl());
        }

        private void ConsultarLaudos_Click(object sender, RoutedEventArgs e)
        {
            SetMainContent(new System.Windows.Controls.TextBlock { Text = "Funcionalidade de laudos ainda não implementada.", FontSize = 20, Margin = new System.Windows.Thickness(20) });
        }

        private void Inicio_Click(object sender, RoutedEventArgs e)
        {
            RestoreMainPanel();
        }

        // Torna acessível a restauração do painel principal para outros controles
        public void RestoreMainPanel()
        {
            var label = new System.Windows.Controls.Label
            {
                Content = "Painel Principal",
                FontSize = 32,
                FontWeight = System.Windows.FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0x60, 0x64)),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                Margin = new System.Windows.Thickness(0,25,0,0)
            };

            var stack = new System.Windows.Controls.StackPanel { Margin = new System.Windows.Thickness(30,100,30,30) };
            stack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "Bem-vindo ao sistema da clínica.",
                FontSize = 20,
                FontWeight = System.Windows.FontWeights.SemiBold
            });

            stack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "Selecione uma opção no menu ao lado para iniciar.",
                FontSize = 20,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new System.Windows.Thickness(0,15,0,0)
            });

            var container = new System.Windows.Controls.Grid();
            container.Children.Add(label);
            container.Children.Add(stack);

            SetMainContent(container);
        }











        // Pequeno helper para encadear AddWithValue sem repetir linhas
    }

    internal static class CmdExtensions
    {
        public static MySqlCommand WithParam(this MySqlCommand cmd, string name, object value)
        {
            cmd.Parameters.AddWithValue(name, value);
            return cmd;
        }
    }
}