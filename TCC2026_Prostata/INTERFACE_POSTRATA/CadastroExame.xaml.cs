using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
namespace INTERFACE_POSTRATA
{
    public partial class CadastroExame : Window
    {
        public CadastroExame()
        {
            InitializeComponent();
        }

        private void GerarLaudo_Click(object sender, RoutedEventArgs e)
        {
            string paciente = "Paciente Teste";
            string idade = "67";
            string medico = "Dr. Carlos Henrique";
            string crm = "CRM 123456";

            string psaTotal = txtPSATotal.Text;
            string psaLivre = txtPSALivre.Text;
            string densidade = txtDensidade.Text;

            ProcessStartInfo psi = new ProcessStartInfo();

            psi.FileName = @"C:\Users\kaioc\AppData\Local\Programs\Python\Python314\python.exe";


            psi.Arguments = @"C:\Users\kaioc\source\repos\INTERFACE_POSTRATA\executar_ia.py " +idade + " " +psaTotal + " " +psaLivre + " " +densidade;
            psi.FileName =
@"C:\Users\kaioc\AppData\Local\Programs\Python\Python314\python.exe";

            psi.Arguments =
@"C:\Users\kaioc\source\repos\INTERFACE_POSTRATA\executar_ia.py " +
idade + " " +
psaTotal + " " +
psaLivre + " " +
densidade;

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            Process processo = Process.Start(psi);

            string saida = processo.StandardOutput.ReadToEnd();
            string erro = processo.StandardError.ReadToEnd();

            

            string resultadoIA = saida;

            

            GerarLaudo tela = new GerarLaudo(
    paciente,
    idade,
    medico,
    crm,
    psaTotal,
    psaLivre,
    densidade,
    resultadoIA
); tela.Show();
            this.Close();
        }
        private void VoltarMenu_Click(object sender, RoutedEventArgs e)
        {
            new Window1().Show();
            this.Close();
        }
    }
}