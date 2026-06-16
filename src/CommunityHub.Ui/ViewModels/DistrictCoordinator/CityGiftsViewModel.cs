using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class CityGiftsViewModel : ViewModelBase
{
    private readonly CityGiftService              _giftService;
    private readonly DistrictAccessRequestService _districtService;
    private readonly long                         _coordinatorId;

    private ViewModelBase? _activeDialog;
    private string         _notificationText = string.Empty;
    private bool           _hasNotification;

    public CityGiftsViewModel(long coordinatorId)
    {
        _coordinatorId   = coordinatorId;
        _giftService     = new CityGiftService(Injector.Get<ICityGiftRepository>());
        _districtService = new DistrictAccessRequestService(
            Injector.Get<IDistrictAccessRequestRepository>(),
            Injector.Get<IDistrictRepository>());

        Gifts                  = new ObservableCollection<CityGiftRowViewModel>();
        RefreshCommand         = new RelayCommand(_ => LoadGifts());
        BackToFinancesCommand  = new RelayCommand(_ => BackToFinancesRequested?.Invoke());
        CancelDialogCommand              = new RelayCommand(_ => CloseDialog(), _ => HasDialog);
        RegisterFirstAvailableGiftCommand = new RelayCommand(
            _ => Gifts.FirstOrDefault(g => g.CanStillRegister)?.RegisterCommand.Execute(null),
            _ => Gifts.Any(g => g.CanStillRegister));
    }

    public ObservableCollection<CityGiftRowViewModel> Gifts { get; }
    public ICommand RefreshCommand                   { get; }
    public ICommand BackToFinancesCommand            { get; }
    public ICommand CancelDialogCommand              { get; }
    public ICommand RegisterFirstAvailableGiftCommand { get; }

    public event Action? BackToFinancesRequested;

    public ViewModelBase? ActiveDialog
    {
        get => _activeDialog;
        private set
        {
            if (SetProperty(ref _activeDialog, value))
            {
                OnPropertyChanged(nameof(HasDialog));
                (CancelDialogCommand as RelayCommand)?.RaiseCanExecuteChanged();
                ActiveDialogChanged?.Invoke(value);
            }
        }
    }
    public bool HasDialog => _activeDialog != null;

    public event Action<ViewModelBase?>? ActiveDialogChanged;

    public string NotificationText { get => _notificationText; private set => SetProperty(ref _notificationText, value); }
    public bool   HasNotification  { get => _hasNotification;  private set => SetProperty(ref _hasNotification,  value); }



    public void EnsureLoaded()
    {
        if (Gifts.Count == 0) LoadGifts();
    }

    private void LoadGifts()
    {
        try
        {
            _giftService.AwardPendingGifts();

            var allDistricts = _districtService.GetDistrictsByCoordinatorId(_coordinatorId);
            var dtos         = _giftService.GetAllGifts(_coordinatorId);

            Gifts.Clear();
            foreach (var dto in dtos)
            {
                var registeredIds = dto.IsOpen && dto.HasRegistered
                    ? _giftService.GetRegisteredDistrictIds(dto.Id, _coordinatorId)
                    : new List<long>();

                var available        = allDistricts.Where(d => !registeredIds.Contains(d.Id)).ToList();
                bool canStillRegister = dto.IsOpen && available.Count > 0;

                Gifts.Add(new CityGiftRowViewModel(
                    dto,
                    registeredIds.Count,
                    allDistricts.Count,
                    canStillRegister,
                    row => OpenRegisterDialog(row, available)));
            }
        }
        catch (Exception ex)
        {
            ShowNotification($"Greška pri učitavanju: {ex.Message}");
        }
    }

    private void OpenRegisterDialog(CityGiftRowViewModel row, List<DistrictDto> availableDistricts)
    {
        try
        {
            if (availableDistricts.Count == 0) { ShowNotification("Nema dostupnih kvartova za prijavu."); return; }

            var dialog = new RegisterForGiftViewModel(row.Id, row.Name, row.AmountDisplay, _coordinatorId, availableDistricts);
            dialog.CancelRequested += CloseDialog;
            dialog.Registered      += OnRegistered;
            ActiveDialog = dialog;
        }
        catch (Exception ex)
        {
            ShowNotification($"Greška: {ex.Message}");
        }
    }

    private void CloseDialog()
    {
        if (_activeDialog is RegisterForGiftViewModel d)
        {
            d.CancelRequested -= CloseDialog;
            d.Registered      -= OnRegistered;
        }
        ActiveDialog = null;
    }

    private void OnRegistered()
    {
        CloseDialog();
        LoadGifts();
        ShowNotification("Uspešno ste se prijavili za poklon.");
    }

    private void ShowNotification(string message)
    {
        NotificationText = message;
        HasNotification  = true;

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.5) };
        timer.Tick += (_, _) =>
        {
            HasNotification = false;
            timer.Stop();
        };
        timer.Start();
    }
}
