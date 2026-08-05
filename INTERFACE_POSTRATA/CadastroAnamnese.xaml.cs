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
using System.Linq;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA
{
    /// <summary>
    /// Lógica interna para CadastroAnamnese.xaml
    /// </summary>
    public partial class CadastroAnamnese : Window
    {
        private readonly Dictionary<string, string> _prevText = new Dictionary<string, string>();
        private readonly Dictionary<string, int> _lastSelection = new Dictionary<string, int>();
        private bool alterando = false;

        public CadastroAnamnese()
        {
            InitializeComponent();
            // configurar máscara de data para txtInicio e txtFim usando mesma lógica de CadastroPaciente
            txtInicio.PreviewKeyDown += TxtDate_PreviewKeyDown;
            txtInicio.PreviewTextInput += TxtData_PreviewTextInput;
            DataObject.AddPastingHandler(txtInicio, OnTxtDatePasting);

            txtFim.PreviewKeyDown += TxtDate_PreviewKeyDown;
            txtFim.PreviewTextInput += TxtData_PreviewTextInput;
            DataObject.AddPastingHandler(txtFim, OnTxtDatePasting);
        }
        // Permitir apenas dígitos (usado para dosagem, número, CEP, etc.)
        private void DigitsOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }
        // Reutilizar aqui a mesma implementação de máscara de data usada em CadastroPaciente
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

        private void TxtData_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, "^[0-9]+$")) { e.Handled = true; return; }
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb == null) return;
            // permitir inserir mas a lógica de máscara será aplicada no TextChanged
            e.Handled = false;
        }

        private void TxtData_TextChanged(object sender, TextChangedEventArgs e)
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
        }
        private void ChkDoenca_Checked(object sender, RoutedEventArgs e)
        {
            txtDoencas.IsReadOnly = false;
            txtDoencas.Background = System.Windows.Media.Brushes.White;
        }

        private void ChkDoenca_Unchecked(object sender, RoutedEventArgs e)
        {
            txtDoencas.IsReadOnly = true;
            txtDoencas.Background = System.Windows.Media.Brushes.LightGray;
            txtDoencas.Text = string.Empty;
        }

        private void ChkRemedio_Checked(object sender, RoutedEventArgs e)
        {
            txtRemedio.IsReadOnly = false;
            txtRemedio.Background = System.Windows.Media.Brushes.White;
            txtDosagem.IsReadOnly = false;
            txtDosagem.Background = System.Windows.Media.Brushes.White;
            // habilitar datas quando tomar remédio
            txtInicio.IsReadOnly = false;
            txtInicio.Background = System.Windows.Media.Brushes.White;
            txtFim.IsReadOnly = false;
            txtFim.Background = System.Windows.Media.Brushes.White;
        }

        private void ChkRemedio_Unchecked(object sender, RoutedEventArgs e)
        {
            txtRemedio.IsReadOnly = true;
            txtRemedio.Background = System.Windows.Media.Brushes.LightGray;
            txtRemedio.Text = string.Empty;
            txtDosagem.IsReadOnly = true;
            txtDosagem.Background = System.Windows.Media.Brushes.LightGray;
            txtDosagem.Text = string.Empty;
            // desabilitar e limpar datas
            txtInicio.IsReadOnly = true;
            txtInicio.Background = System.Windows.Media.Brushes.LightGray;
            txtInicio.Text = string.Empty;
            txtFim.IsReadOnly = true;
            txtFim.Background = System.Windows.Media.Brushes.LightGray;
            txtFim.Text = string.Empty;
        }

        private void CbTabagismo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ((ComboBoxItem)cbTabagismo.SelectedItem)?.Content?.ToString() ?? string.Empty;
            if (selected == "Atual" || selected == "Anterior")
            {
                txtFrequencia.IsReadOnly = false;
                txtFrequencia.Background = System.Windows.Media.Brushes.White;
            }
            else
            {
                txtFrequencia.IsReadOnly = true;
                txtFrequencia.Background = System.Windows.Media.Brushes.LightGray;
                txtFrequencia.Text = string.Empty;
            }
        }

        private void CbAlcool_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ((ComboBoxItem)cbAlcool.SelectedItem)?.Content?.ToString() ?? string.Empty;
            if (selected == "Atual" || selected == "Anterior")
            {
                txtFrequencia.IsReadOnly = false;
                txtFrequencia.Background = System.Windows.Media.Brushes.White;
            }
            else
            {
                txtFrequencia.IsReadOnly = true;
                txtFrequencia.Background = System.Windows.Media.Brushes.LightGray;
                txtFrequencia.Text = string.Empty;
            }
        }

        private DateTime? ParseShortDate(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            string digits = new string((text ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length < 6) return null;
            string dd = digits.Substring(0, 2);
            string mm = digits.Substring(2, 2);
            string aa = digits.Substring(4, 2);
            if (!int.TryParse(dd, out int day) || !int.TryParse(mm, out int month) || !int.TryParse(aa, out int year2)) return null;
            int yearFull = year2 + (year2 <= DateTime.Now.Year % 100 ? 2000 : 1900);
            try { return new DateTime(yearFull, month, day); } catch { return null; }
        }
        private void VoltarMenu_Click(object sender, RoutedEventArgs e)
        {
            INTERFACE_POSTRATA.Helpers.NavigationHelper.ShowMainWindow();
            this.Close();
        }

        private void SalvarContinuar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar via AnamneseValidator antes de inserir
                DateTime? inicio = ParseShortDate(txtInicio.Text);
                DateTime? fim = ParseShortDate(txtFim.Text);

                var validation = Validators.AnamneseValidator.Validate(
                    txtCPF.Text?.Trim(),
                    txtRM.Text?.Trim(),
                    chkDoenca.IsChecked == true,
                    txtDoencas.Text,
                    txtObservacoes.Text,
                    chkRemedio.IsChecked == true,
                    txtRemedio.Text,
                    txtDosagem.Text,
                    inicio,
                    fim,
                    ((ComboBoxItem)cbTabagismo.SelectedItem)?.Content?.ToString(),
                    ((ComboBoxItem)cbAlcool.SelectedItem)?.Content?.ToString(),
                    txtFrequencia.Text
                );

                if (!validation.IsValid)
                {
                    MessageBox.Show(validation.Message, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (MySqlConnection conn = Conexao.ObterConexao())
                {
                    // Verificar se o CPF informado corresponde a um paciente cadastrado
                    string sqlCheck = "SELECT COUNT(1) FROM paciente WHERE cpf = @cpf";
                    using (MySqlCommand cmdCheck = new MySqlCommand(sqlCheck, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@cpf", validation.Value.CpfPaciente);
                        var exists = Convert.ToInt32(cmdCheck.ExecuteScalar() ?? 0);
                        if (exists == 0)
                        {
                            MessageBox.Show("Paciente não cadastrado. Cadastre o paciente antes de inserir a anamnese.", "Paciente não encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                    string sql = @"INSERT INTO anamnese
                    (
                        cpf_paciente,
                        rm_medico,
                        possui_doenca,
                        doencas,
                        observacoes,
                        toma_remedio,
                        remedio_nome,
                        dosagem_mg,
                        inicio_tratamento,
                        fim_tratamento,
                        tabagismo,
                        alcool,
                        frequencia
                    ) VALUES (
                        @cpf,@rm,@possui,@doencas,@obs,@remedio,@remedionome,@dosagem,@inicio,@fim,@tabagismo,@alcool,@frequencia
                    );";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@cpf", validation.Value.CpfPaciente);
                        // rm_medico: usar DBNull quando não informado
                        cmd.Parameters.AddWithValue("@rm", validation.Value.RmMedico.HasValue ? (object)validation.Value.RmMedico.Value : (object)System.DBNull.Value);
                        cmd.Parameters.AddWithValue("@possui", validation.Value.PossuiDoenca ? 1 : 0);

                        // quando não possui doença, salvar 'Nunca'
                        cmd.Parameters.AddWithValue("@doencas", validation.Value.PossuiDoenca ? validation.Value.Doencas : "Nunca");

                        cmd.Parameters.AddWithValue("@obs", validation.Value.Observacoes);
                        cmd.Parameters.AddWithValue("@remedio", validation.Value.TomaRemedio ? 1 : 0);

                        // quando não toma remédio, salvar 'Nunca' no nome; dosagem fica DBNull se nula
                        cmd.Parameters.AddWithValue("@remedionome", validation.Value.TomaRemedio ? validation.Value.RemedioNome : "Nunca");
                        cmd.Parameters.AddWithValue("@dosagem", validation.Value.DosagemMg.HasValue ? (object)validation.Value.DosagemMg.Value : (object)System.DBNull.Value);

                        // datas: usar valores validados ou DBNull
                        cmd.Parameters.AddWithValue("@inicio", validation.Value.InicioTratamento ?? (object)System.DBNull.Value);
                        cmd.Parameters.AddWithValue("@fim", validation.Value.FimTratamento ?? (object)System.DBNull.Value);

                        // tabagismo/alcool: se vazio, salvar 'Nunca'
                        cmd.Parameters.AddWithValue("@tabagismo", string.IsNullOrWhiteSpace(validation.Value.Tabagismo) ? "Nunca" : validation.Value.Tabagismo);
                        cmd.Parameters.AddWithValue("@alcool", string.IsNullOrWhiteSpace(validation.Value.Alcool) ? "Nunca" : validation.Value.Alcool);

                        // frequencia: permitir vazio
                        cmd.Parameters.AddWithValue("@frequencia", string.IsNullOrWhiteSpace(validation.Value.Frequencia) ? "" : validation.Value.Frequencia);

                        try { cmd.ExecuteNonQuery(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Erro ao inserir anamnese: {ex.Message}"); }
                    }
                }

                // Avançar para próxima tela (se houver)
                MessageBox.Show("Anamnese salva com sucesso.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                INTERFACE_POSTRATA.Helpers.NavigationHelper.ShowMainWindow();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar anamnese: {ex.Message}");
            }
        }

    }
}
