using System;
using System.Globalization;

namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class CityGiftDto
{
    public long     Id                 { get; set; }
    public string   Name               { get; set; } = string.Empty;
    public decimal  Amount             { get; set; }
    public DateTime Deadline           { get; set; }
    public bool     Awarded            { get; set; }
    public long?    WinnerDistrictId   { get; set; }
    public string?  WinnerDistrictName { get; set; }
    public bool     HasRegistered      { get; set; }

    public string AmountDisplay   => Amount.ToString("N0", CultureInfo.InvariantCulture).Replace(",", ".") + " RSD";
    public string DeadlineDisplay => Deadline.ToString("d.M.yyyy");
    public bool   IsOpen          => !Awarded && Deadline > DateTime.Now;
    public bool   IsPendingAward  => !Awarded && Deadline <= DateTime.Now;

    public int    DaysRemaining => Math.Max(0, (Deadline.Date - DateTime.Today).Days);
    public string DaysRemainingDisplay => DaysRemaining == 0
        ? "Rok istice danas"
        : $"Rok istice za {DaysRemaining} {(DaysRemaining == 1 ? "dan" : "dana")}";
}
