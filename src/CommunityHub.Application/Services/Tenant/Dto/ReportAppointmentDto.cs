using System.Globalization;

namespace CommunityHub.Application.Services.Tenant.Dto;

public class ReportAppointmentDto
{
    private static readonly CultureInfo SrLatin = new CultureInfo("sr-Latn-RS");

    public DateTime SlotStart       { get; set; }
    public DateTime SlotEnd         { get; set; }
    public string OtherPersonName   { get; set; } = string.Empty;
    public bool IAmOffering         { get; set; }
    public string Category          { get; set; } = string.Empty;
    public string AdDescription     { get; set; } = string.Empty;
    public string Source            { get; set; } = string.Empty;

    public string MyRoleDisplay =>
        IAmOffering ? "Nudim pomoć" : "Tražim pomoć";

    public string SlotDisplay =>
        $"{SlotStart.ToString("dd.MM.yyyy. HH:mm", SrLatin)} – {SlotEnd.ToString("HH:mm", SrLatin)}";
}
