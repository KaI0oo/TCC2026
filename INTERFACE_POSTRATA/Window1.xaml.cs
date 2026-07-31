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
            catch { }
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











    }
}