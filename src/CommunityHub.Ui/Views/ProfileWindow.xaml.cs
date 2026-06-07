using System.Collections.ObjectModel;
using System.Windows;
using CommunityHub.Application;
using CommunityHub.Application.Database.Repositories;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Ui.Views;

public partial class ProfileWindow : Window
{
    private readonly UserDbRepository _userRepository;
    private readonly ICitizenTrustRecordService _trustRecordService;

    private readonly long _userId;
    private User? _user;
    private ObservableCollection<Post>? _posts;

    public ProfileWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _userRepository = new UserDbRepository();
        _trustRecordService = Injector.Get<ICitizenTrustRecordService>();
        LoadUser();
    }

    private void LoadUser()
    {
        User? user = _userRepository.GetWithPosts(_userId);
        if (user == null)
        {
            MessageBox.Show("Korisnik nije pronađen.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }

        _user = user;
        UserInfoTextBlock.Text = $"{_user.Name} {_user.Surname} ({_user.BirthDay.ToString("dd.MM.yyyy.")})";

        _posts = new ObservableCollection<Post>(_user.Posts ?? []);
        PostsItemsControl.ItemsSource = _posts;
        LoadTrustRecord();
    }

    private void LoadTrustRecord()
    {
        CitizenTrustRecordDto? trustRecord = _trustRecordService.GetMyTrustRecord(_userId);
        if (trustRecord == null)
        {
            TrustJoinDateTextBlock.Text = "-";
            TrustOrganizedCountTextBlock.Text = "-";
            TrustVolunteerCountTextBlock.Text = "-";
            TrustLevelTextBlock.Text = "Niste član kvarta";
            TrustInfoTextBlock.Text = "Evidencija poverenja postaje dostupna nakon učlanjenja u kvart.";
            return;
        }

        TrustJoinDateTextBlock.Text = trustRecord.JoinDate.ToString("dd.MM.yyyy.");
        TrustOrganizedCountTextBlock.Text = trustRecord.OrganizedCompletedEventsCount.ToString();
        TrustVolunteerCountTextBlock.Text = trustRecord.ConfirmedVolunteerEventsCount.ToString();
        TrustLevelTextBlock.Text = FormatTrustLevel(trustRecord.TrustLevel);
        TrustInfoTextBlock.Text = string.Empty;
    }

    private static string FormatTrustLevel(string level)
    {
        return level switch
        {
            "Nov" => "Nov u kvartu",
            "Neaktivan" => "Neaktivan građanin",
            "Aktivan" => "Aktivan građanin",
            "Istaknuti" => "Istaknut građanin",
            "OdPoverenja" => "Građanin od poverenja",
            _ => level
        };
    }

    private void BackButton_Click(object sender, RoutedEventArgs e) => Close();

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        var toClose = System.Windows.Application.Current.Windows.Cast<Window>().ToList();
        new LogInForm().Show();
        foreach (var w in toClose)
            w.Close();
    }
}
