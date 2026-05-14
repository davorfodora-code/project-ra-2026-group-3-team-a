using System.Collections.ObjectModel;
using System.Windows;
using CommunityHub.Application.Database.Repositories;
using CommunityHub.Application.Domain;

namespace CommunityHub.Ui.Views;

public partial class ProfileWindow : Window
{
    private readonly UserDbRepository _userRepository;

    private readonly long _userId;
    private User _user;
    private ObservableCollection<Post> _posts;

    public ProfileWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _userRepository = new UserDbRepository();
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
