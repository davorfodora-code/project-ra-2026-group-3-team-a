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
        IEnumerable<DistrictAccessRequest> sequence = _allRequests;
        sequence = ApplyStatusFilter(sequence);
        sequence = ApplySortOrder(sequence);

        RequestsDataGrid.ItemsSource = sequence
            .Select(r => new DistrictAccessRequestRowViewModel(r))
            .ToList();
    }

    private IEnumerable<DistrictAccessRequest> ApplyStatusFilter(IEnumerable<DistrictAccessRequest> requests)
    {
        if (StatusFilterComboBox.SelectedItem is not ComboBoxItem statusItem)
        {
            return requests;
        }

        string statusFilter = statusItem.Content?.ToString() ?? "Svi";
        if (statusFilter == "Svi")
        {
            return requests;
        }

        DistrictAccessRequestStatus statusEnum = Enum.Parse<DistrictAccessRequestStatus>(statusFilter);
        return requests.Where(r => r.Status == statusEnum);
    }

    private IEnumerable<DistrictAccessRequest> ApplySortOrder(IEnumerable<DistrictAccessRequest> requests)
    {
        if (SortComboBox.SelectedItem is not ComboBoxItem sortItem)
        {
            return requests;
        }

        string sortOption = sortItem.Content?.ToString() ?? "Najnoviji prvo";
        if (sortOption == "Najstariji prvo")
        {
            return requests.OrderBy(r => r.CreationDate);
        }

        return requests.OrderByDescending(r => r.CreationDate);
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

        if (!Enum.TryParse(vm.Status, out DistrictAccessRequestStatus status) || status != DistrictAccessRequestStatus.Pending)
        {
            MessageBox.Show("Možete obrisati samo zahteve u statusu Pending.", "Upozorenje",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        MessageBoxResult result = MessageBox.Show(
            "Da li ste sigurni da želite da obrišete zahtev?",
            "Potvrda brisanja zahteva",
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
