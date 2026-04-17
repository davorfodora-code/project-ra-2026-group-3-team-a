using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Database.Repositories.DistrictCoordinator;
using CommunityHub.Application.Domain;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public class DistrictAccessRequestRow
{
    public long RequestId { get; set; }
    public string CitizenFullName { get; set; }
    public string CitizenAddress { get; set; }
    public string DistrictName { get; set; }
    public string Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedAtFormatted => CreatedAt.ToString("dd.MM.yyyy. HH:mm");
    public string Status { get; set; }

    public DistrictAccessRequestRow(DistrictAccessRequest request)
    {
        RequestId = request.Id;
        CitizenFullName = $"{request.User.Name} {request.User.Surname}";
        CitizenAddress = request.User.Address ?? "Nije navedena";
        DistrictName = request.District.Name;
        Location = $"{request.District.Location.City}, {request.District.Location.Country}";
        CreatedAt = request.CreationDate;
        Status = request.Status.ToString();
    }
}

public partial class AccessRequestsWindow : Window
{
    private readonly long _coordinatorId;
    private readonly DistrictAccessRequestDbRepository _repository;
    private List<DistrictAccessRequest> _allRequests;

    public AccessRequestsWindow(long coordinatorId)
    {
        InitializeComponent();
        _coordinatorId = coordinatorId;
        _repository = new DistrictAccessRequestDbRepository();
        _allRequests = new List<DistrictAccessRequest>();

        LoadRequests();
    }

    private void LoadRequests()
    {
        _allRequests = _repository.GetByCoordinatorId(_coordinatorId);
        ApplyFilterAndSort();
    }

    private void ApplyFilterAndSort()
    {
        IEnumerable<DistrictAccessRequest> filtered = _allRequests;

        string selectedStatus = GetSelectedStatus();
        if (selectedStatus != "Svi")
        {
            filtered = filtered.Where(r => r.Status.ToString() == selectedStatus);
        }

        bool newestFirst = GetSortNewestFirst();
        filtered = newestFirst
            ? filtered.OrderByDescending(r => r.CreationDate)
            : filtered.OrderBy(r => r.CreationDate);

        RequestsDataGrid.ItemsSource = filtered.Select(r => new DistrictAccessRequestRow(r)).ToList();
    }

    private string GetSelectedStatus()
    {
        if (StatusFilterComboBox.SelectedItem is ComboBoxItem item)
        {
            return item.Content.ToString() ?? "Svi";
        }
        return "Svi";
    }

    private bool GetSortNewestFirst()
    {
        if (SortComboBox.SelectedItem is ComboBoxItem item)
        {
            return item.Content.ToString() == "Najnoviji prvo";
        }
        return true;
    }

    private void ApproveButton_Click(object sender, RoutedEventArgs e)
    {
        DistrictAccessRequestRow? selected = RequestsDataGrid.SelectedItem as DistrictAccessRequestRow;
        if (!ValidatePendingSelection(selected, "odobriti")) return;
        if (!ConfirmApproval(selected!)) return;

        TryApproveRequest(selected!);
    }

    private void RejectButton_Click(object sender, RoutedEventArgs e)
    {
        DistrictAccessRequestRow? selected = RequestsDataGrid.SelectedItem as DistrictAccessRequestRow;
        if (!ValidatePendingSelection(selected, "odbiti")) return;
        if (!TryGetRejectionReason(out string? reason)) return;

        TryRejectRequest(selected!, reason);
    }

    private bool ValidatePendingSelection(DistrictAccessRequestRow? selected, string actionLabel)
    {
        if (selected == null)
        {
            ShowWarning("Morate selektovati zahtev.");
            return false;
        }
        if (selected.Status != "Pending")
        {
            ShowWarning($"Mozete {actionLabel} samo zahteve sa statusom 'Pending'.");
            return false;
        }
        return true;
    }

    private bool ConfirmApproval(DistrictAccessRequestRow selected)
    {
        MessageBoxResult result = MessageBox.Show(
            $"Da li ste sigurni da zelite da odobrite zahtev gradjanina {selected.CitizenFullName} za kvart {selected.DistrictName}?",
            "Potvrda odobravanja",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    private bool TryGetRejectionReason(out string? reason)
    {
        reason = null;
        RejectReasonDialog dialog = new RejectReasonDialog { Owner = this };
        if (dialog.ShowDialog() != true) return false;
        reason = string.IsNullOrWhiteSpace(dialog.Reason) ? null : dialog.Reason;
        return true;
    }

    private void TryApproveRequest(DistrictAccessRequestRow selected)
    {
        try
        {
            _repository.Approve(selected.RequestId);
            ShowInfo("Zahtev je odobren i clanstvo je kreirano.");
            LoadRequests();
        }
        catch (Exception ex)
        {
            ShowError($"Greska prilikom odobravanja: {ex.Message}");
        }
    }

    private void TryRejectRequest(DistrictAccessRequestRow selected, string? reason)
    {
        try
        {
            _repository.Reject(selected.RequestId, reason);
            ShowInfo("Zahtev je odbijen.");
            LoadRequests();
        }
        catch (Exception ex)
        {
            ShowError($"Greska prilikom odbijanja: {ex.Message}");
        }
    }

    private static void ShowWarning(string message)
    {
        MessageBox.Show(message, "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private static void ShowInfo(string message)
    {
        MessageBox.Show(message, "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(message, "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_allRequests != null)
        {
            ApplyFilterAndSort();
        }
    }

    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_allRequests != null)
        {
            ApplyFilterAndSort();
        }
    }
}
