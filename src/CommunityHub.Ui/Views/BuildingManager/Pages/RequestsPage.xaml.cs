using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityHub.Application.Dto.BuildingManager;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class RequestsPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly AccessRequestsViewModel _vm;

    private int _activeTab = 0;
    private bool _newestFirst = true;
    private Button[] _tabButtons = null!;

    private static readonly Color _activeColor   = Color.FromRgb(0x1A, 0x1A, 0x1A);
    private static readonly Color _inactiveColor = Color.FromRgb(0xAA, 0xAA, 0xAA);

    public RequestsPage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new AccessRequestsViewModel(shell.UserId);
        _tabButtons = new[] { BtnZaPristup, BtnZaIznajmljivanje };
        SetActiveTab(0);
    }

    // --- Tab logika ---

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
            _tabButtons[i].Foreground  = new SolidColorBrush(active ? _activeColor : _inactiveColor);
            _tabButtons[i].BorderBrush = new SolidColorBrush(active ? _activeColor : Colors.Transparent);
            _tabButtons[i].FontWeight  = active ? FontWeights.SemiBold : FontWeights.Normal;
        }

        if (index == 0) LoadPristupTab();
        else            LoadIznajmljivanjeTab();
    }

    // --- Za pristup tab ---

    private void LoadPristupTab()
    {
        _vm.LoadRequests();
        RenderPristupRequests();
    }

    private void RenderPristupRequests()
    {
        RequestsPanel.Children.Clear();

        string filterTag = GetSelectedFilterTag();
        List<BuildingAccessRequestDto> requests = _vm.GetFiltered(filterTag, _newestFirst);

        if (requests.Count == 0)
        {
            RequestsPanel.Children.Add(new TextBlock
            {
                Text = "Nema zahteva za prikaz.",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            });
            return;
        }

        foreach (BuildingAccessRequestDto req in requests)
            RequestsPanel.Children.Add(CreatePristupCard(req));
    }

    private UIElement CreatePristupCard(BuildingAccessRequestDto req)
    {
        Border card = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(14, 12, 14, 12)
        };

        StackPanel content = new StackPanel();

        // Ime stanara
        content.Children.Add(new TextBlock
        {
            Text = req.TenantFullName,
            FontSize = 15, FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 4)
        });

        content.Children.Add(MakeInfoLine($"{req.BuildingAddress}, {req.Neighborhood}, {req.Location}"));
        content.Children.Add(MakeInfoLine($"Stan: {req.ApartmentNumber}"));

        // Red: datum+status levo, dugmad desno
        Grid bottomRow = new Grid { Margin = new Thickness(0, 6, 0, 0) };
        bottomRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        bottomRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        StackPanel leftInfo = new StackPanel();
        leftInfo.Children.Add(MakeInfoLine($"Datum: {req.DateDisplay}"));
        leftInfo.Children.Add(new TextBlock
        {
            Text = $"Status: {req.StatusDisplay}",
            FontSize = 12, FontWeight = FontWeights.SemiBold,
            Foreground = StatusColor(req.Status)
        });
        Grid.SetColumn(leftInfo, 0);
        bottomRow.Children.Add(leftInfo);

        if (req.IsPending)
        {
            StackPanel btnStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(8, 0, 0, 0)
            };

            Button odobriBtn = new Button
            {
                Content = "Odobri",
                Style = (Style)FindResource("ApproveStyle"),
                Margin = new Thickness(0, 0, 0, 4)
            };
            Button odbijBtn = new Button
            {
                Content = "Odbij",
                Style = (Style)FindResource("RejectStyle")
            };

            btnStack.Children.Add(odobriBtn);
            btnStack.Children.Add(odbijBtn);
            Grid.SetColumn(btnStack, 1);
            bottomRow.Children.Add(btnStack);

            // Panel za obrazlozenje (skriven)
            StackPanel rejectPanel = new StackPanel
            {
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 8, 0, 0)
            };
            rejectPanel.Children.Add(new TextBlock
            {
                Text = "Obrazlozenje za odbijanje:",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
                Margin = new Thickness(0, 0, 0, 4)
            });

            TextBox reasonBox = new TextBox
            {
                Height = 60, FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Padding = new Thickness(6),
                Text = "Zahtev je odbijen zbog...",
                Foreground = Brushes.Gray
            };
            reasonBox.GotFocus  += (s, e) => { if (reasonBox.Text == "Zahtev je odbijen zbog...") { reasonBox.Text = ""; reasonBox.Foreground = Brushes.Black; } };
            reasonBox.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(reasonBox.Text)) { reasonBox.Text = "Zahtev je odbijen zbog..."; reasonBox.Foreground = Brushes.Gray; } };

            StackPanel confirmRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 6, 0, 0)
            };
            Button potvrdiBtn = new Button { Content = "Potvrdi", Style = (Style)FindResource("SmallBtnStyle"), Margin = new Thickness(0, 0, 6, 0) };
            Button otkaziBtn  = new Button { Content = "Otkazi",  Style = (Style)FindResource("SmallBtnStyle") };
            confirmRow.Children.Add(potvrdiBtn);
            confirmRow.Children.Add(otkaziBtn);

            rejectPanel.Children.Add(reasonBox);
            rejectPanel.Children.Add(confirmRow);

            odobriBtn.Click += (s, e) =>
            {
                try { _vm.Approve(req.RequestId); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Greska"); return; }
                LoadPristupTab();
            };

            odbijBtn.Click += (s, e) =>
            {
                rejectPanel.Visibility = Visibility.Visible;
                odbijBtn.IsEnabled = false;
            };

            potvrdiBtn.Click += (s, e) =>
            {
                string reason = reasonBox.Text.Trim() == "Zahtev je odbijen zbog..." ? "" : reasonBox.Text.Trim();
                try { _vm.Reject(req.RequestId, reason); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Greska"); return; }
                LoadPristupTab();
            };

            otkaziBtn.Click += (s, e) =>
            {
                rejectPanel.Visibility = Visibility.Collapsed;
                odbijBtn.IsEnabled = true;
            };

            content.Children.Add(bottomRow);
            content.Children.Add(rejectPanel);
        }
        else
        {
            content.Children.Add(bottomRow);
        }

        card.Child = content;
        return card;
    }

    // --- Za iznajmljivanje tab (stub — Feature 3) ---

    private void LoadIznajmljivanjeTab()
    {
        RequestsPanel.Children.Clear();
        RequestsPanel.Children.Add(new TextBlock
        {
            Text = "Zahtevi za iznajmljivanje prostorija dolaze uskoro.",
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(16, 40, 16, 0)
        });
    }

    // --- Filter i sort ---

    private void Filter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (RequestsPanel == null) return;
        if (_activeTab == 0) RenderPristupRequests();
    }

    private void Sort_Click(object sender, RoutedEventArgs e)
    {
        _newestFirst = !_newestFirst;
        if (_activeTab == 0) RenderPristupRequests();
    }

    private string GetSelectedFilterTag()
    {
        if (FilterCombo.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            return tag;
        return "Svi";
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Zahtevi za pristup: odobrite ili odbijte zahteve stanara.\n\n" +
            "Zahtevi za iznajmljivanje: upravljajte zahtevima za zajednicke prostorije.",
            "Pomoc", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // --- Pomocne metode ---

    private static TextBlock MakeInfoLine(string text) =>
        new TextBlock
        {
            Text = text, FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
            Margin = new Thickness(0, 0, 0, 2),
            TextWrapping = TextWrapping.Wrap
        };

    private static Brush StatusColor(string status) => status switch
    {
        "Accepted" => new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32)),
        "Rejected" => new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28)),
        _          => new SolidColorBrush(Color.FromRgb(0xE6, 0x5C, 0x00))
    };
}
