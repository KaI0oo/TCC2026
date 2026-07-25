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
using INTERFACE_POSTRATA.Banco;
using MySql.Data.MySqlClient;
namespace INTERFACE_POSTRATA
{
    public partial class CadastroExame : Window
    {
        public CadastroExame()
        {
            InitializeComponent();
        }

        private void GerarLaudoHtml_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Coletar dados de formulário
                string paciente = "Paciente Teste";
                string medico = INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoName ?? "";
                string crm = "CRM 123456";

                string idadeRaw = txtIdade.Text.Trim();
                string psaTotalRaw = txtPSATotal.Text.Trim();
                string psaLivreRaw = txtPSALivre.Text.Trim();
                string densidadeRaw = txtDensidade.Text.Trim();

                if (string.IsNullOrEmpty(idadeRaw) || string.IsNullOrEmpty(psaTotalRaw) || string.IsNullOrEmpty(psaLivreRaw))
                {
                    Services.DialogService.Warn("Por favor, preencha Idade, PSA Total e PSA Livre antes de gerar o laudo HTML.");
                    return;
                }

                string? idade = Services.NumberFormatHelper.NormalizarNumero(idadeRaw);
                string? psaTotal = Services.NumberFormatHelper.NormalizarNumero(psaTotalRaw);
                string? psaLivre = Services.NumberFormatHelper.NormalizarNumero(psaLivreRaw);
                string? densidade = Services.NumberFormatHelper.NormalizarNumero(densidadeRaw);

                if (string.IsNullOrWhiteSpace(densidadeRaw))
                {
                    Services.DialogService.Info("PSA Densidade não informado. O laudo HTML será gerado sem considerar a densidade.");
                    densidade = "-";
                }

                if (idade == null || psaTotal == null || psaLivre == null)
                {
                    Services.DialogService.Warn("Um ou mais campos contêm valores inválidos. Verifique os campos numéricos.");
                    return;
                }

                var exame = new Models.Exame
                {
                    PacienteNome = paciente,
                    Idade = txtIdade.Text.Trim(),
                    Medico = medico,
                    Crm = crm,
                    PsaTotal = txtPSATotal.Text.Trim(),
                    PsaLivre = txtPSALivre.Text.Trim(),
                    PsaDensidade = string.IsNullOrWhiteSpace(txtDensidade.Text) ? "-" : txtDensidade.Text.Trim(),
                    Resultado = "" // resultado pode ser preenchido manualmente ou pela IA; deixamos vazio aqui
                };

