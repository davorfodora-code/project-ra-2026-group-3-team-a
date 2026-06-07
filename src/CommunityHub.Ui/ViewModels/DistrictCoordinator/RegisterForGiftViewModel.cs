using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class RegisterForGiftViewModel : ViewModelBase
{
    private readonly CityGiftService _service;
    private readonly long            _giftId;
    private readonly long            _coordinatorId;
    private DistrictDto?             _selectedDistrict;
    private string                   _errorText = string.Empty;

    public RegisterForGiftViewModel(long giftId, string giftName, string giftAmountDisplay, long coordinatorId, List<DistrictDto> districts)
    {
        _service          = new CityGiftService(Injector.Get<ICityGiftRepository>());
        _giftId           = giftId;
        _coordinatorId    = coordinatorId;
        GiftName          = giftName;
        GiftAmountDisplay = giftAmountDisplay;

        Districts = new ObservableCollection<DistrictDto>(districts);
        if (Districts.Count > 0) SelectedDistrict = Districts[0];

        ConfirmCommand = new RelayCommand(_ => TryRegister());
        CancelCommand  = new RelayCommand(_ => CancelRequested?.Invoke());
        CloseCommand   = new RelayCommand(_ => CancelRequested?.Invoke());
    }

    public string GiftName            { get; }
    public string GiftAmountDisplay   { get; }
    public bool   HasMultipleDistricts => Districts.Count > 1;

    public ObservableCollection<DistrictDto> Districts { get; }

    public DistrictDto? SelectedDistrict
    {
        get => _selectedDistrict;
        set => SetProperty(ref _selectedDistrict, value);
    }

    public string ErrorText
    {
        get => _errorText;
        private set { SetProperty(ref _errorText, value); OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(_errorText);

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand  { get; }
    public ICommand CloseCommand   { get; }

    public event Action? CancelRequested;
    public event Action? Registered;

    private void TryRegister()
    {
        if (SelectedDistrict == null) { ErrorText = "Izaberite kvart."; return; }
        try
        {
            _service.Register(_giftId, SelectedDistrict.Id, _coordinatorId);
            Registered?.Invoke();
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            ErrorText = msg.Contains("unique") || msg.Contains("duplicate") || msg.Contains("23505")
                ? "Vec ste prijavljeni za ovaj poklon."
                : $"Greška: {msg}";
        }
    }
}
