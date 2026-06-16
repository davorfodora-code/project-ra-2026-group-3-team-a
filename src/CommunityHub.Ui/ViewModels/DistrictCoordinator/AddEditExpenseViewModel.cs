using System;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class AddEditExpenseViewModel : ViewModelBase
{
    private readonly BudgetService _service;
    private readonly long _districtId;
    private readonly long _categoryId;
    private readonly long? _editId;

    private string    _amountText      = string.Empty;
    private string    _description     = string.Empty;
    private DateTime? _selectedDate;
    private string    _amountError     = string.Empty;
    private string    _descError       = string.Empty;
    private bool      _showLeaveConfirm;
    private bool      _amountTouched;
    private bool      _descTouched;

    public AddEditExpenseViewModel(long districtId, long categoryId, ExpenseDto? existing = null)
    {
        _service    = new BudgetService(Injector.Get<IBudgetRepository>());
        _districtId = districtId;
        _categoryId = categoryId;
        IsEdit      = existing != null;
        _editId     = existing?.Id;

        if (existing != null)
        {
            _amountText  = existing.Amount.ToString("F0");
            _description = existing.Description;
            _selectedDate = existing.ExpenseDate;
        }

        SaveCommand   = new RelayCommand(_ => TrySave());
        CancelCommand = new RelayCommand(_ => TryCancel());
        CloseCommand  = new RelayCommand(_ => TryCancel());
        ConfirmLeaveCommand = new RelayCommand(_ => { ShowLeaveConfirm = false; CancelRequested?.Invoke(); });
        StayCommand   = new RelayCommand(_ => ShowLeaveConfirm = false);
    }

    public bool IsEdit { get; }
    public string Title => IsEdit ? "Uredi trosak" : "Novi trosak";
    public string SaveLabel => IsEdit ? "Sacuvaj izmene" : "Sacuvaj";
    public string CancelLabel => IsEdit ? "Otkazi izmene" : "Otkazi";

    public string AmountText
    {
        get => _amountText;
        set
        {
            if (!_amountTouched) { _amountTouched = true; OnPropertyChanged(nameof(ShowAmountFeedback)); }
            SetProperty(ref _amountText, value);
            ValidateAmount();
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (!_descTouched) { _descTouched = true; OnPropertyChanged(nameof(ShowDescFeedback)); }
            SetProperty(ref _description, value);
            ValidateDescription();
        }
    }

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set => SetProperty(ref _selectedDate, value);
    }

    public string AmountError
    {
        get => _amountError;
        private set { SetProperty(ref _amountError, value); OnPropertyChanged(nameof(HasAmountError)); }
    }

    public string DescError
    {
        get => _descError;
        private set { SetProperty(ref _descError, value); OnPropertyChanged(nameof(HasDescError)); }
    }

    public bool HasAmountError      => !string.IsNullOrEmpty(_amountError);
    public bool HasDescError        => !string.IsNullOrEmpty(_descError);
    public bool ShowAmountFeedback  => _amountTouched;
    public bool ShowDescFeedback    => _descTouched;

    public bool ShowLeaveConfirm
    {
        get => _showLeaveConfirm;
        set => SetProperty(ref _showLeaveConfirm, value);
    }

    public ICommand SaveCommand         { get; }
    public ICommand CancelCommand       { get; }
    public ICommand CloseCommand        { get; }
    public ICommand ConfirmLeaveCommand { get; }
    public ICommand StayCommand         { get; }

    public event Action?       CancelRequested;
    public event Action<long>? Saved;

    public bool HasUnsavedInput => !string.IsNullOrWhiteSpace(_amountText) ||
                                     !string.IsNullOrWhiteSpace(_description) ||
                                     _selectedDate.HasValue;

    private void TrySave()
    {
        _amountTouched = true; OnPropertyChanged(nameof(ShowAmountFeedback));
        _descTouched   = true; OnPropertyChanged(nameof(ShowDescFeedback));
        ValidateAmount();
        ValidateDescription();
        if (HasAmountError || HasDescError) return;
        if (_selectedDate == null) { AmountError = "Izaberite datum."; return; }

        decimal amount = decimal.Parse(_amountText.Replace(".", "").Replace(",", ""));
        try
        {
            long id;
            if (IsEdit && _editId.HasValue)
            {
                _service.UpdateExpense(_editId.Value, amount, _description.Trim(), _selectedDate.Value);
                id = _editId.Value;
            }
            else
            {
                id = _service.AddExpense(_districtId, _categoryId, amount, _description.Trim(), _selectedDate.Value);
            }
            Saved?.Invoke(id);
        }
        catch (Exception ex)
        {
            AmountError = $"Greška: {ex.Message}";
        }
    }

    private void TryCancel()
    {
        if (HasUnsavedInput && !IsEdit)
            ShowLeaveConfirm = true;
        else
            CancelRequested?.Invoke();
    }

    private void ValidateAmount()
    {
        string raw = _amountText.Replace(".", "").Replace(",", "").Trim();
        if (string.IsNullOrWhiteSpace(raw)) { AmountError = "Iznos je obavezno polje."; return; }
        if (!decimal.TryParse(raw, out decimal val) || val <= 0) { AmountError = "Unesite ispravan pozitivan iznos."; return; }
        AmountError = string.Empty;
    }

    private void ValidateDescription()
    {
        if (string.IsNullOrWhiteSpace(_description)) { DescError = "Opis je obavezna stavka."; return; }
        if (_description.Trim().Length < 30) { DescError = "Opis mora imati najmanje 30 karaktera."; return; }
        DescError = string.Empty;
    }
}
