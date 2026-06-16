using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public static class PdfReportGenerator
{
    public static void Generate(
        string filePath,
        string districtName,
        DateTime from,
        DateTime to,
        (decimal TotalDonations, decimal TotalExpenses, decimal Saldo) summary,
        List<BudgetCategoryDto> cats)
    {
        var contentBytes = Encoding.Latin1.GetBytes(BuildContent(districtName, from, to, summary, cats));
        WritePdf(filePath, contentBytes);
    }

    private static string BuildContent(
        string districtName,
        DateTime from,
        DateTime to,
        (decimal TotalDonations, decimal TotalExpenses, decimal Saldo) summary,
        List<BudgetCategoryDto> cats)
    {
        var sb = new StringBuilder();
        float y = 800f;
        const float left = 50f;
        const float col2 = 250f, col3 = 370f, col4 = 480f;

        sb.AppendLine("BT");

        // Title
        AddText(sb, "/F2 18 Tf", left, y, "FINANSIJSKI IZVESTAJ");
        y -= 28f;

        // Meta info
        AddText(sb, "/F1 11 Tf", left, y, $"Kvart: {S(districtName)}");
        y -= 16f;
        AddText(sb, "/F1 11 Tf", left, y, $"Period: {from:dd.MM.yyyy} - {to:dd.MM.yyyy}");
        y -= 16f;
        AddText(sb, "/F1 9 Tf", left, y, $"Generisano: {DateTime.Now:dd.MM.yyyy HH:mm}");
        y -= 30f;

        // Summary section
        AddText(sb, "/F2 13 Tf", left, y, "SUMARNI PREGLED");
        y -= 20f;
        AddText(sb, "/F1 11 Tf", left, y, $"Ukupno donacija:    {BudgetService.FormatRsd(summary.TotalDonations)}");
        y -= 16f;
        AddText(sb, "/F1 11 Tf", left, y, $"Ukupno troskova:    {BudgetService.FormatRsd(summary.TotalExpenses)}");
        y -= 16f;
        AddText(sb, "/F1 11 Tf", left, y, $"Saldo:              {BudgetService.FormatRsd(summary.Saldo)}");
        y -= 32f;

        // Categories header
        AddText(sb, "/F2 13 Tf", left, y, "KATEGORIJE BUDZETA");
        y -= 22f;

        // Table header
        AddText(sb, "/F2 10 Tf", left, y, "Kategorija");
        AddText(sb, "/F2 10 Tf", col2,  y, "Donacije");
        AddText(sb, "/F2 10 Tf", col3,  y, "Troskovi");
        AddText(sb, "/F2 10 Tf", col4,  y, "Saldo");
        y -= 14f;

        // Separator
        AddText(sb, "/F1 8 Tf", left, y, new string('-', 90));
        y -= 14f;

        // Category rows
        foreach (var cat in cats)
        {
            if (y < 80f) break;
            AddText(sb, "/F1 10 Tf", left, y, S(cat.Name));
            AddText(sb, "/F1 10 Tf", col2,  y, BudgetService.FormatRsd(cat.TotalDonations));
            AddText(sb, "/F1 10 Tf", col3,  y, BudgetService.FormatRsd(cat.TotalExpenses));
            AddText(sb, "/F1 10 Tf", col4,  y, BudgetService.FormatRsd(cat.Balance));
            y -= 14f;
        }

        // Footer
        AddText(sb, "/F1 8 Tf", left, 35f, "CommunityHub | Finansijski izvestaj kvarta");

        sb.AppendLine("ET");
        return sb.ToString();
    }

    private static void AddText(StringBuilder sb, string fontCmd, float x, float y, string text)
    {
        sb.AppendLine(fontCmd);
        // InvariantCulture is required — PDF syntax requires '.' as decimal separator
        sb.AppendLine(FormattableString.Invariant($"1 0 0 1 {x:F1} {y:F1} Tm"));
        sb.AppendLine($"({Esc(text)}) Tj");
    }

    private static string Esc(string s) => s
        .Replace("\\", "\\\\")
        .Replace("(", "\\(")
        .Replace(")", "\\)");

    // Replace Serbian/Latin-extended chars not in WinAnsiEncoding
    private static string S(string s) => s
        .Replace("š", "s").Replace("Š", "S")
        .Replace("ž", "z").Replace("Ž", "Z")
        .Replace("ć", "c").Replace("Ć", "C")
        .Replace("č", "c").Replace("Č", "C")
        .Replace("đ", "dj").Replace("Đ", "Dj");

    private static void WritePdf(string filePath, byte[] content)
    {
        using var ms = new MemoryStream();
        var off = new long[7];

        W(ms, "%PDF-1.4\n");

        off[1] = ms.Position;
        W(ms, "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        off[2] = ms.Position;
        W(ms, "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

        off[3] = ms.Position;
        W(ms, "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842]\n" +
              "   /Contents 4 0 R\n" +
              "   /Resources << /Font << /F1 5 0 R /F2 6 0 R >> >> >>\nendobj\n");

        off[4] = ms.Position;
        W(ms, $"4 0 obj\n<< /Length {content.Length} >>\nstream\n");
        ms.Write(content);
        W(ms, "\nendstream\nendobj\n");

        off[5] = ms.Position;
        W(ms, "5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica" +
              " /Encoding /WinAnsiEncoding >>\nendobj\n");

        off[6] = ms.Position;
        W(ms, "6 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold" +
              " /Encoding /WinAnsiEncoding >>\nendobj\n");

        long xrefPos = ms.Position;
        W(ms, "xref\n0 7\n");
        W(ms, "0000000000 65535 f \n");
        for (int i = 1; i <= 6; i++)
            W(ms, $"{off[i]:D10} 00000 n \n");

        W(ms, $"trailer\n<< /Size 7 /Root 1 0 R >>\nstartxref\n{xrefPos}\n%%EOF\n");

        File.WriteAllBytes(filePath, ms.ToArray());
    }

    private static void W(Stream s, string text)
        => s.Write(Encoding.Latin1.GetBytes(text));
}
