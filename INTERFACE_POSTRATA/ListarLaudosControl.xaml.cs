using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using INTERFACE_POSTRATA.Banco;
using INTERFACE_POSTRATA.Helpers;
using MySql.Data.MySqlClient;

namespace INTERFACE_POSTRATA
{
    public partial class ListarLaudosControl : UserControl
    {
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
        private readonly bool _permitirVisualizacao;
        private readonly bool _autoCarregar;

        public ListarLaudosControl(bool permitirVisualizacao = false, bool autoCarregar = true)
        {
            _permitirVisualizacao = permitirVisualizacao;
            _autoCarregar = autoCarregar;
            InitializeComponent();
            ConfigureAcesso();
            if (_autoCarregar)
                CarregarLaudos();
        }

        private void ConfigureAcesso()
        {
            if (_permitirVisualizacao)
                btnVisualizar.Visibility = Visibility.Visible;

            if (Session.IsSecretaria)
                btnTodos.Visibility = Visibility.Collapsed;
        }

        public void CarregarLaudos(string? cpf = null)
        {
            try
            {
                using MySqlConnection conn = Conexao.ObterConexao();

                if (!string.IsNullOrWhiteSpace(cpf))
                {
                    if (!PacienteExiste(conn, cpf))
                    {
                        dgLaudos.ItemsSource = null;
                        txtStatus.Text = $"Nenhum paciente encontrado com o CPF {cpf.Trim()}.";
                        return;
                    }
                }

                string sql = @"SELECT l.id_laudo, l.id_exame, l.classificacao, l.interpretacao, l.data_laudo,
                                      e.cpf_paciente, e.psa_total, e.psa_livre, e.densidade_psa, e.data_exame,
                                      p.nome AS paciente_nome, p.idade, p.data_nascimento
                               FROM laudo l
                               INNER JOIN exame e ON e.id_exame = l.id_exame
                               INNER JOIN paciente p ON p.cpf = e.cpf_paciente";

                if (!string.IsNullOrWhiteSpace(cpf))
                    sql += " WHERE e.cpf_paciente = @cpf";

                sql += " ORDER BY l.data_laudo DESC, l.id_laudo DESC";

                using MySqlCommand cmd = new MySqlCommand(sql, conn);
                if (!string.IsNullOrWhiteSpace(cpf))
                    cmd.Parameters.AddWithValue("@cpf", cpf.Trim());

                using MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dgLaudos.ItemsSource = dt.DefaultView;
                AtualizarStatus(dt.Rows.Count, cpf);
            }
            catch (Exception ex)
            {
                dgLaudos.ItemsSource = null;
                txtStatus.Text = string.Empty;
                MessageBox.Show($"Erro ao carregar laudos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool PacienteExiste(MySqlConnection conn, string cpf)
        {
            using MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM paciente WHERE cpf = @cpf", conn);
            cmd.Parameters.AddWithValue("@cpf", cpf.Trim());
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private void AtualizarStatus(int total, string? cpf)
        {
            if (total == 0)
            {
                txtStatus.Text = string.IsNullOrWhiteSpace(cpf)
                    ? "Nenhum laudo cadastrado no sistema."
                    : $"O paciente informado não possui laudos cadastrados (CPF {cpf.Trim()}).";
                return;
            }

            txtStatus.Text = string.IsNullOrWhiteSpace(cpf)
                ? $"{total} laudo(s) encontrado(s)."
                : $"{total} laudo(s) encontrado(s) para o CPF {cpf.Trim()}.";
        }

        private void BtnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            string cpf = txtPesquisa.Text.Trim();
            if (string.IsNullOrWhiteSpace(cpf))
            {
                MessageBox.Show("Informe o CPF do paciente para pesquisar.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CarregarLaudos(cpf);
        }

        private void BtnTodos_Click(object sender, RoutedEventArgs e)
        {
            txtPesquisa.Text = string.Empty;
            CarregarLaudos();
        }

        private void BtnVisualizar_Click(object sender, RoutedEventArgs e)
        {
            VisualizarLaudoSelecionado();
        }

        private void VisualizarLaudoSelecionado()
        {
            if (dgLaudos.SelectedItem is not DataRowView row)
            {
                MessageBox.Show("Selecione um laudo na lista para visualizar.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(row["id_laudo"]?.ToString(), out int idLaudo))
            {
                MessageBox.Show("Laudo selecionado inválido.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using MySqlConnection conn = Conexao.ObterConexao();
                string sql = @"SELECT l.id_laudo, l.classificacao, l.interpretacao, l.data_laudo,
                                      e.psa_total, e.psa_livre, e.densidade_psa, e.data_exame, e.cpf_paciente,
                                      p.nome AS paciente_nome, p.idade, p.data_nascimento,
                                      m.nome AS medico_nome, m.crm AS medico_crm
                               FROM laudo l
                               INNER JOIN exame e ON e.id_exame = l.id_exame
                               INNER JOIN paciente p ON p.cpf = e.cpf_paciente
                               LEFT JOIN funcionario m ON m.rm = p.rm_medico
                               WHERE l.id_laudo = @id";

                using MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", idLaudo);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    MessageBox.Show("Laudo não encontrado.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string paciente = reader["paciente_nome"]?.ToString() ?? "—";
                string cpf = reader["cpf_paciente"]?.ToString() ?? "";
                string idade = reader["idade"]?.ToString() ?? "";
                string medico = reader["medico_nome"]?.ToString() ?? Session.CurrentFuncionarioName ?? "—";
                string crm = reader["medico_crm"]?.ToString() ?? Session.CurrentFuncionarioCrm ?? "—";
                string psaTotal = FormatarDecimal(reader["psa_total"]);
                string psaLivre = FormatarDecimal(reader["psa_livre"]);
                string densidade = FormatarDecimal(reader["densidade_psa"]);
                string classificacao = reader["classificacao"]?.ToString() ?? "—";
                string dataNascimento = FormatarData(reader["data_nascimento"]);
                string dataExame = FormatarData(reader["data_exame"]);

                var tela = new GerarLaudo(
                    paciente,
                    idade,
                    medico,
                    crm,
                    psaTotal,
                    psaLivre,
                    densidade,
                    classificacao,
                    cpf,
                    dataNascimento,
                    dataExame);
                tela.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao visualizar laudo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string FormatarDecimal(object? valor)
        {
            if (valor == null || valor == DBNull.Value) return "—";
            if (decimal.TryParse(valor.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal numero))
                return numero.ToString("F2", PtBr);
            return valor.ToString() ?? "—";
        }

        private static string FormatarData(object? valor)
        {
            if (valor == null || valor == DBNull.Value) return "—";
            if (valor is DateTime dt) return dt.ToString("dd/MM/yyyy", PtBr);
            if (DateTime.TryParse(valor.ToString(), out DateTime parsed))
                return parsed.ToString("dd/MM/yyyy", PtBr);
            return valor.ToString() ?? "—";
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is Window1 win)
                win.RestoreMainPanel();
        }
    }
}
