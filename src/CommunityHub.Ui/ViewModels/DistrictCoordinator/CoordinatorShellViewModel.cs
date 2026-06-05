using System;
using System.Windows.Input;
using CommunityHub.Application.Services;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class CoordinatorShellViewModel : ViewModelBase
{
    public const string TabHome = "home", TabRequests = "requests", TabStats = "stats", TabForum = "forum", TabReservations = "reservations", TabReviews = "reviews", TabFinances = "finances";

    private readonly RegistrationOverlayController _overlayCtrl;
    private string _currentTab = TabHome;
    private ViewModelBase _currentTabContent;
    private ViewModelBase? _activeOverlay;

    public CoordinatorShellViewModel(long coordinatorId)
    {
        CoordinatorId = coordinatorId;
        Home         = new HomeContentViewModel(coordinatorId);
        Requests     = new AccessRequestsViewModel(coordinatorId);
        Stats        = new StatisticsViewModel(coordinatorId);
        Forum        = new ForumViewModel(coordinatorId);
        Reservations = new ReservationsViewModel(coordinatorId);
        Reviews      = new ReviewsViewModel(coordinatorId);
        Finances     = new FinancesViewModel(coordinatorId);
        Header       = new CoordinatorHeaderViewModel(coordinatorId, Home);
        _currentTabContent = Home;
        _overlayCtrl = new RegistrationOverlayController(coordinatorId,
            v => ActiveOverlay = v,
            () => { Home.Refresh(); Header.RefreshDisplayProps(); },
            m => NotificationRequested?.Invoke(m));
        Home.RegisterDistrictRequested += () => _overlayCtrl.Open();
        Home.FinanceRequested          += districtId =>
        {
            Finances.PreSelectDistrict(districtId);
            CurrentTab = TabFinances;
        };
        Finances.BackToHomeRequested += () => CurrentTab = TabHome;
        Home.SelectionChanged          += _ => Header.RefreshDisplayProps();
        SwitchTabCommand         = new RelayCommand(p => CurrentTab = p?.ToString() ?? TabHome);
        LogoutCommand            = new RelayCommand(_ => LogoutRequested?.Invoke());
        NotImplementedTabCommand = new RelayCommand(p => NotificationRequested?.Invoke($"Sekcija '{p}' još nije implementirana."));
        OpenRegistrationCommand  = new RelayCommand(_ => _overlayCtrl.Open());
    }

    public long CoordinatorId { get; }
    public HomeContentViewModel       Home         { get; }
    public AccessRequestsViewModel    Requests     { get; }
    public StatisticsViewModel        Stats        { get; }
    public ForumViewModel             Forum        { get; }
    public ReservationsViewModel      Reservations { get; }
    public ReviewsViewModel           Reviews      { get; }
    public FinancesViewModel          Finances     { get; }
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
                    _               => Home
                };
                OnPropertyChanged(nameof(IsHomeTab));
                OnPropertyChanged(nameof(IsRequestsTab));
                OnPropertyChanged(nameof(IsStatsTab));
                OnPropertyChanged(nameof(IsForumTab));
                OnPropertyChanged(nameof(IsReservationsTab));
                OnPropertyChanged(nameof(IsReviewsTab));
                OnPropertyChanged(nameof(IsFinancesTab));
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

    public ViewModelBase? ActiveOverlay
    {
        get => _activeOverlay;
        private set { if (SetProperty(ref _activeOverlay, value)) OnPropertyChanged(nameof(HasOverlay)); }
    }
    public bool HasOverlay => _activeOverlay != null;

    public ICommand SwitchTabCommand         { get; }
    public ICommand LogoutCommand            { get; }
    public ICommand NotImplementedTabCommand { get; }
    public ICommand OpenRegistrationCommand  { get; }

    public event Action? LogoutRequested;
    public event Action<string>? NotificationRequested;

    public void Initialize()
    {
        Header.Initialize();
        Home.EnsureLoaded();
        Header.RefreshDisplayProps();
    }
}
