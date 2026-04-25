using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FisaActivitateZilnicaApi.Export.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    [HttpGet("export-pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(statusCode: 200)]
    public IActionResult ExportDailyActivitySheetPdf()
    {
        byte[] pdfBytes = Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(12));

                page.Header()
                    .PaddingBottom(10)
                    .AlignCenter()
                    .Text("Fișă de Activitate Zilnică")
                    .SemiBold()
                    .FontSize(18);

                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(TableHeaderCell).Text("Dată");
                                header.Cell().Element(TableHeaderCell).Text("Activitate Didactică");
                                header.Cell().Element(TableHeaderCell).Text("Ore");
                            });

                            table.Cell().Element(TableBodyCell).Text("20.04.2026");
                            table.Cell()
                                .Element(TableBodyCell)
                                .Text("Sisteme Distribuite — conf. dr. Ștefan Țițeica");
                            table.Cell().Element(TableBodyCell).Text("2");

                            table.Cell().Element(TableBodyCell).Text("21.04.2026");
                            table.Cell()
                                .Element(TableBodyCell)
                                .Text("Laborator Sisteme Distribuite (grupa 3141A)");
                            table.Cell().Element(TableBodyCell).Text("4");
                        });
                    });

                page.Footer()
                    .PaddingTop(10)
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Pagina ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
            });
        }).GeneratePdf();

        return File(pdfBytes, "application/pdf", "Fisa_Activitate.pdf");
    }

    private static IContainer TableHeaderCell(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Medium)
            .PaddingVertical(6)
            .DefaultTextStyle(x => x.FontFamily(Fonts.Arial).SemiBold());
    }

    private static IContainer TableBodyCell(IContainer container)
    {
        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
    }
}
