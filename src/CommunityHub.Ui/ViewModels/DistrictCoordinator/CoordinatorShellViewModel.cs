using System;
using System.Windows.Input;
using CommunityHub.Application.Services;
using CommunityHub.Ui.Settings;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class CoordinatorShellViewModel : ViewModelBase
{
    public const string TabHome = "home", TabRequests = "requests", TabStats = "stats", TabForum = "forum", TabReservations = "reservations", TabReviews = "reviews", TabFinances = "finances", TabCityGifts = "citygifts", TabHelp = "help";

    private readonly RegistrationOverlayController _overlayCtrl;
    private string _currentTab = TabHome;
    private ViewModelBase _currentTabContent;
    private ViewModelBase? _activeOverlay;
    private bool _isContextHelpOpen;
    private bool _showNavLeaveConfirm;
    private string? _pendingNavTab;
    private bool _showWelcomeBanner;

    public CoordinatorShellViewModel(long coordinatorId)
    {
        CoordinatorId = coordinatorId;
        Home         = new HomeContentViewModel(coordinatorId);
        Requests     = new AccessRequestsViewModel(coordinatorId);
        Stats        = new StatisticsViewModel(coordinatorId);
        Forum        = new ForumViewModel(coordinatorId);
        Reservations = new ReservationsViewModel(coordinatorId, v =>
        {
            if (v != null) ActiveOverlay = v;
            else if (_activeOverlay is ReservationDialogViewModel) ActiveOverlay = null;
        });
        Reviews      = new ReviewsViewModel(coordinatorId);
        Finances     = new FinancesViewModel(coordinatorId);
        CityGifts    = new CityGiftsViewModel(coordinatorId);
        Help         = new HelpViewModel();
        Header       = new CoordinatorHeaderViewModel(coordinatorId, Home);
        _currentTabContent = Home;
        _overlayCtrl = new RegistrationOverlayController(coordinatorId,
            v => ActiveOverlay = v,
            () => { Home.Refresh(); Header.RefreshDisplayProps(); Finances.RefreshDistricts(); Reservations.RefreshDistricts(); Reviews.RefreshDistricts(); },
            m => NotificationRequested?.Invoke(m));
        Home.RegisterDistrictRequested  += () => _overlayCtrl.Open();
        Home.FinanceRequested           += districtId =>
        {
            Finances.PreSelectDistrict(districtId);
            CurrentTab = TabFinances;
        };
        Home.DistrictDetailRequested    += dto =>
        {
            var vm = new DistrictDetailViewModel(dto);
            vm.CloseRequested += () => ActiveOverlay = null;
            ActiveOverlay = vm;
        };
        Finances.BackToHomeRequested      += () => CurrentTab = TabHome;
        CityGifts.BackToFinancesRequested += () => CurrentTab = TabFinances;
        Home.SelectionChanged          += _ => Header.RefreshDisplayProps();
        Home.UserDistrictClicked       += districtId => Finances.PreSelectDistrict(districtId);
        SwitchTabCommand = new RelayCommand(p =>
        {
            var tab = p?.ToString() ?? TabHome;
            if (tab == _currentTab) return;
            if (HasUnsavedFormInput())
            {
                _pendingNavTab = tab;
                ShowNavLeaveConfirm = true;
            }
            else
            {
                CurrentTab = tab;
            }
        });
        ConfirmNavLeaveCommand = new RelayCommand(_ =>
        {
            ShowNavLeaveConfirm = false;
            if (_activeOverlay is DistrictRegistrationViewModel) _overlayCtrl.Close();
            Finances.ForceCloseDialog();
            Forum.ForceCloseDialog();
            CurrentTab = _pendingNavTab ?? TabHome;
            _pendingNavTab = null;
        });
        StayNavCommand = new RelayCommand(_ => { ShowNavLeaveConfirm = false; _pendingNavTab = null; });
        LogoutCommand            = new RelayCommand(_ => LogoutRequested?.Invoke());
        NotImplementedTabCommand = new RelayCommand(p => NotificationRequested?.Invoke($"Sekcija '{p}' još nije implementirana."));
        OpenRegistrationCommand  = new RelayCommand(_ => _overlayCtrl.Open());
        ToggleContextHelpCommand = new RelayCommand(_ => { if (_currentTab != TabHelp) IsContextHelpOpen = !_isContextHelpOpen; });
        CloseContextHelpCommand  = new RelayCommand(_ => IsContextHelpOpen = false);
        OpenHelpCenterCommand    = new RelayCommand(_ => { IsContextHelpOpen = false; CurrentTab = TabHelp; });

        // Uvodni "welcome" banner — prikazuje se samo prvi put kada se aplikacija pokrene
        _showWelcomeBanner = !AppSettings.HasSeenWelcome;
        WatchWelcomeTutorialCommand = new RelayCommand(_ => { DismissWelcome(); IsContextHelpOpen = false; CurrentTab = TabHelp; });
        DismissWelcomeCommand       = new RelayCommand(_ => DismissWelcome());

        NewItemCommand = new RelayCommand(_ =>
        {
            switch (_currentTab)
            {
                case TabHome:         Home.RegisterDistrictCommand.Execute(null);                  break;
                case TabForum:        Forum.OpenCreateForumCommand.Execute(null);                   break;
                case TabReservations: Reservations.OpenFirstReservationCommand.Execute(null);       break;
                case TabCityGifts:    CityGifts.RegisterFirstAvailableGiftCommand.Execute(null);   break;
            }
        });

        RefreshTabCommand = new RelayCommand(_ =>
        {
            switch (_currentTab)
            {
                case TabHome:         Home.Refresh();                              break;
                case TabRequests:     Requests.RefreshCommand.Execute(null);       break;
                case TabStats:        Stats.RefreshCommand.Execute(null);          break;
                case TabForum:        Forum.SearchCommand.Execute(null);           break;
                case TabReservations: Reservations.RefreshCommand.Execute(null);   break;
                case TabReviews:      Reviews.RefreshCommand.Execute(null);        break;
                case TabCityGifts:    CityGifts.RefreshCommand.Execute(null);      break;
            }
        });

        ConfirmCommand = new RelayCommand(_ =>
        {
            if (_currentTab == TabRequests) Requests.ApproveSelectedCommand.Execute(null);
        });

        DeleteCommand = new RelayCommand(_ =>
        {
            if (_currentTab == TabRequests) Requests.RejectSelectedCommand.Execute(null);
        });

        CancelDialogTabCommand = new RelayCommand(_ =>
        {
            if (_currentTab == TabReservations) Reservations.CancelDialogCommand.Execute(null);
        });

        GlobalEscapeCommand = new RelayCommand(_ =>
        {
            if (_isContextHelpOpen) { IsContextHelpOpen = false; return; }
            switch (_currentTab)
            {
                case TabRequests:     Requests.CloseOverlayCommand.Execute(null);    break;
                case TabCityGifts:    CityGifts.CancelDialogCommand.Execute(null);   break;
            }
        });
    }

    public long CoordinatorId { get; }
    public HomeContentViewModel       Home         { get; }
    public AccessRequestsViewModel    Requests     { get; }
    public StatisticsViewModel        Stats        { get; }
    public ForumViewModel             Forum        { get; }
    public ReservationsViewModel      Reservations { get; }
    public ReviewsViewModel           Reviews      { get; }
    public FinancesViewModel          Finances     { get; }
    public CityGiftsViewModel         CityGifts    { get; }
    public HelpViewModel              Help         { get; }
    public CoordinatorHeaderViewModel Header       { get; }

    public string CurrentTab
    {
        get => _currentTab;
        set
        {
            if (SetProperty(ref _currentTab, value))
            {
                CurrentTabContent = value switch
                {
                    TabRequests     => Requests,
                    TabStats        => Stats,
                    TabForum        => Forum,
                    TabReservations => Reservations,
                    TabReviews      => Reviews,
                    TabFinances     => Finances,
                    TabCityGifts    => CityGifts,
                    TabHelp         => Help,
                    _               => Home
                };
                if (value == TabCityGifts) CityGifts.EnsureLoaded();
                if (value == TabHelp) IsContextHelpOpen = false;
                OnPropertyChanged(nameof(ContextHelpTutorial));
                OnPropertyChanged(nameof(ShowContextHelpButton));
                OnPropertyChanged(nameof(IsHomeTab));
                OnPropertyChanged(nameof(IsRequestsTab));
                OnPropertyChanged(nameof(IsStatsTab));
                OnPropertyChanged(nameof(IsForumTab));
                OnPropertyChanged(nameof(IsReservationsTab));
                OnPropertyChanged(nameof(IsReviewsTab));
                OnPropertyChanged(nameof(IsFinancesTab));
                OnPropertyChanged(nameof(IsCityGiftsTab));
                OnPropertyChanged(nameof(IsHelpTab));
            }
        }
    }

    public ViewModelBase CurrentTabContent { get => _currentTabContent; private set => SetProperty(ref _currentTabContent, value); }
    public bool IsHomeTab         => _currentTab == TabHome;
    public bool IsRequestsTab     => _currentTab == TabRequests;
    public bool IsStatsTab        => _currentTab == TabStats;
    public bool IsForumTab        => _currentTab == TabForum;
    public bool IsReservationsTab => _currentTab == TabReservations;
    public bool IsReviewsTab      => _currentTab == TabReviews;
    public bool IsFinancesTab     => _currentTab == TabFinances;
    public bool IsCityGiftsTab   => _currentTab == TabCityGifts;
    public bool IsHelpTab        => _currentTab == TabHelp;

    public bool IsContextHelpOpen
    {
        get => _isContextHelpOpen;
        private set => SetProperty(ref _isContextHelpOpen, value);
    }

    public bool ShowContextHelpButton => _currentTab != TabHelp;
    public TutorialItemViewModel? ContextHelpTutorial => Help.GetTutorialForTab(_currentTab);

    public bool ShowWelcomeBanner
    {
        get => _showWelcomeBanner;
        private set => SetProperty(ref _showWelcomeBanner, value);
    }

    public ViewModelBase? ActiveOverlay
    {
        get => _activeOverlay;
        private set { if (SetProperty(ref _activeOverlay, value)) OnPropertyChanged(nameof(HasOverlay)); }
    }
    public bool HasOverlay => _activeOverlay != null;

    public bool ShowNavLeaveConfirm
    {
        get => _showNavLeaveConfirm;
        private set => SetProperty(ref _showNavLeaveConfirm, value);
    }

    public ICommand SwitchTabCommand           { get; }
    public ICommand LogoutCommand              { get; }
    public ICommand NotImplementedTabCommand   { get; }
    public ICommand OpenRegistrationCommand    { get; }
    public ICommand ToggleContextHelpCommand   { get; }
    public ICommand CloseContextHelpCommand    { get; }
    public ICommand OpenHelpCenterCommand      { get; }
    public ICommand WatchWelcomeTutorialCommand { get; }
    public ICommand DismissWelcomeCommand      { get; }
    public ICommand NewItemCommand             { get; }
    public ICommand RefreshTabCommand          { get; }
    public ICommand ConfirmCommand             { get; }
    public ICommand DeleteCommand              { get; }
    public ICommand CancelDialogTabCommand     { get; }
    public ICommand GlobalEscapeCommand        { get; }
    public ICommand ConfirmNavLeaveCommand     { get; }
    public ICommand StayNavCommand             { get; }

    public event Action? LogoutRequested;
    public event Action<string>? NotificationRequested;

    public void Initialize()
    {
        Header.Initialize();
        Home.EnsureLoaded();
        Header.RefreshDisplayProps();
    }

    private void DismissWelcome()
    {
        if (!_showWelcomeBanner) return;
        ShowWelcomeBanner = false;
        AppSettings.HasSeenWelcome = true;
        AppSettings.Save();
    }

    private bool HasUnsavedFormInput()
    {
        if (_activeOverlay is DistrictRegistrationViewModel reg && reg.HasUnsavedInput) return true;
        if (Forum.ActiveDialog is CreateForumDialogViewModel forum && forum.HasUnsavedInput) return true;
        if (Finances.ActiveDialog is AddEditExpenseViewModel exp && exp.HasUnsavedInput) return true;
        if (Finances.ActiveDialog is AddDonationViewModel don && don.HasUnsavedInput) return true;
        return false;
    }
}
