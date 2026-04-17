using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Database.Repositories.BuildingManager;
using CommunityHub.Application.Domain;

namespace CommunityHub.Ui.Views.BuildingManager;

// Pomoćna klasa za prikaz zahteva u DataGrid-u
public class AccessRequestRow
{
    public long RequestId { get; set; }
    public string TenantFullName { get; set; }
    public string BuildingAddress { get; set; }
    public string Neighborhood { get; set; }
    public string Location { get; set; }
    public int ApartmentNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedAtFormatted => CreatedAt.ToString("dd.MM.yyyy. HH:mm");
    public string Status { get; set; }

    public AccessRequestRow(BuildingAccessRequest request)
    {
        RequestId = request.Id;
        TenantFullName = $"{request.Tenant.Name} {request.Tenant.Surname}";
        BuildingAddress = $"{request.Building.Address.Street} {request.Building.Address.StreetNumber}";
        Neighborhood = request.Building.Neighborhood;
        Location = $"{request.Building.Location.City}, {request.Building.Location.Country}";
        ApartmentNumber = request.ApartmentNumber;
        CreatedAt = request.CreatedAt;
        Status = request.Status;
    }
}

public partial class AccessRequestsWindow : Window
{
    private readonly long _managerId;
    private readonly BuildingAccessRequestDbRepository _repository;
    private List<BuildingAccessRequest> _allRequests;

    public AccessRequestsWindow(long managerId)
    {
        InitializeComponent();
        _managerId = managerId;
        _repository = new BuildingAccessRequestDbRepository();
        _allRequests = new List<BuildingAccessRequest>();

        LoadRequests();
    }

    // Učitava sve zahteve iz baze i primenjuje filter/sort
    private void LoadRequests()
    {
        _allRequests = _repository.GetByManagerId(_managerId);
        ApplyFilterAndSort();
    }

    // Primenjuje trenutni filter i sortiranje na listu zahteva
    private void ApplyFilterAndSort()
    {
        IEnumerable<BuildingAccessRequest> filtered = _allRequests;

        // Filter po statusu
        string selectedStatus = GetSelectedStatus();
        if (selectedStatus != "Svi")
        {
            filtered = filtered.Where(r => r.Status == selectedStatus);
        }

        // Sortiranje po datumu
        bool newestFirst = GetSortNewestFirst();
        filtered = newestFirst
            ? filtered.OrderByDescending(r => r.CreatedAt)
            : filtered.OrderBy(r => r.CreatedAt);

        // Mapiraj u redove za prikaz
        RequestsDataGrid.ItemsSource = filtered.Select(r => new AccessRequestRow(r)).ToList();
    }

    // Čita odabrani status iz ComboBox-a
    private string GetSelectedStatus()
    {
        if (StatusFilterComboBox.SelectedItem is ComboBoxItem item)
        {
            return item.Content.ToString() ?? "Svi";
        }
        return "Svi";
    }

    // Čita smer sortiranja iz ComboBox-a
    private bool GetSortNewestFirst()
    {
        if (SortComboBox.SelectedItem is ComboBoxItem item)
        {
            return item.Content.ToString() == "Najnoviji prvo";
        }
        return true;
    }

    // Vraća selektovani zahtev ako je validan i ima status Pending, inače prikazuje grešku
    private AccessRequestRow? GetValidatedPendingRequest()
    {
        AccessRequestRow? selected = RequestsDataGrid.SelectedItem as AccessRequestRow;

        if (selected == null)
        {
            MessageBox.Show("Morate selektovati zahtev.", "Upozorenje",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        if (selected.Status != "Pending")
        {
            MessageBox.Show("Akcija je moguća samo za zahteve sa statusom 'Pending'.", "Upozorenje",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        return selected;
    }

    // Odobrava selektovani zahtev
    private void ApproveButton_Click(object sender, RoutedEventArgs e)
    {
        AccessRequestRow? selected = GetValidatedPendingRequest();
        if (selected == null) return;

        MessageBoxResult confirm = MessageBox.Show(
            $"Da li ste sigurni da želite da odobrite zahtev stanara {selected.TenantFullName} za stan {selected.ApartmentNumber}?",
            "Potvrda odobravanja",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            _repository.Approve(selected.RequestId);
            MessageBox.Show("Zahtev je odobren i članstvo je kreirano.", "Uspeh",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadRequests();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Greška prilikom odobravanja: {ex.Message}", "Greška",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Odbija selektovani zahtev sa opcionim obrazloženjem
    private void RejectButton_Click(object sender, RoutedEventArgs e)
    {
        AccessRequestRow? selected = GetValidatedPendingRequest();
        if (selected == null) return;

        RejectReasonDialog dialog = new RejectReasonDialog();
        dialog.Owner = this;

        if (dialog.ShowDialog() != true) return;

        try
        {
            string? reason = string.IsNullOrWhiteSpace(dialog.Reason) ? null : dialog.Reason;
            _repository.Reject(selected.RequestId, reason);
            MessageBox.Show("Zahtev je odbijen.", "Uspeh",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadRequests();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Greška prilikom odbijanja: {ex.Message}", "Greška",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Kad se promeni filter
    private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_allRequests != null)
        {
            ApplyFilterAndSort();
        }
    }

    // Kad se promeni sortiranje
    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_allRequests != null)
        {
            ApplyFilterAndSort();
        }
    }
}
