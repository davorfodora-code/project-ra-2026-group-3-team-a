using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Database.Repositories.Tenant;
using CommunityHub.Application.Domain;

namespace CommunityHub.Ui.Views.Tenant;

public partial class MyBuildingRequestsWindow : Window
{
    private readonly long _userId;
    private readonly BuildingAccessRequestDbRepository _requestRepository;
    private List<BuildingAccessRequest> _allRequests = new();

    public MyBuildingRequestsWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _requestRepository = new BuildingAccessRequestDbRepository();
        LoadRequests();
    }

    private void LoadRequests()
    {
        _allRequests = _requestRepository.GetAllByTenant(_userId);
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allRequests.AsEnumerable();

        // Filter po statusu
        if (StatusFilterComboBox.SelectedItem is ComboBoxItem statusItem)
        {
            string statusFilter = statusItem.Content.ToString()!;
            if (statusFilter != "Svi")
            {
                filtered = filtered.Where(r => r.Status == statusFilter);
            }
        }

        // Sortiranje po datumu
        if (SortComboBox.SelectedItem is ComboBoxItem sortItem)
        {
            string sortOption = sortItem.Content.ToString()!;
            filtered = sortOption == "Najstariji prvo"
                ? filtered.OrderBy(r => r.CreatedAt)
                : filtered.OrderByDescending(r => r.CreatedAt);
        }

        RequestsDataGrid.ItemsSource = filtered.Select(r => new RequestViewModel(r)).ToList();
    }

    private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) ApplyFilters();
    }

    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) ApplyFilters();
    }

    private void CancelRequestButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is RequestViewModel vm)
        {
            var result = MessageBox.Show("Da li ste sigurni da želite da otkažete zahtev?",
                "Potvrda otkazivanja", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _requestRepository.Delete(vm.RequestId);
                LoadRequests();
            }
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        TenantHomeWindow homeWindow = new TenantHomeWindow(_userId);
        homeWindow.Show();
        this.Close();
    }
}

public class RequestViewModel
{
    public long RequestId { get; }
    public string AddressDisplay { get; }
    public string Neighborhood { get; }
    public string LocationDisplay { get; }
    public int ApartmentNumber { get; }
    public DateTime CreatedAt { get; }
    public string Status { get; }
    public Visibility CancelButtonVisibility { get; }

    public RequestViewModel(BuildingAccessRequest request)
    {
        RequestId = request.Id;
        AddressDisplay = $"{request.Building.Address.Street} {request.Building.Address.StreetNumber}";
        Neighborhood = request.Building.Neighborhood;
        LocationDisplay = $"{request.Building.Location.City}, {request.Building.Location.Country}";
        ApartmentNumber = request.ApartmentNumber;
        CreatedAt = request.CreatedAt;
        Status = request.Status;
        CancelButtonVisibility = request.Status == "Pending" ? Visibility.Visible : Visibility.Collapsed;
    }
}
