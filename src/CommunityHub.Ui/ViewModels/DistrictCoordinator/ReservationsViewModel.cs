using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class ReservationsViewModel : ViewModelBase
{
    private readonly CityObjectService         _cityObjectService;
    private readonly DistrictAccessRequestService _districtService;
    private readonly Action<ViewModelBase?>? _showOverlay;
    private readonly long _coordinatorId;
    private DistrictDto? _selectedDistrict;
    private ReservationDialogViewModel? _activeDialog;
    private string _notificationText = string.Empty;
    private bool _hasNotification;

    public ReservationsViewModel(long coordinatorId, Action<ViewModelBase?>? showOverlay = null)
    {
        _coordinatorId     = coordinatorId;
        _showOverlay       = showOverlay;
        _cityObjectService = new CityObjectService(Injector.Get<ICityObjectRepository>());
        _districtService   = new DistrictAccessRequestService(
            Injector.Get<IDistrictAccessRequestRepository>(),
            Injector.Get<IDistrictRepository>());

        Districts   = new ObservableCollection<DistrictDto>();
        CityObjects = new ObservableCollection<CityObjectCardViewModel>();

        RefreshCommand              = new RelayCommand(_ => { if (_selectedDistrict != null) LoadCityObjects(); });
        CancelDialogCommand         = new RelayCommand(_ => CloseDialog(), _ => HasDialog);
        OpenFirstReservationCommand = new RelayCommand(
            _ => { if (CityObjects.Count > 0 && _selectedDistrict != null) OpenDialog(CityObjects[0]); },
            _ => CityObjects.Count > 0 && _selectedDistrict != null && !HasDialog);
    }

    public ObservableCollection<DistrictDto>             Districts   { get; }
    public ObservableCollection<CityObjectCardViewModel> CityObjects { get; }
    public ICommand RefreshCommand               { get; }
    public ICommand CancelDialogCommand          { get; }
    public ICommand OpenFirstReservationCommand  { get; }

    public DistrictDto? SelectedDistrict
    {
        get => _selectedDistrict;
        set
        {
            if (SetProperty(ref _selectedDistrict, value))
                LoadCityObjects();
        }
    }

    public ReservationDialogViewModel? ActiveDialog
    {
        get => _activeDialog;
        private set
        {
            if (SetProperty(ref _activeDialog, value))
        {
            OnPropertyChanged(nameof(HasDialog));
            RaiseDialogCanExecute();
        }
        }
    }

    public bool HasDialog => _activeDialog != null;

    private void RaiseDialogCanExecute()
    {
        (CancelDialogCommand        as RelayCommand)?.RaiseCanExecuteChanged();
        (OpenFirstReservationCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    public string NotificationText
    {
        get => _notificationText;
        private set => SetProperty(ref _notificationText, value);
    }

    public bool HasNotification
    {
        get => _hasNotification;
        private set => SetProperty(ref _hasNotification, value);
    }

    public void RefreshDistricts()
    {
        if (Districts.Count == 0) return;
        var currentId = _selectedDistrict?.Id;
        Districts.Clear();
        try
        {
            var districts = _districtService.GetDistrictsByCoordinatorId(_coordinatorId);
            foreach (var d in districts) Districts.Add(d);
            if (currentId.HasValue)
                foreach (var d in Districts)
                    if (d.Id == currentId.Value) { SelectedDistrict = d; return; }
            if (Districts.Count > 0) SelectedDistrict = Districts[0];
        }
        catch (Exception ex)
        {
            ShowNotification($"Greška pri osvežavanju kvartova: {ex.Message}");
        }
    }

    public void EnsureLoaded()
    {
        if (Districts.Count > 0) return;
        try
        {
            var districts = _districtService.GetDistrictsByCoordinatorId(_coordinatorId);
            foreach (var d in districts)
                Districts.Add(d);

            if (Districts.Count > 0)
                SelectedDistrict = Districts[0];
            else
                LoadCityObjects();
        }
        catch (Exception ex)
        {
            ShowNotification($"Greška pri učitavanju kvartova: {ex.Message}");
        }
    }

    private void LoadCityObjects()
    {
        try
        {
            CityObjects.Clear();
            var objects = _cityObjectService.GetAllWithVotes();
            foreach (var dto in objects)
                CityObjects.Add(new CityObjectCardViewModel(dto, OpenDialog));
        }
        catch (Exception ex)
        {
            ShowNotification($"Greška pri učitavanju objekata: {ex.Message}");
        }
    }

    private void OpenDialog(CityObjectCardViewModel card)
    {
        if (_selectedDistrict == null)
        {
            ShowNotification("Izaberite kvart pre rezervacije.");
            return;
        }

        var dialog = new ReservationDialogViewModel(card.Id, card.Name, _selectedDistrict.Id);
        dialog.CancelRequested      += CloseDialog;
        dialog.ReservationCompleted += OnReservationCompleted;
        ActiveDialog = dialog;
        _showOverlay?.Invoke(dialog);
    }

    private void CloseDialog()
    {
        if (_activeDialog == null) return;
        _activeDialog.CancelRequested      -= CloseDialog;
        _activeDialog.ReservationCompleted -= OnReservationCompleted;
        ActiveDialog = null;
        _showOverlay?.Invoke(null);
    }

    private void OnReservationCompleted()
    {
        CloseDialog();
        LoadCityObjects();
        ShowNotification("Rezervacija uspešno izvršena.");
    }

    private void ShowNotification(string message)
    {
        NotificationText = message;
        HasNotification  = true;

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.5) };
        timer.Tick += (_, _) => { HasNotification = false; timer.Stop(); };
        timer.Start();
    }
}
