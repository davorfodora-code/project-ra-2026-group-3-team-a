using System;
using System.Collections.Generic;
using System.Globalization;
using CommunityHub.Application.Services.BuildingManager.Dto;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CommunityHub.Ui.Reports;

// Generator PDF izvestaja o skupstinama stanara za izabranu zgradu
// u datom vremenskom periodu — sa spiskom prisustva po stanovima.
public static class AssembliesReportGenerator
{
    private static readonly CultureInfo SrLatin = new CultureInfo("sr-Latn-RS");

    // Vraca byte[] generisanog PDF-a — caller ga sacuva u fajl i otvori
    public static byte[] Generate(
        BuildingDto building,
        DateTime startDate,
        DateTime endDate,
        IReadOnlyList<AssemblyDetailDto> assemblies)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(t => t.FontSize(11).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader(c, building, startDate, endDate));
                page.Content().Element(c => ComposeContent(c, assemblies));
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, BuildingDto building, DateTime start, DateTime end)
    {
        container.Column(col =>
        {
            col.Item().Text("Izveštaj o skupštinama stanara")
                .FontSize(20).Bold().FontColor(Colors.Black);

            col.Item().PaddingTop(4).Text(building.AddressDisplay)
                .FontSize(13).SemiBold().FontColor(Colors.Grey.Darken3);

            col.Item().Text($"{building.Neighborhood}, {building.City}")
                .FontSize(11).FontColor(Colors.Grey.Darken1);

            col.Item().PaddingTop(8).Text(text =>
            {
                text.Span("Period: ").SemiBold();
                text.Span($"{start.ToString("dd.MM.yyyy.", SrLatin)} — {end.ToString("dd.MM.yyyy.", SrLatin)}");
            });

            col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeContent(IContainer container, IReadOnlyList<AssemblyDetailDto> assemblies)
    {
        container.PaddingVertical(10).Column(col =>
        {
            col.Spacing(16);

            if (assemblies.Count == 0)
            {
                col.Item().PaddingTop(20).AlignCenter().Text("Nema skupština u izabranom periodu.")
                    .FontSize(12).Italic().FontColor(Colors.Grey.Darken1);
                return;
            }

            col.Item().Text($"Ukupno skupština: {assemblies.Count}")
                .FontSize(11).FontColor(Colors.Grey.Darken2);

            foreach (AssemblyDetailDto assembly in assemblies)
                col.Item().Element(c => ComposeAssembly(c, assembly));
        });
    }

    private static void ComposeAssembly(IContainer container, AssemblyDetailDto a)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Text(a.ScheduledAt.ToString("dddd, dd.MM.yyyy. - HH:mm", SrLatin))
                    .FontSize(13).Bold();
                row.AutoItem().Text(a.StatusDisplay)
                    .FontSize(11).FontColor(Colors.Grey.Darken2);
            });

            // Teme
            if (a.Topics.Count > 0)
            {
                col.Item().PaddingTop(8).Text("Dnevni red:").SemiBold().FontSize(11);
                int idx = 1;
                foreach (AssemblyTopicDto topic in a.Topics)
                {
                    string mark = topic.IsAdditional ? " (dodatna)" : "";
                    col.Item().PaddingLeft(8).Text($"{idx++}. {topic.Text}{mark}")
                        .FontSize(11);
                }
            }

            // Prisustvo
            if (a.Attendances.Count > 0)
            {
                col.Item().PaddingTop(10).Text("Prisustvo po stanovima:").SemiBold().FontSize(11);

                int confirmed = 0;
                foreach (var att in a.Attendances) if (att.IsConfirmed) confirmed++;

                col.Item().PaddingLeft(8).Text($"Potvrđeno: {confirmed} / {a.Attendances.Count}")
                    .FontSize(11).FontColor(Colors.Grey.Darken2);

                col.Item().PaddingLeft(8).PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    foreach (AttendanceItemDto att in a.Attendances)
                    {
                        string mark = att.IsConfirmed ? "✓" : "—";
                        table.Cell().Padding(2).Text($"Stan {att.ApartmentNumber}: {mark}")
                            .FontSize(10);
                    }
                });
            }
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span($"Izveštaj generisan {DateTime.Now.ToString("dd.MM.yyyy. HH:mm", SrLatin)}")
                .FontSize(9).FontColor(Colors.Grey.Darken1);
            text.Span("  •  CommunityHub")
                .FontSize(9).FontColor(Colors.Grey.Darken1);
        });
    }
}
