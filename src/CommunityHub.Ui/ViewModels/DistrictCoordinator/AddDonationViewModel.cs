using System;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class AddDonationViewModel : ViewModelBase
{
    private readonly BudgetService _service;
    private readonly long _districtId;
    private readonly long _categoryId;
    private readonly long _coordinatorId;

    private string    _amountText   = string.Empty;
    private DateTime? _selectedDate = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);
    private string    _amountError  = string.Empty;

    public AddDonationViewModel(long districtId, long categoryId, long coordinatorId)
    {
        _service       = new BudgetService(Injector.Get<IBudgetRepository>());
        _districtId    = districtId;
        _categoryId    = categoryId;
        _coordinatorId = coordinatorId;

        SaveCommand   = new RelayCommand(_ => TrySave());
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke());
        CloseCommand  = new RelayCommand(_ => CancelRequested?.Invoke());
    }

    public string AmountText
    {
        get => _amountText;
        set { SetProperty(ref _amountText, value); ValidateAmount(); }
    }

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set => SetProperty(ref _selectedDate, value.HasValue
            ? DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified)
            : (DateTime?)null);
    }

    public string AmountError
    {
        get => _amountError;
        private set { SetProperty(ref _amountError, value); OnPropertyChanged(nameof(HasAmountError)); }
    }

    public bool HasAmountError   => !string.IsNullOrEmpty(_amountError);
    public bool AmountIsValid    => string.IsNullOrEmpty(_amountError) && !string.IsNullOrWhiteSpace(_amountText);

    public ICommand SaveCommand   { get; }
    public ICommand CancelCommand { get; }
    public ICommand CloseCommand  { get; }

    public event Action? CancelRequested;
    public event Action? Saved;

    private void TrySave()
    {
        ValidateAmount();
        if (HasAmountError) return;
        if (_selectedDate == null) { AmountError = "Izaberite datum."; return; }

        string raw = _amountText.Replace(".", "").Replace(",", "").Trim();
        decimal amount = decimal.Parse(raw);
        try
        {
            _service.AddDonation(_districtId, _categoryId, _coordinatorId, amount, _selectedDate.Value);
            Saved?.Invoke();
        }
        catch (Exception ex)
        {
            AmountError = $"Greška: {ex.Message}";
        }
    }

    private void ValidateAmount()
    {
        string raw = _amountText.Replace(".", "").Replace(",", "").Trim();
        if (string.IsNullOrWhiteSpace(raw)) { AmountError = "Iznos je obavezno polje."; return; }
        if (!decimal.TryParse(raw, out decimal val) || val <= 0) { AmountError = "Unesite ispravan pozitivan iznos."; return; }
        AmountError = string.Empty;
    }
}
