using System.Windows;
using CommunityHub.Application.Database.Repositories;
using CommunityHub.Ui.Views.BuildingManager;

namespace CommunityHub.Ui.Views;

public partial class LogInForm : Window
{
    private readonly UserDbRepository _userRepository;

    public LogInForm()
    {
        InitializeComponent();
        _userRepository = new UserDbRepository();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        string username = UsernameTextBox.Text;
        string password = PasswordBox.Password;

        var credentials = _userRepository.GetByCredentials(username, password);

        if (credentials == null)
        {
            ErrorMessageTextBlock.Text = "Neispravno korisničko ime ili lozinka.";
            ErrorMessageTextBlock.Visibility = Visibility.Visible;
            return;
        }

        long userId = credentials.Value.Id;
        string role = credentials.Value.Role;

        Window? targetWindow = role switch
        {
            "BuildingManager" => new BuildingManagerHomeWindow(userId),
            // "Tenant" => new TenantHomeWindow(userId),
            // "DistrictCoordinator" => new DistrictCoordinatorHomeWindow(userId),
            // "Citizen" => new CitizenHomeWindow(userId),
            _ => null
        };

        if (targetWindow != null)
        {
            targetWindow.Show();
            this.Close();
        }
        else
        {
            ErrorMessageTextBlock.Text = $"Stranica za ulogu \"{role}\" je u pripremi.";
            ErrorMessageTextBlock.Visibility = Visibility.Visible;
        }
    }
}
