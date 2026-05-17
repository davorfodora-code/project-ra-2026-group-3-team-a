using System;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class CoordinatorShellViewModel : ViewModelBase
{
    public const string TabHome     = "home";
    public const string TabRequests = "requests";
    public const string TabStats    = "stats";
    public const string TabForum    = "forum";

    private readonly IUserRepository _userRepository;
    private readonly RegistrationOverlayController _overlayCtrl;

    private string _currentTab = TabHome;
    private ViewModelBase _currentTabContent;
    private ViewModelBase? _activeOverlay;
    private (string Name, string Initials) _coordinator = (string.Empty, "??");
    private string _savedAtText = string.Empty;

    public CoordinatorShellViewModel(long coordinatorId)
    {
        CoordinatorId   = coordinatorId;
        _userRepository = Injector.Get<IUserRepository>();

        Home     = new HomeContentViewModel(coordinatorId);
        Requests = new AccessRequestsViewModel(coordinatorId);
        Stats    = new StatisticsViewModel(coordinatorId);
        Forum    = new ForumViewModel(coordinatorId);

        _currentTabContent = Home;

        _overlayCtrl = new RegistrationOverlayController(
            coordinatorId,
            v => ActiveOverlay = v,
            () => { Home.Refresh(); OnPropertyChanged(nameof(TotalBudgetDisplay)); OnPropertyChanged(nameof(FooterCoordinatorInfo)); },
            m => NotificationRequested?.Invoke(m));

        Home.RegisterDistrictRequested += () => _overlayCtrl.Open();
        Home.FinanceRequested          += d  => NotificationRequested?.Invoke($"Finansije za kvart '{d}' — još nije implementirano.");
        Home.SelectionChanged          += _  => OnPropertyChanged(nameof(FooterCoordinatorInfo));

        SwitchTabCommand         = new RelayCommand(p => CurrentTab = p?.ToString() ?? TabHome);
        LogoutCommand            = new RelayCommand(_ => LogoutRequested?.Invoke());
        NotImplementedTabCommand = new RelayCommand(p => NotificationRequested?.Invoke($"Sekcija '{p}' još nije implementirana."));
        OpenRegistrationCommand  = new RelayCommand(_ => _overlayCtrl.Open());
    }

    public long CoordinatorId { get; }

    public HomeContentViewModel    Home     { get; }
    public AccessRequestsViewModel Requests { get; }
    public StatisticsViewModel     Stats    { get; }
    public ForumViewModel          Forum    { get; }

    public string CurrentTab
    {
        get => _currentTab;
        set
        {
            if (SetProperty(ref _currentTab, value))
            {
                CurrentTabContent = value switch
                {
                    TabRequests => Requests,
                    TabStats    => Stats,
                    TabForum    => Forum,
                    _           => Home,
                };
                OnPropertyChanged(nameof(IsHomeTab));
                OnPropertyChanged(nameof(IsRequestsTab));
                OnPropertyChanged(nameof(IsStatsTab));
                OnPropertyChanged(nameof(IsForumTab));
            }
        }
    }

    public ViewModelBase CurrentTabContent
    {
        get => _currentTabContent;
        private set => SetProperty(ref _currentTabContent, value);
    }

    public bool IsHomeTab     => _currentTab == TabHome;
    public bool IsRequestsTab => _currentTab == TabRequests;
    public bool IsStatsTab    => _currentTab == TabStats;
    public bool IsForumTab    => _currentTab == TabForum;

    public ViewModelBase? ActiveOverlay
    {
        get => _activeOverlay;
        private set { if (SetProperty(ref _activeOverlay, value)) OnPropertyChanged(nameof(HasOverlay)); }
    }

    public bool HasOverlay => _activeOverlay != null;

    public string CoordinatorFullName
    {
        get => _coordinator.Name;
        private set { _coordinator.Name = value; OnPropertyChanged(); }
    }

    public string CoordinatorInitials
    {
        get => _coordinator.Initials;
        private set { _coordinator.Initials = value; OnPropertyChanged(); }
    }

    public string SavedAtText
    {
        get => _savedAtText;
        private set => SetProperty(ref _savedAtText, value);
    }

    public string TotalBudgetDisplay
    {
        get
        {
            decimal total = Home.DistrictCards.Sum(c => c.Budget);
            return total.ToString("N0", CultureInfo.GetCultureInfo("sr-Latn-RS")) + " RSD";
        }
    }

    public string FooterCoordinatorInfo
    {
        get
        {
            string placeLine = Home.SelectedCard != null
                ? $"{Home.SelectedCard.Name}, {Home.SelectedCard.LocationDisplay.Split(',')[0].Trim()}"
                : string.Empty;
            return string.Join(Environment.NewLine,
                new[] { _coordinator.Name, "Koordinator", placeLine }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }

    public ICommand SwitchTabCommand         { get; }
    public ICommand LogoutCommand            { get; }
    public ICommand NotImplementedTabCommand { get; }
    public ICommand OpenRegistrationCommand  { get; }

    public event Action? LogoutRequested;
    public event Action<string>? NotificationRequested;

    public void Initialize()
    {
        SavedAtText = $"Sačuvano u {DateTime.Now:HH:mm}";
        LoadCoordinator();
        Home.EnsureLoaded();
        OnPropertyChanged(nameof(TotalBudgetDisplay));
        OnPropertyChanged(nameof(FooterCoordinatorInfo));
    }

    private void LoadCoordinator()
    {
        User? user = _userRepository.GetById(CoordinatorId);
        if (user == null) { CoordinatorFullName = "Nepoznat korisnik"; CoordinatorInitials = "?"; return; }
        CoordinatorFullName = $"{user.Name} {user.Surname}";
        CoordinatorInitials = $"{InitialOf(user.Name)}{InitialOf(user.Surname)}";
    }

    private static char InitialOf(string s) =>
        string.IsNullOrWhiteSpace(s) ? '?' : char.ToUpper(s.Trim()[0]);
}
