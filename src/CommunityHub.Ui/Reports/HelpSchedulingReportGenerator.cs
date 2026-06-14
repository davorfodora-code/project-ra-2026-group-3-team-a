using System;
using System.Collections.Generic;
using System.Globalization;
using CommunityHub.Application.Services.Tenant.Dto;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CommunityHub.Ui.Reports;

public static class HelpSchedulingReportGenerator
{
    private static readonly CultureInfo SrLatin = new CultureInfo("sr-Latn-RS");

    public static byte[] Generate(
        DateTime startDate,
        DateTime endDate,
        string   typeLabel,
        IReadOnlyList<ReportAppointmentDto> appointments)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(t => t.FontSize(11).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader(c, startDate, endDate, typeLabel));
                page.Content().Element(c => ComposeContent(c, appointments));
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, DateTime start, DateTime end, string typeLabel)
    {
        container.Column(col =>
        {
            col.Item().Text("Izveštaj o zakazanim terminima pomoći")
                .FontSize(20).Bold().FontColor(Colors.Black);

            col.Item().PaddingTop(4).Text(text =>
            {
                text.Span("Tip: ").SemiBold();
                text.Span(typeLabel);
                text.Span("   ");
                text.Span("Period: ").SemiBold();
                text.Span($"{start.ToString("dd.MM.yyyy.", SrLatin)} - {end.ToString("dd.MM.yyyy.", SrLatin)}");
            });

            col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeContent(IContainer container, IReadOnlyList<ReportAppointmentDto> appointments)
    {
        container.PaddingVertical(10).Column(col =>
        {
            if (appointments.Count == 0)
            {
                col.Item().PaddingTop(24).AlignCenter()
                    .Text("Nema zakazanih termina u izabranom periodu.")
                    .FontSize(12).Italic().FontColor(Colors.Grey.Darken1);
                return;
            }

            col.Item().PaddingBottom(8)
                .Text($"Ukupno termina: {appointments.Count}")
                .FontSize(11).FontColor(Colors.Grey.Darken2);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(3);  // Termin
                    cols.RelativeColumn(2);  // Osoba
                    cols.RelativeColumn(2);  // Moja uloga
                    cols.RelativeColumn(2);  // Kategorija
                    cols.RelativeColumn(2);  // Izvor
                    cols.RelativeColumn(4);  // Opis
                });

                table.Header(header =>
                {
                    string[] labels = ["Termin", "Osoba", "Moja uloga", "Kategorija", "Izvor", "Opis"];
                    foreach (string label in labels)
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6)
                              .Text(label).Bold().FontSize(10);
                });

                bool alt = false;
                foreach (ReportAppointmentDto appt in appointments)
                {
                    string bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                    alt = !alt;

                    table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                         .Text(appt.SlotDisplay).FontSize(10);
                    table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                         .Text(appt.OtherPersonName).FontSize(10);
                    table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                         .Text(appt.MyRoleDisplay).FontSize(10);
                    table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                         .Text(appt.Category).FontSize(10);
                    table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                         .Text(appt.Source).FontSize(10);
                    table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                         .Text(appt.AdDescription).FontSize(10).FontColor(Colors.Grey.Darken2);
                }
            });
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
