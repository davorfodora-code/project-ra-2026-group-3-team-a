using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.BuildingManager;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager;

public partial class AccessRequestsWindow : Window
{
    private readonly AccessRequestsViewModel _viewModel;

    public AccessRequestsWindow(long managerId)
    {
        InitializeComponent();
        _viewModel = new AccessRequestsViewModel(managerId);
        LoadRequests();
    }

    // Učitava sve zahteve i primenjuje filter/sort
    private void LoadRequests()
    {
        _viewModel.LoadRequests();
        ApplyFilterAndSort();
    }

    // Primenjuje trenutni filter i sortiranje na listu zahteva
    private void ApplyFilterAndSort()
    {
        string status = GetSelectedStatus();
        bool newestFirst = GetSortNewestFirst();
        RequestsDataGrid.ItemsSource = _viewModel.GetFiltered(status, newestFirst);
    }

    // Čita odabrani status iz ComboBox-a
    private string GetSelectedStatus()
    {
        if (StatusFilterComboBox.SelectedItem is ComboBoxItem item)
            return item.Content.ToString() ?? "Svi";
        return "Svi";
    }

    // Čita smer sortiranja iz ComboBox-a
    private bool GetSortNewestFirst()
    {
        if (SortComboBox.SelectedItem is ComboBoxItem item)
            return item.Content.ToString() == "Najnoviji prvo";
        return true;
    }

    // Vraća selektovani zahtev ako je validan i ima status Pending, inače prikazuje grešku
    private BuildingAccessRequestDto? GetValidatedPendingRequest()
    {
        BuildingAccessRequestDto? selected = RequestsDataGrid.SelectedItem as BuildingAccessRequestDto;

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
        BuildingAccessRequestDto? selected = GetValidatedPendingRequest();
        if (selected == null) return;

        MessageBoxResult confirm = MessageBox.Show(
            $"Da li ste sigurni da želite da odobrite zahtev stanara {selected.TenantFullName} za stan {selected.ApartmentNumber}?",
            "Potvrda odobravanja",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            _viewModel.Approve(selected.RequestId);
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
        BuildingAccessRequestDto? selected = GetValidatedPendingRequest();
        if (selected == null) return;

        RejectReasonDialog dialog = new RejectReasonDialog();
        dialog.Owner = this;

        if (dialog.ShowDialog() != true) return;

        try
        {
            string? reason = string.IsNullOrWhiteSpace(dialog.Reason) ? null : dialog.Reason;
            _viewModel.Reject(selected.RequestId, reason);
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
        if (_viewModel != null)
            ApplyFilterAndSort();
    }

    // Kad se promeni sortiranje
    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel != null)
            ApplyFilterAndSort();
    }
}
