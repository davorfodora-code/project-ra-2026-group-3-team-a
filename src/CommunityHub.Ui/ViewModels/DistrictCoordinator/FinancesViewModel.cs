using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class PeriodOption
{
    public string   Label { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End   { get; set; }
}

public partial class FinancesViewModel : ViewModelBase
{
    internal readonly BudgetService                _budgetService;
    internal readonly DistrictAccessRequestService _districtService;
    internal readonly long                         _coordinatorId;

    private DistrictDto?  _selectedDistrict;
    private PeriodOption? _selectedPeriod;
    private string _saldoDisplay     = "—";
    private string _donationsDisplay = "—";
    private string _expensesDisplay  = "—";
    private string _loadError        = string.Empty;

    internal bool   _isDetailView;
    internal long   _detailCategoryId;
    private string _detailCategoryName   = string.Empty;
    private string _detailBalance        = "—";
    private string _detailTotalDonations = "—";
    private string _detailTotalExpenses  = "—";
    private string _fullDescriptionText  = string.Empty;
    private bool   _showFullDescription;

    internal List<DonationDto> _allDonations = new();
    internal List<ExpenseDto>  _allExpenses  = new();
    internal const int PageSize = 10;
    internal int _donPage = 1;
    internal int _expPage = 1;
    internal string _donSort = "Datum ↑";
    internal string _expSort = "Datum ↑";

    private ViewModelBase? _activeDialog;
    private string _notificationText = string.Empty;
    private bool   _hasNotification;

    public FinancesViewModel(long coordinatorId)
    {
        _coordinatorId   = coordinatorId;
        _budgetService   = new BudgetService(Injector.Get<IBudgetRepository>());
        _districtService = new DistrictAccessRequestService(
            Injector.Get<IDistrictAccessRequestRepository>(),
            Injector.Get<IDistrictRepository>());

        Districts     = new ObservableCollection<DistrictDto>();
        Periods       = new ObservableCollection<PeriodOption>();
        CategoryCards = new ObservableCollection<BudgetCategoryCardViewModel>();
        Donations     = new ObservableCollection<DonationDto>();
        Expenses      = new ObservableCollection<ExpenseDto>();
        SortOptions   = new ObservableCollection<string> { "Datum ↑", "Datum ↓" };

        BackToHomeCommand    = new RelayCommand(_ => BackToHomeRequested?.Invoke());
        BackToGridCommand    = new RelayCommand(_ => GoBackToGrid());
        AddExpenseCommand    = new RelayCommand(_ => OpenAddExpense());
        AddDonationCommand   = new RelayCommand(_ => OpenAddDonation());
        EditExpenseCommand   = new RelayCommand(p => { if (p is ExpenseDto e) OpenEditExpense(e); });
        DeleteExpenseCommand = new RelayCommand(p => { if (p is ExpenseDto e) DoDeleteExpense(e); });
        ShowFullDescCommand  = new RelayCommand(p => ShowFullDesc(p?.ToString()));
        HideFullDescCommand  = new RelayCommand(_ => ShowFullDescription = false);
        OpenReportCommand    = new RelayCommand(_ => OpenReportDialog());
        DonPrevCommand = new RelayCommand(_ => { if (_donPage > 1)            { _donPage--; RefreshDonationsPage(); } });
        DonNextCommand = new RelayCommand(_ => { if (_donPage < DonTotalPages) { _donPage++; RefreshDonationsPage(); } });
        ExpPrevCommand = new RelayCommand(_ => { if (_expPage > 1)            { _expPage--; RefreshExpensesPage(); } });
        ExpNextCommand = new RelayCommand(_ => { if (_expPage < ExpTotalPages) { _expPage++; RefreshExpensesPage(); } });

        BuildPeriods();
    }

    public ObservableCollection<DistrictDto>                Districts     { get; }
    public ObservableCollection<PeriodOption>               Periods       { get; }
    public ObservableCollection<BudgetCategoryCardViewModel> CategoryCards { get; }
    public ObservableCollection<string>                     SortOptions   { get; }
    public ObservableCollection<DonationDto>                Donations     { get; }
    public ObservableCollection<ExpenseDto>                 Expenses      { get; }

    public DistrictDto? SelectedDistrict
    {
        get => _selectedDistrict;
        set { if (SetProperty(ref _selectedDistrict, value) && value != null) LoadGridData(); }
    }
    public PeriodOption? SelectedPeriod
    {
        get => _selectedPeriod;
        set { if (SetProperty(ref _selectedPeriod, value)) LoadGridData(); }
    }
    public string SaldoDisplay     { get => _saldoDisplay;     private set => SetProperty(ref _saldoDisplay,     value); }
    public string DonationsDisplay { get => _donationsDisplay; private set => SetProperty(ref _donationsDisplay, value); }
    public string ExpensesDisplay  { get => _expensesDisplay;  private set => SetProperty(ref _expensesDisplay,  value); }
    public string LoadError        { get => _loadError;        private set { SetProperty(ref _loadError, value); OnPropertyChanged(nameof(HasLoadError)); } }
    public bool   HasLoadError     => !string.IsNullOrEmpty(_loadError);

    public bool IsDetailView { get => _isDetailView; private set { if (SetProperty(ref _isDetailView, value)) OnPropertyChanged(nameof(IsGridView)); } }
    public bool IsGridView   => !_isDetailView;
    public string DetailCategoryName   { get => _detailCategoryName;   private set => SetProperty(ref _detailCategoryName,   value); }
    public string DetailBalance        { get => _detailBalance;        private set => SetProperty(ref _detailBalance,        value); }
    public string DetailTotalDonations { get => _detailTotalDonations; private set => SetProperty(ref _detailTotalDonations, value); }
    public string DetailTotalExpenses  { get => _detailTotalExpenses;  private set => SetProperty(ref _detailTotalExpenses,  value); }
    public string DonSort { get => _donSort; set { if (SetProperty(ref _donSort, value)) SortAndPage(); } }
    public string ExpSort { get => _expSort; set { if (SetProperty(ref _expSort, value)) SortAndPage(); } }
    public string DonPageLabel  => $"{_donPage}/{DonTotalPages}";
    public string ExpPageLabel  => $"{_expPage}/{ExpTotalPages}";
    private int DonTotalPages => Math.Max(1, (int)Math.Ceiling(_allDonations.Count / (double)PageSize));
    private int ExpTotalPages => Math.Max(1, (int)Math.Ceiling(_allExpenses.Count  / (double)PageSize));
    public string FullDescriptionText { get => _fullDescriptionText; private set => SetProperty(ref _fullDescriptionText, value); }
    public bool   ShowFullDescription { get => _showFullDescription; private set => SetProperty(ref _showFullDescription, value); }

    public ViewModelBase? ActiveDialog  { get => _activeDialog;      private set { if (SetProperty(ref _activeDialog, value)) OnPropertyChanged(nameof(HasDialog)); } }
    public bool           HasDialog     => _activeDialog != null;
    public string NotificationText { get => _notificationText; private set => SetProperty(ref _notificationText, value); }
    public bool   HasNotification  { get => _hasNotification;  private set => SetProperty(ref _hasNotification,  value); }

    public ICommand BackToHomeCommand    { get; }
    public ICommand BackToGridCommand    { get; }
    public ICommand AddExpenseCommand    { get; }
    public ICommand AddDonationCommand   { get; }
    public ICommand EditExpenseCommand   { get; }
    public ICommand DeleteExpenseCommand { get; }
    public ICommand ShowFullDescCommand  { get; }
    public ICommand HideFullDescCommand  { get; }
    public ICommand OpenReportCommand    { get; }
    public ICommand DonPrevCommand       { get; }
    public ICommand DonNextCommand       { get; }
    public ICommand ExpPrevCommand       { get; }
    public ICommand ExpNextCommand       { get; }
    public event Action? BackToHomeRequested;
}
