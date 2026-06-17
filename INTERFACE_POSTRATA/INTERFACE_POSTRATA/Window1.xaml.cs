using System.Windows;

namespace INTERFACE_POSTRATA
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

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

        private void GerarLaudo_Click(object sender, RoutedEventArgs e)
        {
            CadastroExame tela = new CadastroExame();
            tela.Show();
            this.Close();
        }

        private void ListarPacientes_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tela ainda não criada.");
        }

        private void ListarLaudos_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tela ainda não criada.");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void Sair_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}