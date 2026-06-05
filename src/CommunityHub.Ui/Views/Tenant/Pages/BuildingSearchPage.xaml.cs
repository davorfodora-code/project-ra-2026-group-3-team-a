using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant.Pages;

public partial class BuildingSearchPage : System.Windows.Controls.Page
{
    private readonly TenantWindow _shell;
    private readonly long _userId;
    private readonly BuildingSearchViewModel _searchVm;
    private readonly MyBuildingRequestsViewModel _requestsVm;

    public BuildingSearchPage(TenantWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _userId = shell.UserId;

        _searchVm = new BuildingSearchViewModel();
        _requestsVm = new MyBuildingRequestsViewModel(_userId);

        RequestsDataGrid.ItemsSource = _requestsVm.FilteredRequests;

        _searchVm.LoadAll();
        PopulateCombos();
        RefreshCards();
        _shell.SetBreadcrumb("Pretraga zgrada");
    }

    private void TabSearch_Click(object sender, RoutedEventArgs e)
    {
        SwitchTab(TabSearchBtn, TabSearchText, SearchPanel, TabRequestsBtn, TabRequestsText, RequestsPanel);
        _shell.SetBreadcrumb("Pretraga zgrada");
    }

    private void TabRequests_Click(object sender, RoutedEventArgs e)
    {
        SwitchTab(TabRequestsBtn, TabRequestsText, RequestsPanel, TabSearchBtn, TabSearchText, SearchPanel);
        _requestsVm.LoadRequests();
        ApplyRequestFilters();
        _shell.SetBreadcrumb("Moji zahtevi");
    }

    private static void SwitchTab(
        Button activeBtn, TextBlock activeText, UIElement showPanel,
        Button inactiveBtn, TextBlock inactiveText, UIElement hidePanel)
    {
        activeBtn.Tag   = "active";
        inactiveBtn.Tag = null;
        activeText.FontWeight   = FontWeights.SemiBold;
        activeText.Foreground   = Brushes.Black;
        inactiveText.FontWeight = FontWeights.Normal;
        inactiveText.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
        showPanel.Visibility = Visibility.Visible;
        hidePanel.Visibility = Visibility.Collapsed;
    }

    private void PopulateCombos()
    {
        var cities = new List<string> { "Svi gradovi" };
        cities.AddRange(_searchVm.Buildings.Select(b => b.City).Distinct().OrderBy(c => c));
        CityComboBox.ItemsSource = cities;
        CityComboBox.SelectedIndex = 0;
        var countries = new List<string> { "Sve države" };
        countries.AddRange(_searchVm.Buildings.Select(b => b.Country).Distinct().OrderBy(c => c));
        CountryComboBox.ItemsSource = countries;
        CountryComboBox.SelectedIndex = 0;
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e) => PerformSearch();

    private void LiveSearch_Changed(object sender, EventArgs e)
    {
        if (IsLoaded) PerformSearch();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        AddressTextBox.Text = string.Empty;
        NeighborhoodTextBox.Text = string.Empty;
        CityComboBox.SelectedIndex = 0;
        CountryComboBox.SelectedIndex = 0;
        _searchVm.LoadAll();
        RefreshCards();
    }

    private void PerformSearch()
    {
        string? address = AddressTextBox.Text.Trim() is { Length: > 0 } a ? a : null;
        string? neighborhood = NeighborhoodTextBox.Text.Trim() is { Length: > 0 } n ? n : null;
        string? city = CityComboBox.SelectedIndex > 0 ? CityComboBox.SelectedItem as string : null;
        string? country = CountryComboBox.SelectedIndex > 0 ? CountryComboBox.SelectedItem as string : null;

        _searchVm.Search(address, neighborhood, city, country);
        RefreshCards();
    }

    private void RefreshCards()
    {
        BuildingCards.ItemsSource = _searchVm.Buildings.Select(b => new BuildingCardVm(b)).ToList();
        int count = _searchVm.Buildings.Count;
        ResultCountText.Text = count == 1 ? "Pronađena 1 zgrada" : $"Pronađeno {count} zgrada";
    }

    private void RequestAccess_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: BuildingCardVm card })
        {
            var dlg = new BuildingAccessRequestForm(_userId, card.Building.Id, card.Building);
            dlg.Owner = System.Windows.Window.GetWindow(this);
            dlg.ShowDialog();
        }
    }

    private void PrevImage_Click(object sender, RoutedEventArgs e) { if (sender is Button { DataContext: BuildingCardVm vm }) vm.Prev(); }
    private void NextImage_Click(object sender, RoutedEventArgs e) { if (sender is Button { DataContext: BuildingCardVm vm }) vm.Next(); }

    private void RequestFiltersChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) ApplyRequestFilters();
    }

    private void ApplyRequestFilters()
    {
        var selectedItem = StatusFilterComboBox.SelectedItem as ComboBoxItem;
        string? tagValue = selectedItem?.Tag?.ToString();
        string filterStatus = string.IsNullOrEmpty(tagValue) ? "Svi" : tagValue;
        bool newestFirst = (SortComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() != "Najstariji prvo";
        _requestsVm.ApplyFilters(filterStatus, newestFirst);
    }

    private void CancelRequestButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: TenantAccessRequestDto dto })
        {
            var result = MessageBox.Show(
                "Da li ste sigurni da želite da otkažete zahtev?",
                "Potvrda", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _requestsVm.Cancel(dto.RequestId);
                _requestsVm.LoadRequests();
                ApplyRequestFilters();
            }
        }
    }
}
