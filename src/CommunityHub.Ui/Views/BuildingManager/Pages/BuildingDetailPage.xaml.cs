using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityHub.Application.Dto.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class BuildingDetailPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly BuildingDto _building;

    private Button[] _tabButtons = null!;
    private int _activeTab = 0;

    private static readonly Color _activeColor = Color.FromRgb(0x1A, 0x1A, 0x1A);
    private static readonly Color _inactiveColor = Color.FromRgb(0xAA, 0xAA, 0xAA);

    public BuildingDetailPage(BuildingManagerWindow shell, BuildingDto building)
    {
        InitializeComponent();
        _shell = shell;
        _building = building;

        AddressText.Text = building.AddressDisplay;
        LocationText.Text = $"{building.Neighborhood}, {building.City}";

        _tabButtons = new[] { BtnInfo, BtnClanovi, BtnProstorije, BtnProblemi };
        SetActiveTab(0);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
        {
            _shell.SetActiveNav(1);
            _shell.MainFrame.Navigate(new BuildingsPage(_shell));
        }
    }

    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        int index = int.Parse((string)((Button)sender).Tag);
        SetActiveTab(index);
    }

    private void SetActiveTab(int index)
    {
        _activeTab = index;

        for (int i = 0; i < _tabButtons.Length; i++)
        {
            bool active = i == index;
            _tabButtons[i].Foreground = new SolidColorBrush(active ? _activeColor : _inactiveColor);
            _tabButtons[i].BorderBrush = new SolidColorBrush(active ? _activeColor : Colors.Transparent);
            _tabButtons[i].FontWeight = active ? FontWeights.SemiBold : FontWeights.Normal;
        }

        RenderTabContent(index);
    }

    // Privremeni stub sadržaj za svaki tab (popunjava se u Commit 4)
    private void RenderTabContent(int index)
    {
        TabContent.Children.Clear();

        switch (index)
        {
            case 0: // Info
                TabContent.Children.Add(MakeInfoRow("Ulica", _building.AddressDisplay));
                TabContent.Children.Add(MakeInfoRow("Naselje", _building.Neighborhood));
                TabContent.Children.Add(MakeInfoRow("Grad", _building.City));
                TabContent.Children.Add(MakeInfoRow("Država", _building.Country));
                TabContent.Children.Add(MakeInfoRow("Spratovi", _building.NumberOfFloors.ToString()));
                TabContent.Children.Add(MakeInfoRow("Stanovi", _building.TotalApartments.ToString()));
                break;

            case 1: // Članovi
                TabContent.Children.Add(MakePlaceholder("Lista stanara dolazi uskoro."));
                break;

            case 2: // Prostorije
                TabContent.Children.Add(MakePlaceholder("Lista zajedničkih prostorija dolazi uskoro."));
                break;

            case 3: // Problemi
                TabContent.Children.Add(MakePlaceholder("Lista problema dolazi uskoro."));
                break;
        }
    }

    private static UIElement MakeInfoRow(string label, string value)
    {
        Grid row = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        TextBlock lbl = new TextBlock
        {
            Text = label,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88))
        };

        TextBlock val = new TextBlock
        {
            Text = value,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        };

        Grid.SetColumn(lbl, 0);
        Grid.SetColumn(val, 1);
        row.Children.Add(lbl);
        row.Children.Add(val);
        return row;
    }

    private static UIElement MakePlaceholder(string text) =>
        new TextBlock
        {
            Text = text,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 40, 0, 0)
        };
}
