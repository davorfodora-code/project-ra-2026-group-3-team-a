using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Database.Repositories.Citizen;
using CommunityHub.Application.Domain;

namespace CommunityHub.Ui.Views.Citizen;

public partial class MyDistrictRequestsWindow : Window
{
    private readonly long _userId;
    private readonly CitizenDistrictAccessRequestDbRepository _requestRepository;
    private List<DistrictAccessRequest> _allRequests = new();

    public MyDistrictRequestsWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _requestRepository = new CitizenDistrictAccessRequestDbRepository();
        LoadRequests();
    }

    private void LoadRequests()
    {
        _allRequests = _requestRepository.GetAllByUserId(_userId);
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        IEnumerable<DistrictAccessRequest> filtered = _allRequests;

        if (StatusFilterComboBox.SelectedItem is ComboBoxItem statusItem)
        {
            string statusFilter = statusItem.Content.ToString() ?? "Svi";
            if (statusFilter != "Svi")
            {
                DistrictAccessRequestStatus statusEnum = Enum.Parse<DistrictAccessRequestStatus>(statusFilter);
                filtered = filtered.Where(r => r.Status == statusEnum);
            }
        }

        if (SortComboBox.SelectedItem is ComboBoxItem sortItem)
        {
            string sortOption = sortItem.Content.ToString() ?? "Najnoviji prvo";
            filtered = sortOption == "Najstariji prvo"
                ? filtered.OrderBy(r => r.CreationDate)
                : filtered.OrderByDescending(r => r.CreationDate);
        }

        RequestsDataGrid.ItemsSource = filtered.Select(r => new DistrictAccessRequestRowViewModel(r)).ToList();
    }

    private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) ApplyFilters();
    }

    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) ApplyFilters();
    }

    private void DeleteRequestButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not DistrictAccessRequestRowViewModel vm)
            return;

        if (vm.Status != DistrictAccessRequestStatus.Pending.ToString())
        {
            return;
        }

        MessageBoxResult result = MessageBox.Show(
            "Da li ste sigurni da želite da obrišete zahtev?",
            "Potvrda brisanja",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        _requestRepository.Delete(vm.RequestId);
        LoadRequests();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenHomeWindow homeWindow = new CitizenHomeWindow(_userId);
        homeWindow.Show();
        Close();
    }
}
