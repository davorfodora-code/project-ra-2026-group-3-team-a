using System.Windows;
using System.Windows.Media;
using CommunityHub.Ui.Settings;
using CommunityHub.Ui.Views.BuildingManager.Pages;
using CommunityHub.Ui.Views.BuildingManager.Wizard;
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
        MainFrame.Navigate(new HomePage(this));

        // Auto-trigger wizard-a pri prvom pokretanju (pre nego sto je flag setovan u AppSettings)
        if (!AppSettings.WizardCompleted)
            Loaded += (s, e) => StartWizard();
    }

    // Pokreni Pokretni vodič (poziva se i automatski i ručno iz MorePage)
    public void StartWizard()
    {
        WizardController controller = new WizardController(this, Wizard);
        controller.Start();
    }

    // Programatska navigacija na tab (koristi je wizard pre highlight-a koraka)
    public void NavigateToTab(int index)
    {
        switch (index)
        {
            case 0: NavPocetna_Click(this, new RoutedEventArgs()); break;
            case 1: NavZgrade_Click(this, new RoutedEventArgs()); break;
            case 2: NavZahtevi_Click(this, new RoutedEventArgs()); break;
            case 3: NavSkupstine_Click(this, new RoutedEventArgs()); break;
            case 4: NavVise_Click(this, new RoutedEventArgs()); break;
        }
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

    private void NavPocetna_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(0);
        MainFrame.Navigate(new HomePage(this));
    }

    private void NavZgrade_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(1);
        MainFrame.Navigate(new BuildingsPage(this));
    }

    private void NavZahtevi_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(2);
        MainFrame.Navigate(new RequestsPage(this));
    }

    private void NavSkupstine_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(3);
        MainFrame.Navigate(new AssembliesPage(this));
    }

    private void NavVise_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(4);
        MainFrame.Navigate(new MorePage(this));
    }
}
