using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using INTERFACE_POSTRATA.Banco;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA
{
    public partial class CadastroExame : Window
    {
        private readonly System.Collections.Generic.Dictionary<string, string> _prevText = new System.Collections.Generic.Dictionary<string, string>();
        private readonly System.Collections.Generic.Dictionary<string, int> _lastSelection = new System.Collections.Generic.Dictionary<string, int>();
        private bool alterando = false;
        private string _nomePaciente = string.Empty;
        private string _dataNascimento = string.Empty;
        private int? _editingExameId = null;

        public CadastroExame()
        {
            InitializeComponent();
            dtExame.Loaded += DtExame_Loaded;
            this.Loaded += CadastroExame_Loaded;
        }

        private void CadastroExame_Loaded(object sender, RoutedEventArgs e)
        {
            // Pré-popula o CRM com o do médico logado (se houver) e busca o nome.
            try
            {
                var crmSessao = INTERFACE_POSTRATA.Helpers.Session.CurrentFuncionarioCrm;
                if (!string.IsNullOrWhiteSpace(crmSessao))
                {
                    txtCRMMedico.Text = crmSessao.Trim();
                    ConsultarMedicoPorCRM();
                }
            }
            catch { /* fluxo segue sem pré-preencher */ }
        }

        public CadastroExame(int idExame) : this()
        {
            _editingExameId = idExame;
            Loaded += CadastroExame_EditLoaded;
        }

        private void CadastroExame_EditLoaded(object sender, RoutedEventArgs e)
        {
            ConfigureModoEdicao();
            CarregarExame(_editingExameId!.Value);
        }

        private void ConfigureModoEdicao()
        {
            Title = "Editar Exame";
            btnGerarLaudo.Visibility = Visibility.Collapsed;
            btnMenuPrincipal.Visibility = Visibility.Collapsed;

            // No modo edição expomos um botão "Salvar Alterações" abaixo do formulário
            var btnSalvar = new Button
            {
                Name = "btnSalvarExame",
                Content = "Salvar Alterações",
                Width = 220,
                Height = 50,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Background = System.Windows.Media.Brushes.Teal,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 20, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            btnSalvar.Click += SalvarExame_Click;

            // O StackPanel original está dentro de ScrollViewer; recuperamos o pai
            if (FindName("dtExame") is DatePicker dp && dp.Parent is StackPanel sp)
            {
                sp.Children.Add(btnSalvar);
            }

            txtCPF.IsEnabled = false;
        }

        private void CarregarExame(int idExame)
        {
            try
            {
                using (var conn = Conexao.ObterConexao())
                using (var cmd = new MySqlCommand(
                    @"SELECT e.cpf_paciente, e.psa_total, e.psa_livre, e.densidade_psa, e.data_exame, e.caminho_pdf,
                             p.nome, p.idade, p.data_nascimento
                      FROM exame e
                      INNER JOIN paciente p ON p.cpf = e.cpf_paciente
                      WHERE e.id_exame = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", idExame);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Services.DialogService.Warn("Exame não encontrado.");
                            Close();
                            return;
                        }

                        txtCPF.Text = reader["cpf_paciente"]?.ToString() ?? string.Empty;
                        txtPSATotal.Text = reader["psa_total"]?.ToString() ?? string.Empty;
                        txtPSALivre.Text = reader["psa_livre"]?.ToString() ?? string.Empty;
                        txtDensidade.Text = reader["densidade_psa"]?.ToString() ?? string.Empty;

                        if (reader["data_exame"] != DBNull.Value)
                            dtExame.SelectedDate = Convert.ToDateTime(reader["data_exame"]);

                        _nomePaciente = reader["nome"]?.ToString() ?? string.Empty;
                        txtIdade.Text = reader["idade"]?.ToString() ?? string.Empty;

                        if (reader["data_nascimento"] != DBNull.Value)
                            _dataNascimento = Convert.ToDateTime(reader["data_nascimento"]).ToString("dd/MM/yyyy");
                    }
                }
            }
            catch (Exception ex)
            {
                Services.DialogService.Error("Erro ao carregar exame: " + ex.Message);
                Close();
            }
        }

        private void SalvarExame_Click(object sender, RoutedEventArgs e)
        {
            if (!_editingExameId.HasValue)
            {
                Services.DialogService.Warn("Nenhum exame selecionado para edição.");
                return;
            }

            try
            {
                var exameValidation = Validators.ExameValidator.Validate(
                    txtCPF.Text?.Trim(),
                    txtPSATotal.Text?.Trim(),
                    txtPSALivre.Text?.Trim(),
                    txtDensidade.Text?.Trim(),
                    string.Empty
                );

                if (!exameValidation.IsValid)
                {
                    Services.DialogService.Warn($"Validação do exame falhou: {exameValidation.Message}");
                    return;
                }

                using (var conn = Conexao.ObterConexao())
                using (var cmd = new MySqlCommand(
                    @"UPDATE exame
                      SET psa_total = @psa_total,
                          psa_livre = @psa_livre,
                          densidade_psa = @densidade_psa,
                          data_exame = @data_exame
                      WHERE id_exame = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@psa_total", exameValidation.Value.PsaTotal);
                    cmd.Parameters.AddWithValue("@psa_livre", exameValidation.Value.PsaLivre);
                    cmd.Parameters.AddWithValue("@densidade_psa", exameValidation.Value.Densidade);
                    cmd.Parameters.AddWithValue("@data_exame", dtExame.SelectedDate ?? (object)DateTime.Now);
                    cmd.Parameters.AddWithValue("@id", _editingExameId.Value);

                    int affected = cmd.ExecuteNonQuery();
                    if (affected == 0)
                    {
                        Services.DialogService.Warn("Exame não encontrado. Nenhuma alteração foi salva.");
                        return;
                    }
                }

                Services.DialogService.Info("Exame atualizado com sucesso.");
                INTERFACE_POSTRATA.Helpers.NavigationHelper.ShowMainWindow();
                Close();
            }
            catch (Exception ex)
            {
                Services.DialogService.Error("Erro ao salvar exame: " + ex.Message);
            }
        }

        private void DtExame_Loaded(object? sender, RoutedEventArgs e)
        {
            try
            {
                var dp = dtExame;
                var textBox = FindVisualChild<System.Windows.Controls.Primitives.DatePickerTextBox>(dp);
                if (textBox != null)
                {
                    textBox.PreviewKeyDown += TxtDate_PreviewKeyDown;
                    textBox.PreviewTextInput += TxtDate_PreviewTextInput;
                    DataObject.AddPastingHandler(textBox, OnTxtDatePasting);
                    textBox.TextChanged += TxtDate_TextChanged;
                }
            }
            catch { }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private void TxtDate_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb == null) return;
            _prevText[tb.Name] = tb.Text ?? string.Empty;
            _lastSelection[tb.Name] = tb.SelectionStart;
        }

        private void OnTxtDatePasting(object sender, DataObjectPastingEventArgs e)
        {
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb == null) return;
            _prevText[tb.Name] = tb.Text ?? string.Empty;
            _lastSelection[tb.Name] = tb.SelectionStart;
        }

        private void TxtDate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]+$")) { e.Handled = true; return; }
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb == null) return;
            e.Handled = false;
        }

        private void TxtDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (alterando) return;
            alterando = true;
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb == null) { alterando = false; return; }

            string numeros = new string(tb.Text.Where(char.IsDigit).ToArray());

            if (numeros.Length > 8)
                numeros = numeros.Substring(0, 8);

            if (numeros.Length > 2)
                numeros = numeros.Insert(2, "/");

            if (numeros.Length > 5)
                numeros = numeros.Insert(5, "/");

            tb.Text = numeros;
            tb.SelectionStart = tb.Text.Length;

            alterando = false;
        }

        private void TxtCPF_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string cpf = txtCPF.Text?.Trim();
                if (string.IsNullOrWhiteSpace(cpf)) return;

                using (var conn = Conexao.ObterConexao())
                {
                    using (var cmd = new MySqlCommand("SELECT idade, nome, data_nascimento FROM paciente WHERE cpf = @cpf", conn))
                    {
                        cmd.Parameters.AddWithValue("@cpf", cpf);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var idadeObj = reader["idade"];
                                if (idadeObj != DBNull.Value)
                                {
                                    txtIdade.Text = idadeObj.ToString();
                                }

                                _nomePaciente = reader["nome"]?.ToString() ?? string.Empty;
                                if (reader["data_nascimento"] != DBNull.Value)
                                {
                                    _dataNascimento = Convert.ToDateTime(reader["data_nascimento"]).ToString("dd/MM/yyyy");
                                }
                                else
                                {
                                    _dataNascimento = string.Empty;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Services.DialogService.Error("Erro ao consultar paciente: " + ex.Message);
            }
        }

        // ============ CRM -> NOME DO MÉDICO ============

        private void TxtCRMMedico_LostFocus(object sender, RoutedEventArgs e)
        {
            ConsultarMedicoPorCRM();
        }

        private void TxtCRMMedico_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConsultarMedicoPorCRM();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Consulta funcionario pelo CRM (somente cargo=MEDICO) e preenche
        /// automaticamente o nome. Limpa o nome e avisa se o CRM não
        /// corresponder a um médico cadastrado. Mantém o RM em memória
        /// para uso interno (txtRM).
        /// </summary>
        private void ConsultarMedicoPorCRM()
        {
            try
            {
                string crm = txtCRMMedico?.Text?.Trim() ?? string.Empty;

                // Limpa estado anterior
                txtNomeMedico.Text = string.Empty;
                txtRM.Text = string.Empty;

                if (string.IsNullOrWhiteSpace(crm))
                {
                    // Sem CRM: não tenta consultar, deixa o usuário prosseguir.
                    return;
                }

                using (MySqlConnection conn = Conexao.ObterConexao())
                {
                    const string sql =
                        "SELECT nome, rm " +
                        "FROM funcionario " +
                        "WHERE crm = @crm AND UPPER(cargo) = 'MEDICO' " +
                        "LIMIT 1;";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@crm", crm);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string nome = reader["nome"]?.ToString() ?? string.Empty;
                                string rm = reader["rm"]?.ToString() ?? string.Empty;

                                txtNomeMedico.Text = nome;
                                txtRM.Text = rm; // mantido em memória para identificação interna
                            }
                            else
                            {
                                txtNomeMedico.Text = string.Empty;
                                txtRM.Text = string.Empty;
                                Services.DialogService.Warn(
                                    "CRM não corresponde a um médico cadastrado (cargo=MEDICO).");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtNomeMedico.Text = string.Empty;
                txtRM.Text = string.Empty;
                Services.DialogService.Error("Erro ao consultar médico por CRM: " + ex.Message);
            }
        }

        private void GerarLaudo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string paciente = string.IsNullOrWhiteSpace(_nomePaciente) ? "—" : _nomePaciente;
                // Prioriza o nome/CRM/RM preenchidos a partir da busca por CRM.
                // Caso o usuário não tenha informado CRM, faz fallback para a sessão.
                string medico = !string.IsNullOrWhiteSpace(txtNomeMedico?.Text)
                    ? txtNomeMedico.Text.Trim()
                    : (INTERFACE_POSTRATA.Helpers.Session.CurrentFuncionarioName ?? "");
                string crm = !string.IsNullOrWhiteSpace(txtCRMMedico?.Text)
                    ? txtCRMMedico.Text.Trim()
                    : (INTERFACE_POSTRATA.Helpers.Session.CurrentFuncionarioCrm ?? "");
                string cpf = txtCPF.Text?.Trim() ?? "";
                string dataNascimento = _dataNascimento;
                string dataExame = dtExame.SelectedDate.HasValue
                    ? dtExame.SelectedDate.Value.ToString("dd/MM/yyyy")
                    : "";

                string idadeRaw = txtIdade.Text.Trim();
                string psaTotalRaw = txtPSATotal.Text.Trim();
                string psaLivreRaw = txtPSALivre.Text.Trim();
                string densidadeRaw = txtDensidade.Text.Trim();

                if (string.IsNullOrEmpty(idadeRaw) || string.IsNullOrEmpty(psaTotalRaw) || string.IsNullOrEmpty(psaLivreRaw))
                {
                    Services.DialogService.Warn("Por favor, preencha Idade, PSA Total e PSA Livre antes de gerar o laudo.");
                    return;
                }

                string? idade = Services.NumberFormatHelper.NormalizarNumero(idadeRaw);
                string? psaTotal = Services.NumberFormatHelper.NormalizarNumero(psaTotalRaw);
                string? psaLivre = Services.NumberFormatHelper.NormalizarNumero(psaLivreRaw);
                string? densidade = Services.NumberFormatHelper.NormalizarNumero(densidadeRaw);

                if (string.IsNullOrWhiteSpace(densidadeRaw))
                {
                    Services.DialogService.Info("PSA Densidade não foi informado. O laudo será gerado sem considerar a densidade.");
                    densidade = "0";
                }

                if (idade == null || psaTotal == null || psaLivre == null || densidade == null)
                {
                    Services.DialogService.Warn("Um ou mais campos contêm valores inválidos. Por favor, insira números válidos (use . ou , para decimais).");
                    return;
                }

                string? caminhoScriptIA = EncontrarScriptIA();
                if (string.IsNullOrEmpty(caminhoScriptIA))
                {
                    Services.DialogService.Error(
                        "Arquivo IA/executar_ia.py não encontrado.\n\n" +
                        "Verifique se a pasta IA existe na raiz do repositório com executar_ia.py, IA_generator.py, dados_psa_clinica.csv e IA.joblib.");
                    return;
                }

                string pythonExe = EncontrarPython();
                if (string.IsNullOrEmpty(pythonExe))
                {
                    Services.DialogService.Error("Python não foi encontrado no sistema. Certifique-se de tê-lo instalado.");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Dados enviados para a IA: Idade={idade} PSATotal={psaTotal} PSALivre={psaLivre} Densidade={densidade}");

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = pythonExe;
                psi.Arguments = $"\"{caminhoScriptIA}\" {idade} {psaTotal} {psaLivre} {densidade}";
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                System.Diagnostics.Debug.WriteLine($"Comando: {pythonExe} {psi.Arguments}");

                Process processo = Process.Start(psi);
                processo.WaitForExit();

                string saida = processo.StandardOutput.ReadToEnd().Trim();
                string erro = processo.StandardError.ReadToEnd().Trim();

                System.Diagnostics.Debug.WriteLine($"Saída da IA: {saida}");
                System.Diagnostics.Debug.WriteLine($"Erro da IA: {erro}");

                string resultadoIA = string.Empty;

                if (!string.IsNullOrEmpty(erro))
                {
                    Services.DialogService.Error($"Erro ao executar a IA:\n{erro}");
                    return;
                }

                if (!string.IsNullOrEmpty(saida))
                {
                    resultadoIA = saida.ToUpper().Trim();
                    if (resultadoIA != "SUSPEITO" && resultadoIA != "BENIGNO")
                    {
                        Services.DialogService.Warn($"Resultado inesperado da IA: {resultadoIA}\nUsando valor padrão: BENIGNO");
                        resultadoIA = "BENIGNO";
                    }
                }
                else
                {
                    Services.DialogService.Warn("A IA não retornou um resultado. Usando valor padrão: BENIGNO");
                    resultadoIA = "BENIGNO";
                }

                // Persistir exame + laudo (incluindo NOTAS) no banco.
                int? idExameInserido = null;
                int? idLaudoInserido = null;
                try
                {
                    using (MySqlConnection conn = Conexao.ObterConexao())
                    {
                        var exameValidation = Validators.ExameValidator.Validate(
                            txtCPF.Text?.Trim(),
                            txtPSATotal.Text?.Trim(),
                            txtPSALivre.Text?.Trim(),
                            txtDensidade.Text?.Trim(),
                            string.Empty
                        );

                        if (!exameValidation.IsValid)
                        {
                            Services.DialogService.Warn($"Validação do exame falhou: {exameValidation.Message}");
                            return;
                        }

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
                        );
                        SELECT LAST_INSERT_ID();";

                        using (MySqlCommand cmd = new MySqlCommand(sqlInsert, conn))
                        {
                            cmd.Parameters.AddWithValue("@cpf_paciente", exameValidation.Value.CpfPaciente);
                            cmd.Parameters.AddWithValue("@psa_total", exameValidation.Value.PsaTotal);
                            cmd.Parameters.AddWithValue("@psa_livre", exameValidation.Value.PsaLivre);
                            cmd.Parameters.AddWithValue("@densidade_psa", exameValidation.Value.Densidade);
                            cmd.Parameters.AddWithValue("@data_exame", dtExame.SelectedDate ?? (object)DateTime.Now);
                            cmd.Parameters.AddWithValue("@caminho_pdf", string.Empty);

                            var inserted = cmd.ExecuteScalar();
                            if (inserted != null && inserted != DBNull.Value)
                                idExameInserido = Convert.ToInt32(inserted);
                        }

                        if (idExameInserido.HasValue)
                        {
                            string interpretacaoPadrao = resultadoIA == "SUSPEITO"
                                ? "Os valores informados apresentam características compatíveis com risco elevado para alterações prostáticas, sendo recomendada investigação complementar."
                                : "Os valores informados apresentam características compatíveis com acompanhamento clínico e monitoramento periódico.";

                            // Notas são fixas e somente leitura — não persistidas no banco.
                            string sqlInsertLaudo = @"INSERT INTO laudo
                                (id_exame, classificacao, interpretacao, data_laudo)
                                VALUES
                                (@id_exame, @classificacao, @interpretacao, @data_laudo);
                                SELECT LAST_INSERT_ID();";

                            using (MySqlCommand cmdL = new MySqlCommand(sqlInsertLaudo, conn))
                            {
                                cmdL.Parameters.AddWithValue("@id_exame", idExameInserido.Value);
                                cmdL.Parameters.AddWithValue("@classificacao", resultadoIA);
                                cmdL.Parameters.AddWithValue("@interpretacao", interpretacaoPadrao);
                                cmdL.Parameters.AddWithValue("@data_laudo", DateTime.Now.Date);

                                var insertedL = cmdL.ExecuteScalar();
                                if (insertedL != null && insertedL != DBNull.Value)
                                    idLaudoInserido = Convert.ToInt32(insertedL);
                            }
                        }
                    }
                }
                catch (Exception exConn)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao conectar ao banco: {exConn.Message}");
                }

                // Abrir tela de laudo (passando idLaudo para que possa ser editado/salvo)
                GerarLaudo tela = new GerarLaudo(
                    paciente,
                    idade,
                    medico,
                    crm,
                    psaTotal,
                    psaLivre,
                    densidade,
                    resultadoIA,
                    cpf,
                    dataNascimento,
                    dataExame,
                    idLaudoInserido
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

        private static string? EncontrarScriptIA()
        {
            string dirExecavel = AppDomain.CurrentDomain.BaseDirectory;
            var caminhosProcurados = new List<string>();

            var diretorioAtual = new DirectoryInfo(dirExecavel);
            while (diretorioAtual != null)
            {
                string candidato = System.IO.Path.Combine(diretorioAtual.FullName, "IA", "executar_ia.py");
                caminhosProcurados.Add(candidato);
                if (File.Exists(candidato))
                    return candidato;
                diretorioAtual = diretorioAtual.Parent;
            }

            System.Diagnostics.Debug.WriteLine(
                "IA/executar_ia.py não encontrado. Caminhos verificados:\n" +
                string.Join("\n", caminhosProcurados));

            return null;
        }

        private string EncontrarPython()
        {
            string pythonExe = "python";

            try
            {
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

            return pythonExe;
        }

        private void VoltarMenu_Click(object sender, RoutedEventArgs e)
        {
            INTERFACE_POSTRATA.Helpers.NavigationHelper.ShowMainWindow();
            this.Close();
        }
    }
}
