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
using INTERFACE_POSTRATA.Banco;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
namespace INTERFACE_POSTRATA
{
    /// <summary>
    /// Lógica interna para CadastroPaciente.xaml
    /// </summary>
    public partial class CadastroPaciente : Window
    {
        private readonly Dictionary<string, string> _prevText = new Dictionary<string, string>();
        private readonly Dictionary<string, int> _lastSelection = new Dictionary<string, int>();
        private bool alterando = false;
        public CadastroPaciente()
        {
            InitializeComponent();
            // garantir captura de eventos para controle de máscara e caret
            txtNascimento.PreviewKeyDown += TxtNascimento_PreviewKeyDown;
            txtNascimento.PreviewTextInput += TxtNascimento_PreviewTextInput;
            DataObject.AddPastingHandler(txtNascimento, OnTxtNascimentoPasting);
        }

        private void TxtNascimento_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb == null) return;
            _prevText[tb.Name] = tb.Text ?? string.Empty;
            _lastSelection[tb.Name] = tb.SelectionStart;
        }

        private void OnTxtNascimentoPasting(object sender, DataObjectPastingEventArgs e)
        {
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb == null) return;
            _prevText[tb.Name] = tb.Text ?? string.Empty;
            _lastSelection[tb.Name] = tb.SelectionStart;
        }

        // Permitir apenas dígitos em alguns campos
        private void DigitsOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        // Tratamento para o campo de data DD/MM/AA
        private void TxtNascimento_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // aceitar apenas dígitos e inserir '/' a cada 2 dígitos
            if (!Regex.IsMatch(e.Text, "^[0-9]+$")) { e.Handled = true; return; }
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb == null) return;
            string text = tb.Text ?? string.Empty;
            int selStart = tb.SelectionStart;
            // permitir inserir mas a lógica de máscara será aplicada no TextChanged
            e.Handled = false;
        }

        private void TxtNascimento_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (alterando)
                return;

            alterando = true;

            TextBox tb = (TextBox)sender;

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

            if (DateTime.TryParseExact(tb.Text,
                                       "dd/MM/yyyy",
                                       null,
                                       System.Globalization.DateTimeStyles.None,
                                       out DateTime nascimento))
            {
                txtIdade.Text = CalculateAge(nascimento).ToString();
            }
        }

        private int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            int age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            return age;
        }

        private async void TxtCEP_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            string cep = txtCEP.Text?.Trim() ?? string.Empty;
            cep = new string(cep.Where(char.IsDigit).ToArray());
            if (cep.Length != 8)
            {
                // CEP inválido: não preencher
                return;
            }

            try
            {
                // Consulta via via CEP público (viacep) para obter logradouro
                string url = $"https://viacep.com.br/ws/{cep}/json/";
                using (var wc = new System.Net.WebClient())
                {
                    wc.Encoding = System.Text.Encoding.UTF8;
                    string json = await wc.DownloadStringTaskAsync(url);
                    if (!string.IsNullOrWhiteSpace(json) && !json.Contains("erro"))
                    {
                        // parse simples
                        var dto = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
                        if (dto.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            if (dto.TryGetProperty("logradouro", out System.Text.Json.JsonElement log) && dto.TryGetProperty("bairro", out System.Text.Json.JsonElement bairro))
                            {
                                string rua = log.GetString() ?? string.Empty;
                                string bairroStr = bairro.GetString() ?? string.Empty;
                                // Preencher somente rua (mantenha número separado)
                                txtEndereco.Text = rua + (string.IsNullOrWhiteSpace(bairroStr) ? string.Empty : (", " + bairroStr));
                            }
                        }
                    }
                }
            }
            catch { /* falha na consulta: silenciar para não interromper fluxo */ }
        }
        private void VoltarMenu_Click(object sender, RoutedEventArgs e)
        {
            INTERFACE_POSTRATA.Helpers.NavigationHelper.ShowMainWindow();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Validar campos antes de tentar inserir
            try
            {
                lblErrorPaciente.Visibility = System.Windows.Visibility.Collapsed;

                string cpf = txtCPF.Text?.Trim();
                string nome = txtNome.Text?.Trim();
                string idadeStr = txtIdade.Text?.Trim();
                string telefone = txtTelefone.Text?.Trim();

                // Validações centralizadas
                var pacienteValidation = PacienteValidator.Validate(cpf, nome, idadeStr, telefone);
                if (!pacienteValidation.IsValid)
                {
                    lblErrorPaciente.Text = pacienteValidation.Message;
                    lblErrorPaciente.Visibility = System.Windows.Visibility.Visible;
                    return;
                }
                int idade = pacienteValidation.Value;

                using (MySqlConnection conn = Conexao.ObterConexao())
                {
                    string sql =
                        @"INSERT INTO paciente
                    (
                        cpf,
                        nome,
                        idade,
                        sexo,
                        data_nascimento,
                        raca,
                        telefone,
                        endereco,
                        tipo_sanguineo,
                        rm_medico
                    )
                    VALUES
                    (
                        @cpf,
                        @nome,
                        @idade,
                        @sexo,
                        @data,
                        @raca,
                        @telefone,
                        @endereco,
                        @sangue,
                        @medico
                    )";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@cpf", cpf);
                        cmd.Parameters.AddWithValue("@nome", nome);
                        cmd.Parameters.AddWithValue("@idade", idade);

                        cmd.Parameters.AddWithValue(
                            "@sexo",
                            ((ComboBoxItem)cbSexo.SelectedItem)?.Content?.ToString() ?? string.Empty
                        );

                        // data de nascimento: tentar parse do campo txtNascimento (DD/MM/AA)
                        object dataParam = System.DBNull.Value;
                        try
                        {
                            string digits = new string((txtNascimento.Text ?? string.Empty).Where(char.IsDigit).ToArray());
                            if (digits.Length >= 6)
                            {
                                string dd = digits.Substring(0, 2);
                                string mm = digits.Substring(2, 2);
                                string aa = digits.Substring(4, 2);
                                if (int.TryParse(dd, out int day) && int.TryParse(mm, out int month) && int.TryParse(aa, out int year2))
                                {
                                    int yearFull = year2 + (year2 <= DateTime.Now.Year % 100 ? 2000 : 1900);
                                    dataParam = new DateTime(yearFull, month, day);
                                }
                            }
                        }
                        catch { dataParam = System.DBNull.Value; }

                        cmd.Parameters.AddWithValue("@data", dataParam);

                        // raça via ComboBox
                        cmd.Parameters.AddWithValue("@raca", ((ComboBoxItem)cbRaca.SelectedItem)?.Content?.ToString() ?? string.Empty);
                        cmd.Parameters.AddWithValue("@telefone", telefone);
                        cmd.Parameters.AddWithValue("@endereco", txtEndereco.Text);

                        cmd.Parameters.AddWithValue(
                            "@sangue",
                            ((ComboBoxItem)cbSangue.SelectedItem)?.Content?.ToString() ?? string.Empty
                        );

                        // médico logado
                        cmd.Parameters.AddWithValue("@medico", INTERFACE_POSTRATA.Helpers.Session.CurrentMedicoId ?? 0);

                        cmd.ExecuteNonQuery();
                    }

                    // conn será fechado automaticamente pelo using
                }

                // Avançar para anamnese
                var anamnese = new CadastroAnamnese();
                anamnese.Show();
                INTERFACE_POSTRATA.Helpers.NavigationHelper.ShowMainWindow();
                this.Close();
            }
            catch (System.Exception ex)
            {
                lblErrorPaciente.Text = ex.Message;
                lblErrorPaciente.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
