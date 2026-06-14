using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application;
using CommunityHub.Application.Database.Repositories;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Ui.Views.Tenant.Pages;

public partial class TenantProfilePage : Page
{
    private readonly TenantWindow _shell;

    public TenantProfilePage(TenantWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        Load();
    }

    private void Load()
    {
        var user = new UserDbRepository().GetById(_shell.UserId);
        if (user == null) return;

        string n = !string.IsNullOrEmpty(user.Name)    ? char.ToUpper(user.Name[0]).ToString()    : "?";
        string s = !string.IsNullOrEmpty(user.Surname) ? char.ToUpper(user.Surname[0]).ToString() : "?";
        InitialsBlock.Text = n + s;
        FullNameBlock.Text = $"{user.Name} {user.Surname}";
        UsernameBlock.Text       = $"@{user.Username}";
        UsernameDetailBlock.Text = user.Username;
        NameBlock.Text           = user.Name;
        SurnameBlock.Text        = user.Surname;
        BirthdayBlock.Text = user.BirthDay.ToString("dd.MM.yyyy.");
        AddressBlock.Text  = string.IsNullOrWhiteSpace(user.Address) ? "—" : user.Address;

        var memberships = Injector.Get<ITenantProfileRepository>().GetMemberships(_shell.UserId);
        NoBuildingsText.Visibility = memberships.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        BuildingsList.ItemsSource  = memberships;
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        var toClose = System.Windows.Application.Current.Windows.Cast<Window>().ToList();
        new LogInForm().Show();
        foreach (var w in toClose)
            w.Close();
    }
}
