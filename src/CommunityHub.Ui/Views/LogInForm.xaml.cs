using CommunityHub.Application.Database.Repositories;
using CommunityHub.Ui.Views.BuildingManager;
using CommunityHub.Ui.Views.Citizen;
using CommunityHub.Ui.Views.DistrictCoordinator;
using CommunityHub.Ui.Views.Tenant;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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

        login(username, password);
    }

    private void login(string username, string password)
    {
        var credentials = _userRepository.GetByCredentials(username, password);

        if (credentials == null)
        {
            ErrorMessageTextBlock.Text = "Neispravno korisnicko ime ili lozinka.";
            ErrorMessageTextBlock.Visibility = Visibility.Visible;
            return;
        }

        long userId = credentials.Value.Id;
        string role = credentials.Value.Role;

        Window? targetWindow = role switch
        {
            "BuildingManager" => new BuildingManagerWindow(userId),
            "Tenant" => new TenantWindow(userId),
            "DistrictCoordinator" => new CoordinatorHomeWindow(userId),
            "Citizen" => new CitizenShellWindow(userId),
            _ => null
        };

        if (targetWindow != null)
        {
            targetWindow.Show();
            this.Close();
        }
        else
        {
            ErrorMessageTextBlock.Text = $"Uloga '{role}' jos nije implementirana.";
            ErrorMessageTextBlock.Visibility = Visibility.Visible;
        }
    }
}