                Services.HtmlLaudoService.GenerateAndOpenHtml(exame);
            }
            catch (Exception ex)
            {
                Services.DialogService.Error($"Erro ao gerar laudo HTML: {ex.Message}");
            }
        }

        private void LimparCampos_Click(object sender, RoutedEventArgs e)
        {
            txtCPF.Text = string.Empty;
            txtRM.Text = string.Empty;
            txtIdade.Text = string.Empty;
            txtPSATotal.Text = string.Empty;
            txtPSALivre.Text = string.Empty;
            txtDensidade.Text = string.Empty;
            txtPDFSelecionado.Text = "Nenhum PDF selecionado";
            txtPSAEncontrado.Text = "--";
            dtExame.SelectedDate = null;
            // Garantir que o botão de confirmar esteja desabilitado ao limpar
            try { btnConfirmarPSA.IsEnabled = false; } catch { }
        }

        private void ConfirmarPSA_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var valor = txtPSAEncontrado.Text?.Trim();
                if (!string.IsNullOrEmpty(valor) && valor != "--")
                {
                    txtPSATotal.Text = valor.Replace('.', ',');
                    btnConfirmarPSA.IsEnabled = false;
                    MessageBox.Show("PSA confirmado e preenchido no campo PSA Total.", "Confirmado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Nenhum valor de PSA válido para confirmar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao confirmar PSA: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelecionarPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Filter = "PDF files (*.pdf)|*.pdf";
                bool? result = ofd.ShowDialog();
                if (result == true)
                {
                    string caminho = ofd.FileName;
                    // Verificar existência do arquivo antes de tentar importar
                    if (!System.IO.File.Exists(caminho))
                    {
                        Services.DialogService.Error("Arquivo PDF não encontrado.");
                        return;
                    }

                    txtPDFSelecionado.Text = System.IO.Path.GetFileName(caminho);

                    var exame = Services.PdfImportService.ImportFromPdf(caminho);
                    if (exame != null && !string.IsNullOrWhiteSpace(exame.PsaTotal))
                    {
                        txtPSAEncontrado.Text = exame.PsaTotal;
                        btnConfirmarPSA.IsEnabled = true;
                    }
                    else
                    {
                        txtPSAEncontrado.Text = "--";
                        btnConfirmarPSA.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Services.DialogService.Error($"Erro ao selecionar PDF: {ex.Message}");
            }
        }

        private void BtnImportarPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Filter = "PDF files (*.pdf)|*.pdf";
                bool? result = ofd.ShowDialog();
                if (result == true)
                {
                    string caminho = ofd.FileName;
                    // Validar existência antes da importação
                    if (!System.IO.File.Exists(caminho))
                    {
                        Services.DialogService.Error("Arquivo PDF não encontrado.");
                        return;
                    }

                    txtPDFSelecionado.Text = System.IO.Path.GetFileName(caminho);

                    var exame = Services.PdfImportService.ImportFromPdf(caminho);
                    if (exame == null)
                    {
                        Services.DialogService.Warn("Não foi possível extrair informações do PDF.");
                        return;
                    }

                    // Preencher campos quando encontrados (mantendo compatibilidade com comportamento atual)
                    if (!string.IsNullOrEmpty(exame.PsaTotal)) txtPSATotal.Text = exame.PsaTotal;
                    if (!string.IsNullOrEmpty(exame.PsaLivre)) txtPSALivre.Text = exame.PsaLivre;
                    if (!string.IsNullOrEmpty(exame.PsaDensidade)) txtDensidade.Text = exame.PsaDensidade;

                    Services.DialogService.Info("Importação concluída. Revise os valores antes de salvar.");
                }
            }
            catch (Exception ex)
            {
                Services.DialogService.Error($"Erro ao importar PDF: {ex.Message}");
            }
        }

        private void GerarLaudo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Dados do paciente (você pode querer buscar do banco de dados)
                string paciente = "Paciente Teste";
                // usar medico logado quando disponível
                string medico = INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoName ?? "";
                string crm = "CRM 123456";

                // Coletar e normalizar os dados de entrada
                string idadeRaw = txtIdade.Text.Trim();
                string psaTotalRaw = txtPSATotal.Text.Trim();
                string psaLivreRaw = txtPSALivre.Text.Trim();
                string densidadeRaw = txtDensidade.Text.Trim();

                // Validar campos obrigatórios antes de chamar a IA (densidade pode faltar)
                if (string.IsNullOrEmpty(idadeRaw) || string.IsNullOrEmpty(psaTotalRaw) || string.IsNullOrEmpty(psaLivreRaw))
                {
                    Services.DialogService.Warn("Por favor, preencha Idade, PSA Total e PSA Livre antes de gerar o laudo.");
                    return;
                }

                // Normalizar e validar números (aceita "," ou ".")
                string? idade = Services.NumberFormatHelper.NormalizarNumero(idadeRaw);
                string? psaTotal = Services.NumberFormatHelper.NormalizarNumero(psaTotalRaw);
                string? psaLivre = Services.NumberFormatHelper.NormalizarNumero(psaLivreRaw);
                string? densidade = Services.NumberFormatHelper.NormalizarNumero(densidadeRaw);

                // Se densidade estiver ausente, avisar e usar valor 0 para chamada à IA
                if (string.IsNullOrWhiteSpace(densidadeRaw))
                {
                    Services.DialogService.Info("PSA Densidade não foi informado. O laudo será gerado sem considerar a densidade.");
                    densidade = "0";
                }

                // Verificar se a normalização foi bem-sucedida
                if (idade == null || psaTotal == null || psaLivre == null || densidade == null)
                {
                    Services.DialogService.Warn("Um ou mais campos contêm valores inválidos. Por favor, insira números válidos (use . ou , para decimais)." );
                    return;
                }

                // Obter diretório do executável
                string dirExecavel = AppDomain.CurrentDomain.BaseDirectory;

                // Procurar pelo script executar_ia.py em múltiplos locais
                string caminhoScriptIA = null;

                // Tentar em múltiplos caminhos (estrutura: bin\Debug\net10.0-windows\)
                string[] caminhosPossiveis = new[]
                {
                    System.IO.Path.Combine(dirExecavel, "..", "..", "..", "..", "executar_ia.py"),  // ../../../../ (sai de net10.0-windows/Debug/bin/INTERFACE_POSTRATA) para raiz
                    System.IO.Path.Combine(dirExecavel, "..", "..", "..", "executar_ia.py"),       // ../../../ (sai de net10.0-windows/Debug/bin)
                    System.IO.Path.Combine(dirExecavel, "..", "..", "executar_ia.py"),             // ../../ (sai de Debug/bin)
                    System.IO.Path.Combine(dirExecavel, "..", "executar_ia.py"),                   // ../ (sai de bin)
                    System.IO.Path.Combine(dirExecavel, "executar_ia.py")                          // direto em BaseDirectory
                };

                foreach (var caminho in caminhosPossiveis)
                {
                    string caminhoCompleto = System.IO.Path.GetFullPath(caminho);
                    if (System.IO.File.Exists(caminhoCompleto))
                    {
                        caminhoScriptIA = caminhoCompleto;
                        break;
                    }
                }

                // Verificar se o script foi encontrado
                if (string.IsNullOrEmpty(caminhoScriptIA))
                {
                    string mensagemErro = $"Arquivo executar_ia.py não encontrado.\n\nProcurou em:\n" +
                        string.Join("\n", caminhosPossiveis.Select(p => System.IO.Path.GetFullPath(p)));
                    Services.DialogService.Error(mensagemErro);
                    return;
                }

                // Encontrar Python instalado
                string pythonExe = EncontrarPython();
                if (string.IsNullOrEmpty(pythonExe))
                {
                    Services.DialogService.Error("Python não foi encontrado no sistema. Certifique-se de tê-lo instalado.");
                    return;
                }

                // Debug: Exibir dados sendo enviados para a IA
                string debugMsg = $"Dados enviados para a IA:\n" +
                                 $"Idade: {idade}\n" +
                                 $"PSA Total: {psaTotal}\n" +
                                 $"PSA Livre: {psaLivre}\n" +
                                 $"Densidade: {densidade}\n\n" +
                                 $"Relação L/T calculada pela IA: {psaLivre} / {psaTotal}";

                System.Diagnostics.Debug.WriteLine(debugMsg);

                // Configurar processo
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = pythonExe;
                psi.Arguments = $"\"{caminhoScriptIA}\" {idade} {psaTotal} {psaLivre} {densidade}";
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                // Debug: Exibir comando sendo executado
                System.Diagnostics.Debug.WriteLine($"Comando: {pythonExe} {psi.Arguments}");

                // Executar IA
                Process processo = Process.Start(psi);
                processo.WaitForExit();

                string saida = processo.StandardOutput.ReadToEnd().Trim();
                string erro = processo.StandardError.ReadToEnd().Trim();

                // Debug: Exibir saída da IA
                System.Diagnostics.Debug.WriteLine($"Saída da IA: {saida}");
                System.Diagnostics.Debug.WriteLine($"Erro da IA: {erro}");

                // Tratar resultado
                string resultadoIA = string.Empty;

                if (!string.IsNullOrEmpty(erro))
                {
                    Services.DialogService.Error($"Erro ao executar a IA:\n{erro}");
                    return;
                }

                if (!string.IsNullOrEmpty(saida))
                {
                    resultadoIA = saida.ToUpper().Trim();
                    // Validar resultado
                    if (resultadoIA != "SUSPEITO" && resultadoIA != "BENIGNO")
                    {
                        Services.DialogService.Warn($"Resultado inesperado da IA: {resultadoIA}\nUsando valor padrão: BENIGNO");
                        resultadoIA = "BENIGNO"; // valor padrão
                    }
                }
                else
                {
                    Services.DialogService.Warn("A IA não retornou um resultado. Usando valor padrão: BENIGNO");
                    resultadoIA = "BENIGNO"; // valor padrão
                }

                // Tentar salvar exame no banco (se houver tabela 'exame')
                try
                {
                    using (MySqlConnection conn = Conexao.ObterConexao())
                    {
                        // Inserir no esquema correto da tabela 'exame'
                        string sqlInsert = @"INSERT INTO exame
                        (
                            cpf_paciente,
                            psa_total,
                            psa_livre,
                            densidade_psa,
                            data_exame,
                            caminho_pdf
                        )
                        VALUES
                        (
                            @cpf_paciente,
                            @psa_total,
                            @psa_livre,
                            @densidade_psa,
                            @data_exame,
                            @caminho_pdf
                        );";

                        using (MySqlCommand cmd = new MySqlCommand(sqlInsert, conn))
                        {
                            // Validar via ExameValidator antes de inserir
                            var exameValidation = Validators.ExameValidator.Validate(
                                txtCPF.Text?.Trim(),
                                txtPSATotal.Text?.Trim(),
                                txtPSALivre.Text?.Trim(),
                                txtDensidade.Text?.Trim(),
                                txtPDFSelecionado.Text == "Nenhum PDF selecionado" ? string.Empty : txtPDFSelecionado.Text
                            );

                            if (!exameValidation.IsValid)
                            {
                                Services.DialogService.Warn($"Validação do exame falhou: {exameValidation.Message}");
                                return;
                            }

                            // cpf do paciente vem do campo do formulário
                            cmd.Parameters.AddWithValue("@cpf_paciente", exameValidation.Value.CpfPaciente);
                            cmd.Parameters.AddWithValue("@psa_total", exameValidation.Value.PsaTotal);
                            cmd.Parameters.AddWithValue("@psa_livre", exameValidation.Value.PsaLivre);
                            cmd.Parameters.AddWithValue("@densidade_psa", exameValidation.Value.Densidade);
                            cmd.Parameters.AddWithValue("@data_exame", dtExame.SelectedDate ?? (object)DateTime.Now);
                            // caminho do PDF (se o usuário importou) - armazenar apenas o nome do arquivo por enquanto
                            cmd.Parameters.AddWithValue("@caminho_pdf", exameValidation.Value.CaminhoPdf ?? string.Empty);

                            try { cmd.ExecuteNonQuery(); } catch (Exception exDb) { System.Diagnostics.Debug.WriteLine($"Erro ao inserir exame: {exDb.Message}"); }
                        }
                    }
                }
                catch (Exception exConn)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao conectar ao banco: {exConn.Message}");
                }

                // Abrir tela de laudo
                GerarLaudo tela = new GerarLaudo(
                    paciente,
                    idade,
                    medico,
                    crm,
                    psaTotal,
                    psaLivre,
                    densidade,
                    resultadoIA
                );
                tela.Show();
                INTERFACE_POSTRATA.Helpers.NavigationHelper.ShowMainWindow();
                this.Close();
            }
            catch (Exception ex)
            {
                Services.DialogService.Error($"Erro ao gerar laudo:\n{ex.Message}");
            }
        }

        private string EncontrarPython()
        {
            // Procurar Python no PATH
            string pythonExe = "python";

            try
            {
                // Tentar usar 'where python' no Windows
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c where python")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process p = Process.Start(psi);
                string resultado = p.StandardOutput.ReadToEnd().Trim();

                if (!string.IsNullOrEmpty(resultado))
                {
                    return resultado.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
                }
            }
            catch { }

            // Caminhos comuns onde Python pode estar
            string[] caminhosPython = new[]
            {
                @"C:\Python311\python.exe",
                @"C:\Python310\python.exe",
                @"C:\Python312\python.exe",
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Python\Python311\python.exe",
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Python\Python310\python.exe",
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Python\Python312\python.exe",
                @"C:\Program Files\Python311\python.exe",
                @"C:\Program Files\Python310\python.exe",
                @"C:\Program Files\Python312\python.exe"
            };

            foreach (string caminho in caminhosPython)
            {
                if (System.IO.File.Exists(caminho))
                {
                    return caminho;
                }
            }

            return pythonExe; // retorna "python" como fallback
        }

        // Usamos Services.NumberFormatHelper.NormalizarNumero em vez do método local

        private void VoltarMenu_Click(object sender, RoutedEventArgs e)
        {
            INTERFACE_POSTRATA.Helpers.NavigationHelper.ShowMainWindow();
            this.Close();
        }
    }
}