using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MySql.Data.MySqlClient;
using INTERFACE_POSTRATA.Banco;
using INTERFACE_POSTRATA.Services;

namespace INTERFACE_POSTRATA
{
    public partial class GerarLaudo : Window
    {
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

        // Notas fixas exibidas como texto somente leitura (não são editáveis nem persistidas).
        private const string NotasFixas =
            "Notas:\n" +
            "• A Relação PSA LIVRE / PSA TOTAL inferior ao valor de referência somente será significativa quando o PSA TOTAL estiver entre 4,0 e 10,0 ng/mL.\n" +
            "• A interpretação deste exame deverá ser realizada pelo médico responsável.\n" +
            "• Este exame, de forma isolada, não permite o diagnóstico de neoplasia de próstata. O resultado do PSA deve ser avaliado em conjunto com dados clínicos, exame da próstata e fatores de risco associados.\n" +
            "• Elevações transitórias do PSA podem ocorrer mesmo na ausência de neoplasia.";

        private readonly int? _idLaudo;

        public GerarLaudo(
            string paciente,
            string idade,
            string medico,
            string crm,
            string psaTotal,
            string psaLivre,
            string densidade,
            string resultadoIA,
            string cpf = "",
            string dataNascimento = "",
            string dataExame = "",
            int? idLaudo = null)
        {
            InitializeComponent();

            _idLaudo = idLaudo;

            txtPaciente.Text = string.IsNullOrWhiteSpace(paciente) ? "—" : paciente;
            txtCPF.Text = string.IsNullOrWhiteSpace(cpf) ? "—" : cpf;
            txtDataNascimento.Text = string.IsNullOrWhiteSpace(dataNascimento) ? "—" : dataNascimento;
            txtIdade.Text = string.IsNullOrWhiteSpace(idade) ? "—" : idade + " anos";

            txtMedico.Text = string.IsNullOrWhiteSpace(medico) ? "—" : medico;
            txtCRMMedico.Text = string.IsNullOrWhiteSpace(crm) ? "—" : crm;
            txtData.Text = DateTime.Now.ToString("dd/MM/yyyy", PtBr);

            txtValorPSATotal.Text = FormatarNumero(psaTotal);
            txtValorPSALivre.Text = FormatarNumero(psaLivre);
            txtValorDensidade.Text = FormatarNumero(densidade);

            string psaTotalNormalized = psaTotal?.Replace(",", ".") ?? "";
            string psaLivreNormalized = psaLivre?.Replace(",", ".") ?? "";

            try
            {
                if (double.TryParse(psaTotalNormalized, NumberStyles.Any, CultureInfo.InvariantCulture, out double total)
                    && double.TryParse(psaLivreNormalized, NumberStyles.Any, CultureInfo.InvariantCulture, out double livre)
                    && total > 0)
                {
                    double relacaoLT = (livre / total) * 100;
                    txtValorRelacaoLT.Text = relacaoLT.ToString("F2", PtBr);
                }
                else
                {
                    txtValorRelacaoLT.Text = "—";
                }
            }
            catch
            {
                txtValorRelacaoLT.Text = "—";
            }

            txtClassificacao.Text = resultadoIA;
            AplicarEstiloClassificacao(resultadoIA);
            AplicarInterpretacao(resultadoIA);

            txtAssinaturaMedico.Text = medico;
            txtCRM.Text = string.IsNullOrWhiteSpace(crm) ? "" : crm;

            if (!string.IsNullOrWhiteSpace(dataExame))
            {
                txtDataExame.Text = "Data do exame/coleta: " + dataExame;
                txtDataExame.Visibility = Visibility.Visible;
            }

            // Notas: texto FIXO, somente leitura. Não lê do banco, não salva no banco.
            txtNotas.Text = NotasFixas;
        }

        private static string FormatarNumero(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return "—";
            string normalizado = valor.Replace(",", ".");
            if (double.TryParse(normalizado, NumberStyles.Any, CultureInfo.InvariantCulture, out double numero))
            {
                return numero.ToString("F3", PtBr);
            }
            return valor;
        }

        private void AplicarInterpretacao(string resultadoIA)
        {
            if (resultadoIA == "SUSPEITO")
            {
                txtInterpretacao.Text =
                    "Os valores informados apresentam características compatíveis com risco elevado para alterações prostáticas, sendo recomendada investigação complementar.";
            }
            else
            {
                txtInterpretacao.Text =
                    "Os valores informados apresentam características compatíveis com acompanhamento clínico e monitoramento periódico.";
            }
        }

        private void AplicarEstiloClassificacao(string resultadoIA)
        {
            if (resultadoIA == "SUSPEITO")
            {
                borderClassificacao.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xEB, 0xEE));
                txtClassificacao.Foreground = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));
            }
            else
            {
                borderClassificacao.Background = new SolidColorBrush(Color.FromRgb(0xE8, 0xF5, 0xE9));
                txtClassificacao.Foreground = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
            }
        }

        private void VoltarMenu_Click(object sender, RoutedEventArgs e)
        {
            INTERFACE_POSTRATA.Helpers.NavigationHelper.ShowMainWindow();
            this.Close();
        }

        private void GerarPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sfd = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF Document (*.pdf)|*.pdf",
                    FileName = $"Laudo_{SanitizeFileName(txtPaciente.Text)}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };

                bool? ok = sfd.ShowDialog(this);
                if (ok != true) return;

                string targetPath = sfd.FileName;

                var dados = new LaudoPdfData
                {
                    Paciente = txtPaciente.Text,
                    Cpf = txtCPF.Text,
                    DataNascimento = txtDataNascimento.Text,
                    Idade = txtIdade.Text,
                    Medico = txtMedico.Text,
                    Crm = txtCRMMedico.Text,
                    DataLaudo = txtData.Text,
                    DataExame = (txtDataExame.Visibility == Visibility.Visible)
                        ? txtDataExame.Text.Replace("Data do exame/coleta: ", "").Trim()
                        : string.Empty,
                    PsaTotal = txtValorPSATotal.Text,
                    PsaLivre = txtValorPSALivre.Text,
                    RelacaoLivreTotal = txtValorRelacaoLT.Text,
                    DensidadePsa = txtValorDensidade.Text,
                    Interpretacao = txtInterpretacao.Text,
                    Classificacao = txtClassificacao.Text,
                    Notas = NotasFixas
                };

                // Geração DIRETA de PDF (QuestPDF). Sem XPS, sem impressora, sem Window.
                PdfLaudoService.Gerar(dados, targetPath);

                if (File.Exists(targetPath))
                {
                    MessageBox.Show(
                        $"PDF gerado com sucesso em:\n{targetPath}",
                        "Sucesso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Falha ao gerar PDF (arquivo não encontrado após geração).",
                        "PDF",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao gerar PDF: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "paciente";
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Replace(' ', '_');
        }
    }
}
