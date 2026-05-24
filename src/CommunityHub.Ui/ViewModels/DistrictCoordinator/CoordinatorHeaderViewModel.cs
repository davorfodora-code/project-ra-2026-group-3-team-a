using System;
using System.Globalization;
using System.Linq;
using CommunityHub.Application;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class CoordinatorHeaderViewModel : ViewModelBase
{
    private readonly IUserRepository _userRepository;
    private readonly HomeContentViewModel _home;
    private string _fullName = string.Empty;
    private string _initials = "??";
    private string _savedAtText = string.Empty;

    public CoordinatorHeaderViewModel(long coordinatorId, HomeContentViewModel home)
    {
        CoordinatorId   = coordinatorId;
        _userRepository = Injector.Get<IUserRepository>();
        _home           = home;
    }

    public long   CoordinatorId { get; }
    public string FullName      { get => _fullName;    private set { _fullName = value; OnPropertyChanged(); } }
    public string Initials      { get => _initials;    private set { _initials = value; OnPropertyChanged(); } }
    public string SavedAtText   { get => _savedAtText; private set => SetProperty(ref _savedAtText, value); }

    public string TotalBudgetDisplay
    {
        get
        {
            decimal total = _home.DistrictCards.Sum(c => c.Budget);
            return total.ToString("N0", CultureInfo.GetCultureInfo("sr-Latn-RS")) + " RSD";
        }
    }

    public string FooterInfo
    {
        get
        {
            string place = _home.SelectedCard != null
                ? $"{_home.SelectedCard.Name}, {_home.SelectedCard.LocationDisplay.Split(',')[0].Trim()}"
                : string.Empty;
            return string.Join(Environment.NewLine,
                new[] { _fullName, "Koordinator", place }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }

    public void Initialize()
    {
        SavedAtText = $"Sačuvano u {DateTime.Now:HH:mm}";
        User? user  = _userRepository.GetById(CoordinatorId);
        if (user == null) { FullName = "Nepoznat korisnik"; Initials = "?"; return; }
        FullName = $"{user.Name} {user.Surname}";
        Initials = $"{InitialOf(user.Name)}{InitialOf(user.Surname)}";
    }

    public void RefreshDisplayProps()
    {
        OnPropertyChanged(nameof(TotalBudgetDisplay));
        OnPropertyChanged(nameof(FooterInfo));
    }

    private static char InitialOf(string s) =>
        string.IsNullOrWhiteSpace(s) ? '?' : char.ToUpper(s.Trim()[0]);
}
