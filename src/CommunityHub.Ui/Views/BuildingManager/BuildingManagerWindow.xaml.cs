using System.Windows;
using System.Windows.Media;
using FontAwesome.Sharp;

namespace CommunityHub.Ui.Views.BuildingManager;

public partial class BuildingManagerWindow : Window
{
    public readonly long UserId;

    private readonly Color _activeColor = (Color)ColorConverter.ConvertFromString("#1A1A1A");
    private readonly Color _inactiveColor = (Color)ColorConverter.ConvertFromString("#9E9E9E");

    // Parovi (ikonica, tekst) za svaku stavku navigacije
    private (IconBlock Icon, System.Windows.Controls.TextBlock Text)[] _navItems = null!;

    public BuildingManagerWindow(long userId)
    {
        InitializeComponent();
        UserId = userId;

        _navItems = new[]
        {
            (IconPocetna, TextPocetna),
            (IconZgrade, TextZgrade),
            (IconZahtevi, TextZahtevi),
            (IconSkupstine, TextSkupstine),
            (IconVise, TextVise)
        };

        SetActiveNav(0);
    }

    // Postavlja aktivnu stavku navigacije i osvežava boje
    public void SetActiveNav(int index)
    {
        for (int i = 0; i < _navItems.Length; i++)
        {
            Brush color = i == index
                ? new SolidColorBrush(_activeColor)
                : new SolidColorBrush(_inactiveColor);

            _navItems[i].Icon.Foreground = color;
            _navItems[i].Text.Foreground = color;
        }
    }

    // Navigacija — Pages se dodaju u sledećem commitu
    private void NavPocetna_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(0);
        // MainFrame.Navigate(new Pages.HomePage(this));
    }

    private void NavZgrade_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(1);
        // MainFrame.Navigate(new Pages.BuildingsPage(this));
    }

    private void NavZahtevi_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(2);
        // MainFrame.Navigate(new Pages.RequestsPage(this));
    }

    private void NavSkupstine_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(3);
        // MainFrame.Navigate(new Pages.AssembliesPage(this));
    }

    private void NavVise_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(4);
        // MainFrame.Navigate(new Pages.MorePage(this));
    }
}
