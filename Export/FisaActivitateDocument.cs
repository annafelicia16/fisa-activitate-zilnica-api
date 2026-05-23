using System.Globalization;
using FisaActivitateZilnicaApi.DailyActivities.DTOs.Responses;
using FisaActivitateZilnicaApi.DailyActivities.Models;
using FisaActivitateZilnicaApi.ExternalTeachers.DTOs.Responses;
using FisaActivitateZilnicaApi.SupplementaryActivities.DTOs.Responses;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FisaActivitateZilnicaApi.Export;

internal sealed record FisaActivitateInput(
    TeacherProfileResponse Teacher,
    int Year,
    int Month,
    IReadOnlyList<GetDailyActivityRecordResponse> Records,
    IReadOnlyList<GetSupplementaryActivityResponse> Supplementary
);

internal static class FisaActivitateDocument
{
    public const string FontFamily = "DejaVu Sans";
    private const string HeaderBackground = "#E0E0E0";
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private static readonly string[] MonthsRo =
    [
        "Ianuarie",
        "Februarie",
        "Martie",
        "Aprilie",
        "Mai",
        "Iunie",
        "Iulie",
        "August",
        "Septembrie",
        "Octombrie",
        "Noiembrie",
        "Decembrie",
    ];

    public static byte[] Render(FisaActivitateInput input)
    {
        return Document
            .Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(t => t.FontFamily(FontFamily).FontSize(9));

                    page.Content().Column(col =>
                    {
                        ComposeHeader(col, input);
                        col.Item().PaddingTop(8).Element(c => ComposeTable(c, input));
                        col.Item()
                            .PaddingTop(10)
                            .Text("Activități didactice complementare")
                            .Bold()
                            .FontSize(11);
                        col.Item().PaddingTop(4).Element(c => ComposeSupplementaryTable(c, input));
                        col.Item().PaddingTop(10).Element(c => ComposeTotals(c, input));
                        col.Item()
                            .PaddingTop(6)
                            .Text("* Vor fi detaliate într-o anexă vizată de directorul de departament")
                            .Italic()
                            .FontSize(7);
                    });
                });
            })
            .GeneratePdf();
    }

    private static void ComposeHeader(ColumnDescriptor col, FisaActivitateInput input)
    {
        col.Item().AlignRight().Text("Anexa 8 – HCA nr.16 din 14.12.2016").FontSize(8);

        col.Item()
            .PaddingTop(4)
            .AlignCenter()
            .Text("UNIVERSITATEA TRANSILVANIA DIN BRAȘOV")
            .Bold()
            .FontSize(11);

        string facultyName = input.Teacher.PrimaryFaculty?.Name ?? string.Empty;
        col.Item()
            .PaddingTop(6)
            .Text(
                string.IsNullOrWhiteSpace(facultyName)
                    ? "FACULTATEA ........................................................"
                    : $"FACULTATEA {facultyName.ToUpperInvariant()}"
            )
            .FontSize(10);

        col.Item()
            .PaddingTop(6)
            .Row(row =>
            {
                row.RelativeItem()
                    .Column(c =>
                    {
                        c.Item().Text("Decan,").FontSize(10);
                        c.Item()
                            .PaddingTop(2)
                            .Text("..............................................")
                            .FontSize(10);
                    });
                row.RelativeItem()
                    .Column(c =>
                    {
                        c.Item().Text("Director de departament,").FontSize(10);
                        c.Item()
                            .PaddingTop(2)
                            .Text("..............................................")
                            .FontSize(10);
                    });
            });

        // WHY: profile returns "2025-2026"; spec wants "2025/2026".
        string academicYear = (input.Teacher.CurrentAcademicYear ?? string.Empty).Replace('-', '/');
        string departmentName = input.Teacher.Department?.Name ?? string.Empty;
        string teacherName = $"{input.Teacher.Nume} {input.Teacher.Prenume}".Trim();

        col.Item().PaddingTop(8).Text($"Anul universitar {academicYear}").FontSize(10);
        col.Item().Text($"Departamentul {departmentName}").FontSize(10);
        col.Item().Text($"Cadrul didactic {teacherName}").FontSize(10);

        col.Item()
            .PaddingTop(10)
            .AlignCenter()
            .Text("FIŞA DE ACTIVITATE ZILNICĂ")
            .Bold()
            .FontSize(14);

        string monthName = MonthsRo[input.Month - 1];
        col.Item().AlignCenter().Text($"Luna {monthName}").FontSize(11);
    }

    private static void ComposeTable(IContainer container, FisaActivitateInput input)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(0.7f); // Ziua
                cols.RelativeColumn(1.2f); // Ora
                cols.RelativeColumn(2.0f); // Facultatea
                cols.RelativeColumn(2.2f); // Program de studii
                cols.RelativeColumn(2.2f); // Disciplina
                cols.RelativeColumn(0.6f); // Anul
                cols.RelativeColumn(1.4f); // Grupa
                cols.RelativeColumn(1.2f); // Sala
                cols.RelativeColumn(0.9f); // Ore Efective
                cols.RelativeColumn(1.3f); // Ore Convenționale NB/PO
                cols.RelativeColumn(0.9f); // Norma
                cols.RelativeColumn(1.6f); // Semnătura
            });

            table.Header(header =>
            {
                header.Cell().RowSpan(2).Element(HeaderCell).Text("Ziua");
                header.Cell().RowSpan(2).Element(HeaderCell).Text("Ora");
                header.Cell().RowSpan(2).Element(HeaderCell).Text("Facultatea");
                header.Cell().RowSpan(2).Element(HeaderCell).Text("Program de studii");
                header.Cell().RowSpan(2).Element(HeaderCell).Text("Disciplina");
                header.Cell().RowSpan(2).Element(HeaderCell).Text("Anul");
                header.Cell().RowSpan(2).Element(HeaderCell).Text("Grupa");
                header.Cell().RowSpan(2).Element(HeaderCell).Text("Sala");
                header.Cell().ColumnSpan(2).Element(HeaderCell).Text("Ore");
                header.Cell().RowSpan(2).Element(HeaderCell).Text("Norma");
                header.Cell().RowSpan(2).Element(HeaderCell).Text("Semnătura");

                header.Cell().Element(HeaderCell).Text("Efective");
                header.Cell().Element(HeaderCell).Text("Convenționale NB/PO");

                for (int i = 1; i <= 12; i++)
                {
                    header.Cell().Element(HeaderCell).Text(i.ToString(Inv));
                }
            });

            for (int day = 1; day <= 31; day++)
            {
                List<GetDailyActivityRecordResponse> dayRecords = input
                    .Records.Where(r => r.StartDate.Day == day)
                    .OrderBy(r => r.StartDate)
                    .ToList();

                if (dayRecords.Count == 0)
                    continue;

                for (int idx = 0; idx < dayRecords.Count; idx++)
                {
                    GetDailyActivityRecordResponse r = dayRecords[idx];
                    string ziua = idx == 0 ? day.ToString(Inv) : string.Empty;
                    string ora =
                        $"{r.StartDate:HH:mm}-{r.EndDate:HH:mm}";
                    double effectiveHours = (r.EndDate - r.StartDate).TotalHours;
                    string revenueTag = r.RevenueType == RevenueType.BaseSalary ? "NB" : "PO";
                    string convCell = $"{r.ConventionalHours.ToString("0.00", Inv)} {revenueTag}";

                    table.Cell().Element(BodyCell).Text(ziua);
                    table.Cell().Element(BodyCell).Text(ora);
                    table.Cell().Element(BodyCell).Text(r.FacultyName);
                    table.Cell().Element(BodyCell).Text(r.StudyProgram);
                    table.Cell().Element(BodyCell).Text(r.SubjectName);
                    table.Cell().Element(BodyCell).Text(r.Year.ToString(Inv));
                    table.Cell().Element(BodyCell).Text(r.GroupName);
                    table.Cell().Element(BodyCell).Text(r.RoomName);
                    table.Cell().Element(BodyCell).Text(effectiveHours.ToString("0.00", Inv));
                    table.Cell().Element(BodyCell).Text(convCell);
                    table.Cell().Element(BodyCell).Text(string.Empty);
                    table.Cell().Element(BodyCell).Text(string.Empty);
                }
            }
        });
    }

    private static void ComposeSupplementaryTable(IContainer container, FisaActivitateInput input)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(0.7f); // Ziua
                cols.RelativeColumn(3.5f); // Tipul activității
                cols.RelativeColumn(5.0f); // Observații
                cols.RelativeColumn(1.0f); // Ore
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Ziua");
                header.Cell().Element(HeaderCell).Text("Tipul activității");
                header.Cell().Element(HeaderCell).Text("Observații");
                header.Cell().Element(HeaderCell).Text("Ore");
            });

            List<GetSupplementaryActivityResponse> entries = input
                .Supplementary.OrderBy(s => s.Date)
                .ToList();

            if (entries.Count == 0)
            {
                table.Cell().ColumnSpan(4).Element(BodyCell).AlignCenter()
                    .Text("Nicio activitate complementară în această lună.").Italic();
                return;
            }

            foreach (GetSupplementaryActivityResponse s in entries)
            {
                table.Cell().Element(BodyCell).Text(s.Date.Day.ToString(Inv));
                table.Cell().Element(BodyCell).Text(s.ActivityType);
                table.Cell().Element(BodyCell).Text(s.Observations ?? string.Empty);
                table.Cell().Element(BodyCell).Text(s.TotalHours.ToString("0.00", Inv));
            }
        });
    }

    private static void ComposeTotals(IContainer container, FisaActivitateInput input)
    {
        double totalNb = input
            .Records.Where(r => r.RevenueType == RevenueType.BaseSalary)
            .Sum(r => r.ConventionalHours);
        double totalPo = input
            .Records.Where(r => r.RevenueType == RevenueType.HourlyPay)
            .Sum(r => r.ConventionalHours);
        double totalSupp = input.Supplementary.Sum(s => s.TotalHours);
        double grandTotal = totalNb + totalPo + totalSupp;

        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(4);
                cols.RelativeColumn(1);
            });

            void Row(string label, double value, bool bold = false)
            {
                IContainer labelCell = table.Cell().Element(TotalsCell);
                IContainer valueCell = table.Cell().Element(TotalsCell);
                if (bold)
                {
                    labelCell.Text(label).Bold();
                    valueCell.Text(value.ToString("0.00", Inv)).Bold();
                }
                else
                {
                    labelCell.Text(label);
                    valueCell.Text(value.ToString("0.00", Inv));
                }
            }

            Row("Total ore convenționale NB", totalNb);
            Row("Total ore convenționale PO", totalPo);
            Row("Total ore activități didactice complementare*", totalSupp);
            Row("TOTAL ore lucrate", grandTotal, bold: true);
        });
    }

    private static IContainer HeaderCell(IContainer container) =>
        container
            .Border(0.5f)
            .BorderColor(Colors.Black)
            .Background(HeaderBackground)
            .Padding(2)
            .AlignCenter()
            .AlignMiddle()
            .DefaultTextStyle(t => t.FontFamily(FontFamily).FontSize(8).Bold());

    private static IContainer BodyCell(IContainer container) =>
        container
            .Border(0.5f)
            .BorderColor(Colors.Black)
            .Padding(2)
            .AlignMiddle()
            .DefaultTextStyle(t => t.FontFamily(FontFamily).FontSize(8));

    private static IContainer TotalsCell(IContainer container) =>
        container
            .Border(0.5f)
            .BorderColor(Colors.Black)
            .Padding(3)
            .AlignMiddle()
            .DefaultTextStyle(t => t.FontFamily(FontFamily).FontSize(9));
}
