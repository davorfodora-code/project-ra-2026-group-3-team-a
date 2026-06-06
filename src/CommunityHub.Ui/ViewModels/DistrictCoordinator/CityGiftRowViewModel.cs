using System;
using System.Windows.Input;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class CityGiftRowViewModel : ViewModelBase
{
    public CityGiftRowViewModel(
        CityGiftDto dto,
        int registeredCount,
        int totalDistricts,
        bool canStillRegister,
        Action<CityGiftRowViewModel> onRegister)
    {
        Id                   = dto.Id;
        Name                 = dto.Name;
        AmountDisplay        = dto.AmountDisplay;
        DeadlineDisplay      = dto.DeadlineDisplay;
        DaysRemainingDisplay = dto.DaysRemainingDisplay;
        Awarded              = dto.Awarded;
        WinnerDistrictName   = dto.WinnerDistrictName;
        IsOpen               = dto.IsOpen;
        RegisteredCount      = registeredCount;
        TotalDistricts       = totalDistricts;
        CanStillRegister     = canStillRegister;

        RegisterCommand = new RelayCommand(_ => onRegister(this), _ => canStillRegister);
    }

    public long    Id                   { get; }
    public string  Name                 { get; }
    public string  AmountDisplay        { get; }
    public string  DeadlineDisplay      { get; }
    public string  DaysRemainingDisplay { get; }
    public bool    Awarded              { get; }
    public string? WinnerDistrictName   { get; }
    public bool    IsOpen               { get; }
    public int     RegisteredCount      { get; }
    public int     TotalDistricts       { get; }
    public bool    CanStillRegister     { get; }

    public string BadgeText => IsOpen
        ? DaysRemainingDisplay
        : (Awarded
            ? (WinnerDistrictName != null ? $"Dodeljeno: {WinnerDistrictName}" : "Nema prijava")
            : "Rok istekao");

    public bool ShowRegisterButton  => IsOpen && CanStillRegister;
    public bool ShowAllRegistered   => IsOpen && !CanStillRegister && RegisteredCount > 0;
    public bool ShowClosedStatus    => !IsOpen;

    public string RegisterButtonText => RegisteredCount == 0
        ? "Prijavi se"
        : $"Prijavi još  ({RegisteredCount}/{TotalDistricts})";

    public string AllRegisteredText => $"✓  Svi kvartovi prijavljeni  ({RegisteredCount}/{TotalDistricts})";

    public string ClosedStatusText => Awarded
        ? (WinnerDistrictName != null ? $"Dodeljeno: {WinnerDistrictName}" : "Nema prijava")
        : "Rok istekao";

    public ICommand RegisterCommand { get; }
}
