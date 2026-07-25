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

namespace INTERFACE_POSTRATA
{
    public partial class GerarLaudo : Window
    {
        public GerarLaudo(
            string paciente,
            string idade,
            string medico,
            string crm,
            string psaTotal,
            string psaLivre,
            string densidade,
            string resultadoIA)
        {
            InitializeComponent();

            txtPaciente.Text = paciente;
            txtIdade.Text = idade + " anos";
            txtData.Text = DateTime.Now.ToShortDateString();
            txtMedico.Text = medico;

            txtPSATotalLaudo.Text = "PSA Total: " + psaTotal;
            txtPSALivreLaudo.Text = "PSA Livre: " + psaLivre;
            txtDensidadeLaudo.Text = "Densidade PSA: " + densidade;

            double total = Convert.ToDouble(psaTotal);
            double livre = Convert.ToDouble(psaLivre);

            txtRelacaoLT.Text =
                "Relação L/T: " +
                ((livre / total) * 100).ToString("F2") +
                "%";

            txtClassificacao.Text = resultadoIA;

            if (resultadoIA == "SUSPEITO")
            {
                txtInterpretacao.Text =
                    "Os valores informados apresentam características compatíveis com risco elevado para alterações prostáticas, sendo recomendada investigação complementar.";

                txtAssinaturaMedico.Text = medico;
                txtCRM.Text = crm;
            }
            else
            {
                txtInterpretacao.Text =
                    "Os valores informados apresentam características compatíveis com acompanhamento clínico e monitoramento periódico.";

                txtAssinaturaMedico.Text = medico;
                txtCRM.Text = crm;
            }
        }
        private void VoltarMenu_Click(object sender, RoutedEventArgs e)
        {
            new Window1().Show();
            this.Close();
        }
    }
}
