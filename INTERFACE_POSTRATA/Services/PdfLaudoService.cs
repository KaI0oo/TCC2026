using System;
using System.Globalization;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace INTERFACE_POSTRATA.Services
{
    /// <summary>
    /// Dados para geração do PDF do laudo.
    /// Todos os campos são opcionais (texto) — campos vazios/nulos são exibidos como "—".
    /// </summary>
    public sealed class LaudoPdfData
    {
        public string Paciente { get; set; } = "";
        public string Cpf { get; set; } = "";
        public string DataNascimento { get; set; } = "";
        public string Idade { get; set; } = "";

        public string Medico { get; set; } = "";
        public string Crm { get; set; } = "";
        public string DataLaudo { get; set; } = "";
        public string DataExame { get; set; } = "";

        public string PsaTotal { get; set; } = "";
        public string PsaLivre { get; set; } = "";
        public string RelacaoLivreTotal { get; set; } = "";
        public string DensidadePsa { get; set; } = "";

        public string Interpretacao { get; set; } = "";
        public string Classificacao { get; set; } = ""; // BENIGNO | SUSPEITO
        public string Notas { get; set; } = "";
    }

    /// <summary>
    /// Gera PDF do laudo diretamente a partir dos dados, sem renderizar
    /// a Window WPF e sem passar por XPS/impressora.
    /// </summary>
    public static class PdfLaudoService
    {
        // Identidade visual (azul/ciano) usada no sistema.
        private static readonly Color AzulCiano = Color.FromARGB(0xFF, 0x00, 0xBC, 0xD4);
        private static readonly Color AzulCianoEscuro = Color.FromARGB(0xFF, 0x00, 0x83, 0x8F);
        private static readonly Color CinzaClaro = Color.FromARGB(0xFF, 0xB0, 0xBE, 0xC5);
        private static readonly Color VerdeBenigno = Color.FromARGB(0xFF, 0x2E, 0x7D, 0x32);
        private static readonly Color FundoBenigno = Color.FromARGB(0xFF, 0xE8, 0xF5, 0xE9);
        private static readonly Color VermelhoSuspeito = Color.FromARGB(0xFF, 0xC6, 0x28, 0x28);
        private static readonly Color FundoSuspeito = Color.FromARGB(0xFF, 0xFF, 0xEB, 0xEE);

        static PdfLaudoService()
        {
            // Necessário para uso não-comercial/comercial sob a licença do QuestPDF.
            // Caso o usuário tenha licença comercial, isso deve ser ajustado.
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Gera o PDF no caminho <paramref name="pdfPath"/>.
        /// Lança exceção se a geração falhar.
        /// </summary>
        public static void Gerar(LaudoPdfData data, string pdfPath)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(pdfPath)) throw new ArgumentException("pdfPath vazio", nameof(pdfPath));

            string dir = Path.GetDirectoryName(pdfPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(t => t.FontFamily(Fonts.SegoeUI).FontSize(11).FontColor(Colors.Black));

                    page.Header().Element(Header);
                    page.Content().Element(c => Conteudo(c, data));
                    page.Footer().Element(Rodape);
                });
            });

            doc.GeneratePdf(pdfPath);
        }

        private static void Header(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Background(AzulCiano).Padding(20).Column(c =>
                {
                    c.Item().AlignCenter().Text("Sistema de Laudos Médicos")
                        .FontSize(20).FontColor(Colors.White).Bold();
                    c.Item().AlignCenter().PaddingTop(4).Text("Laudo de PSA Total e Livre")
                        .FontSize(12).FontColor(Colors.White);
                });

                col.Item().PaddingTop(14).LineHorizontal(1).LineColor(AzulCiano);
            });
        }

        private static void Conteudo(IContainer container, LaudoPdfData d)
        {
            container.PaddingVertical(10).Column(col =>
            {
                col.Spacing(12);

                // Dados do paciente
                col.Item().Element(c => TituloSecao(c, "Dados do Paciente"));
                col.Item().Element(c => CaixaInfo(c, new (string Label, string Value)[]
                {
                    ("Nome", TextoOuTraco(d.Paciente)),
                    ("CPF", TextoOuTraco(d.Cpf)),
                    ("Data de nasc.", TextoOuTraco(d.DataNascimento)),
                    ("Idade", TextoOuTraco(d.Idade))
                }));

                // Dados do médico
                col.Item().Element(c => TituloSecao(c, "Dados do Médico"));
                col.Item().Element(c => CaixaInfo(c, new (string Label, string Value)[]
                {
                    ("Médico", TextoOuTraco(d.Medico)),
                    ("CRM", TextoOuTraco(d.Crm)),
                    ("Data do laudo", TextoOuTraco(d.DataLaudo))
                }));

                // Resultados do exame
                col.Item().Element(c => TituloSecao(c, "Resultados do Exame"));
                col.Item().Element(c => CaixaResultados(c, d));

                // Data do exame (opcional)
                if (!string.IsNullOrWhiteSpace(d.DataExame))
                {
                    col.Item().AlignRight().Text($"Data do exame/coleta: {d.DataExame}")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                }

                // Interpretação
                col.Item().Element(c => TituloSecao(c, "Interpretação"));
                col.Item().Element(c => CaixaTexto(c, TextoOuTraco(d.Interpretacao)));

                // Classificação de risco (título + resultado BENIGNO/SUSPEITO)
                // Mantidos juntos na mesma página para evitar quebra entre eles.
                col.Item().Element(c => BlocoClassificacao(c, d.Classificacao));

                // Notas (depois do bloco de classificação)
                col.Item().Element(c => TituloSecao(c, "Notas"));
                col.Item().Element(c => CaixaTexto(c, TextoOuTraco(d.Notas)));

                // Assinatura
                col.Item().PaddingTop(28).Element(c => Assinatura(c, d));
            });
        }

        private static void Rodape(IContainer container)
        {
            container.AlignCenter().Text(t =>
            {
                t.Span("Documento gerado eletronicamente — Sistema de Laudos Médicos").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        }

        // ---------- helpers visuais ----------

        private static void TituloSecao(IContainer container, string titulo)
        {
            container.Text(titulo).FontSize(14).Bold().FontColor(AzulCianoEscuro);
        }

        private static void CaixaInfo(IContainer container, (string Label, string Value)[] linhas)
        {
            container.Border(1).BorderColor(CinzaClaro).Padding(12).Table(tabela =>
            {
                tabela.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(110);
                    cols.RelativeColumn();
                    cols.ConstantColumn(110);
                    cols.RelativeColumn();
                });

                for (int i = 0; i < linhas.Length; i += 2)
                {
                    tabela.Cell().PaddingVertical(4).Text(linhas[i].Label).SemiBold();
                    tabela.Cell().PaddingVertical(4).Text(linhas[i].Value);

                    if (i + 1 < linhas.Length)
                    {
                        tabela.Cell().PaddingVertical(4).Text(linhas[i + 1].Label).SemiBold();
                        tabela.Cell().PaddingVertical(4).Text(linhas[i + 1].Value);
                    }
                }
            });
        }

        private static void CaixaResultados(IContainer container, LaudoPdfData d)
        {
            container.Border(1).BorderColor(CinzaClaro).Padding(12).Column(col =>
            {
                col.Spacing(8);

                LinhaResultado(col, "PSA Total", TextoOuTraco(d.PsaTotal), "ng/mL");
                col.Item().LineHorizontal(0.5f).LineColor(CinzaClaro);
                LinhaResultado(col, "PSA Livre", TextoOuTraco(d.PsaLivre), "ng/mL");
                col.Item().LineHorizontal(0.5f).LineColor(CinzaClaro);
                LinhaResultado(col, "Relação Livre/Total", TextoOuTraco(d.RelacaoLivreTotal), "%");
                col.Item().LineHorizontal(0.5f).LineColor(CinzaClaro);
                LinhaResultado(col, "Densidade PSA", TextoOuTraco(d.DensidadePsa), "ng/mL");
            });
        }

        private static void LinhaResultado(ColumnDescriptor col, string label, string valor, string unidade)
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(label).SemiBold().FontSize(12);
                    c.Item().PaddingTop(2).Text("Valores de referência são meramente informativos. A interpretação é de responsabilidade do médico.")
                        .FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                });
                row.ConstantItem(80).AlignRight().AlignMiddle().Text(valor).FontSize(18).Bold().FontColor(AzulCianoEscuro);
                row.ConstantItem(40).AlignLeft().AlignMiddle().Text(unidade).FontSize(10).FontColor(Colors.Grey.Darken1);
            });
        }

        private static void CaixaTexto(IContainer container, string texto)
        {
            container.Border(1).BorderColor(CinzaClaro).Padding(12)
                .Text(texto).FontSize(11);
        }

        private static void CaixaClassificacao(IContainer container, string classificacao)
        {
            bool suspeito = string.Equals(classificacao, "SUSPEITO", StringComparison.OrdinalIgnoreCase);

            var corFundo = suspeito ? FundoSuspeito : FundoBenigno;
            var corTexto = suspeito ? VermelhoSuspeito : VerdeBenigno;

            container.Background(corFundo).Padding(16).Column(c =>
            {
                c.Item().AlignCenter().Text("Classificação gerada pela análise assistida (IA)")
                    .FontSize(9).FontColor(Colors.Grey.Medium);
                c.Item().AlignCenter().PaddingTop(6).Text(classificacao.ToUpperInvariant())
                    .FontSize(20).Bold().FontColor(corTexto);
            });
        }

        /// <summary>
        /// Bloco "Classificação de Risco" + resultado BENIGNO/SUSPEITO.
        /// Renderizados como um único container com column interna,
        /// para reduzir a chance de quebra de página entre o título
        /// e o bloco do resultado.
        /// </summary>
        private static void BlocoClassificacao(IContainer container, string classificacao)
        {
            container.Column(col =>
            {
                col.Item().Text("Classificação de Risco").FontSize(14).Bold().FontColor(AzulCianoEscuro);
                col.Item().PaddingTop(8).Element(c => CaixaClassificacao(c, classificacao));
            });
        }

        private static void Assinatura(IContainer container, LaudoPdfData d)
        {
            container.AlignCenter().Column(c =>
            {
                c.Item().AlignCenter().Width(280).LineHorizontal(1).LineColor(Colors.Black);
                c.Item().AlignCenter().PaddingTop(8).Text(TextoOuTraco(d.Medico)).Bold().FontSize(13);
                c.Item().AlignCenter().PaddingTop(2).Text(TextoOuTraco(d.Crm)).FontSize(11);
                c.Item().AlignCenter().PaddingTop(4).Text("Assinado eletronicamente — identificação do médico responsável")
                    .FontSize(9).FontColor(Colors.Grey.Medium);
            });
        }

        // ---------- formatação ----------

        private static string TextoOuTraco(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? "—" : valor.Trim();
        }
    }
}
